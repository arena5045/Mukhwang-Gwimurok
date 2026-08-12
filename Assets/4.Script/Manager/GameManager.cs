using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Main 씬에서 선택한 값을 Ingame 씬이 생성될 때까지 임시 보관한다.
    // 실제 플레이 데이터는 이 정적 필드가 아니라 BeginNewRun에서 만든 GameContext가 소유한다.
    private static CharacterData pendingCharacter;
    private static DungeonData pendingDungeon;

    // 한 번의 런에서만 유효한 플레이어·맵·진행 상태의 최상위 컨테이너다.
    // 새 런이 시작되면 기존 객체를 수정하지 않고 새 GameContext로 통째로 교체한다.
    public GameContext Context { get; private set; }

    [Tooltip("Main 씬에서 던전을 선택하지 않았거나 Ingame 씬을 직접 실행했을 때 사용할 기본 던전입니다.")]
    public DungeonData testDungeon;

    [Header("새 런 기본 능력치")]
    [SerializeField] private int initialMaxHp = 100;
    [SerializeField] private int initialMaxMp = 100;
    [SerializeField] private float initialAdAttack = 15f;

    public bool canClick = false;
    public bool isGameOver= false;

    [Header("인게임 매니저들")]
    public MapManager mapManager { get; private set; }

    public enum goodsType {gold,soul,count}

    private Coroutine runIntroCoroutine;

    /// <summary>
    /// Unity 에디터에서 도메인 재로드 옵션을 꺼 둔 경우에도 정적 상태를 초기화한다.
    /// 이전 Play 세션의 선택 캐릭터나 던전이 다음 실행으로 넘어가는 것을 방지한다.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        // 에디터의 도메인 재로드가 꺼져 있어도 이전 실행의 선택값을 남기지 않는다.
        Instance = null;
        pendingCharacter = null;
        pendingDungeon = null;
    }

    /// <summary>
    /// Main 씬에서 선택한 새 런 설정을 Ingame 씬으로 전달한다.
    /// 씬 객체를 직접 보존하지 않고 ScriptableObject 선택값만 잠시 보관하며,
    /// RegisterMap이 값을 소비한 직후 정적 참조를 비운다.
    /// </summary>
    public static void RequestNewRun(CharacterData character, DungeonData dungeon)
    {
        // 씬 전환 전에 선택값만 보관하고, 실제 런 상태 생성은 Ingame에서 수행한다.
        pendingCharacter = character;
        pendingDungeon = dungeon;
    }

    private bool TryBeginContentTransition()
    {
        // canClick과 페이드 상태를 같은 지점에서 검사해야 빠른 연속 터치가
        // 서로 다른 콘텐츠 코루틴을 동시에 시작시키지 않는다.
        if (!canClick || GameUiManager.Instance == null || GameUiManager.Instance.IsFading)
        {
            return false;
        }

        canClick = false;
        return true;
    }

    private void EndContentTransition()
    {
        canClick = true;
    }

    private void Awake()
    {
        // GameManager는 Ingame 씬 전용이다. 씬을 나가면 Context도 함께 폐기되며,
        // 다음 Ingame 진입에서는 새 인스턴스가 새 런을 구성한다.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 현재 런은 Ingame 씬 안에서 완결되므로 씬 전환 시 새 인스턴스로 교체한다.
        Instance = this;
    }

    private void OnDestroy()
    {
        // 중복 인스턴스가 파괴될 때 정상 인스턴스까지 지우지 않도록 소유권을 확인한다.
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// MapManager가 절차 맵 데이터 생성을 끝낸 뒤 호출하는 1차 등록 단계다.
    /// 여기서 새 런을 먼저 만들고, 그 Context 안에 맵 데이터를 연결한다.
    /// UI 노드 생성은 아직 끝나지 않았으므로 이 메서드에서는 입력을 열지 않는다.
    /// </summary>
    public bool RegisterMap(
        MapManager manager,
        List<List<Vector2Int>> mapData,
        List<MapNodeData> nodeData,
        Dictionary<int, MapNodeData> nodeIds)
    {
        if (manager == null || mapData == null || nodeData == null || nodeIds == null)
        {
            Debug.LogError("[GameManager] 맵 등록에 필요한 데이터가 없습니다.");
            return false;
        }

        // Main 씬의 선택값을 한 번만 소비한다. Ingame 씬을 직접 실행한 경우에는
        // 캐릭터 기본값과 Inspector의 testDungeon을 사용한다.
        CharacterData requestedCharacter = pendingCharacter;
        DungeonData requestedDungeon = pendingDungeon != null ? pendingDungeon : testDungeon;
        pendingCharacter = null;
        pendingDungeon = null;

        // 맵 씬이 준비되는 시점을 새 런의 유일한 시작점으로 사용한다.
        if (!BeginNewRun(requestedCharacter, requestedDungeon))
        {
            return false;
        }

        mapManager = manager;
        Context.maps = mapData;
        Context.map_nodes = nodeData;
        Context.map_nodes_id = nodeIds;
        return true;
    }

    /// <summary>
    /// 맵 데이터 등록, UI 노드 생성, 시작 노드 표시가 모두 끝났음을 알리는 2차 단계다.
    /// 이 시점부터 UI 갱신과 시작 이벤트 연출을 안전하게 실행할 수 있다.
    /// </summary>
    public void NotifyMapReady(MapManager manager)
    {
        if (manager == null || manager != mapManager || Context == null)
        {
            Debug.LogError("[GameManager] 등록되지 않은 맵에서 준비 완료 신호를 받았습니다.");
            return;
        }

        RefreshPlayerUi();

        if (runIntroCoroutine != null)
        {
            StopCoroutine(runIntroCoroutine);
        }

        runIntroCoroutine = StartCoroutine(StartRunIntro());
    }

    /// <summary>
    /// Ingame 씬 종료 중 MapManager와의 연결을 해제한다.
    /// 파괴 순서와 무관하게 Context가 이미 파괴된 UI 노드와 GameObject를 참조하지 않게 한다.
    /// </summary>
    public void UnregisterMap(MapManager manager)
    {
        if (manager == null || manager != mapManager) return;

        if (runIntroCoroutine != null)
        {
            StopCoroutine(runIntroCoroutine);
            runIntroCoroutine = null;
        }

        // 맵이 먼저 파괴되더라도 남은 런 상태가 씬 객체를 가리키지 않게 한다.
        mapManager = null;
        canClick = false;

        if (Context != null)
        {
            Context.maps = null;
            Context.map_nodes = null;
            Context.map_nodes_id = null;
            Context.cur_node = null;
            Context.cur_node_Data = null;
        }
    }

    /// <summary>
    /// 새 런의 모든 런타임 상태를 생성하는 유일한 진입점이다.
    /// 플레이어 능력치, 진행도, 입력 상태, 시간 배율, 이벤트 풀과 전투 임시 상태를 함께 초기화한다.
    /// 일부 값만 이전 런에서 남는 문제를 피하기 위해 기존 Context를 재사용하지 않는다.
    /// </summary>
    public bool BeginNewRun(CharacterData character, DungeonData dungeon)
    {
        if (dungeon == null)
        {
            Debug.LogError("[GameManager] 새 런을 시작할 던전이 지정되지 않았습니다.");
            return false;
        }

        // 선택 캐릭터에 유효한 시작 수치가 있으면 우선 사용하고,
        // 데이터가 없거나 0 이하이면 GameManager의 기본 능력치로 대체한다.
        var player = new PlayerData();
        player.stats.MaxHp = character != null && character.startHp > 0
            ? character.startHp
            : initialMaxHp;
        player.stats.MaxMp = initialMaxMp;
        player.stats.baseAdAttack = character != null && character.startAtk > 0
            ? character.startAtk
            : initialAdAttack;
        player.Initialize();

        // 새 런에서 이어지면 안 되는 모든 런타임 상태를 한 번에 교체한다.
        Context = new GameContext
        {
            player = player,
            character = character,
            cur_dungeon = dungeon,
            currentDay = 0,
            currentFloor = 0
        };

        isGameOver = false;
        canClick = false;
        Time.timeScale = 1f;

        EventManager.Instance?.ResetForNewRun();
        BattleManager.instance?.ResetForNewRun();
        return true;
    }

    /// <summary>
    /// 플레이어의 실제 HP/MP 데이터를 최대치로 회복한 뒤 UI를 갱신한다.
    /// 현재는 '전투 종료 후 완전 회복' 규칙에서만 호출한다.
    /// </summary>
    public void RestorePlayerVitals()
    {
        if (Context?.player == null) return;

        // 현재 게임 규칙인 '전투 종료 후 완전 회복'을 명시적으로 표현한다.
        Context.player.Initialize();
        RefreshPlayerUi();
    }

    /// <summary>
    /// 상태값은 변경하지 않고 현재 GameContext의 값을 UI에 다시 표시한다.
    /// 데이터 초기화와 화면 갱신을 분리하기 위한 메서드다.
    /// </summary>
    public void RefreshPlayerUi()
    {
        if (Context?.player == null || GameUiManager.Instance == null) return;

        GameUiManager.Instance.RefreshUi();
    }

    private IEnumerator StartRunIntro()
    {
        // UI 노드가 생성된 같은 프레임에 페이드를 시작하면 RectTransform 레이아웃과
        // 입력 상태가 아직 확정되지 않을 수 있으므로 프레임 끝까지 기다린다.
        yield return new WaitForEndOfFrame();
        runIntroCoroutine = null;

        if (mapManager == null || Context?.cur_dungeon == null) yield break;

        // StartEvent가 전환 잠금을 획득할 수 있는 순간에만 입력을 잠시 연다.
        canClick = true;
        StartEvent(Context.cur_dungeon.Call_StartEvent());
    }

    public bool ChangeGold(int amount)
    {
        // 소모하려는 양이 현재 골드보다 많으면 (돈 부족)
        if (amount < 0 && Context.player.gold < Mathf.Abs(amount))
        {
            Debug.Log("골드가 부족합니다!");
            return false; // 처리에 실패했다고 알려줌
        }

        // 실제 골드 반영
        Context.player.gold += amount;

        // 혹시 모를 상황을 대비한 최솟값 방어 (0 이하로 안 내려가게)
        Context.player.gold = Mathf.Max(0, Context.player.gold);

        // UI 갱신
        GameUiManager.Instance.UpdateGoldUi();

        return true; // 성공적으로 변경됨
    }

    public bool CanBuyGold(int price) => Context.player.gold >= price;

    public bool CanBuySoul(int price) => Context.player.soul >= price;

    public string RareString(Rarity rarity)
    {
        string rare = "";

        switch (rarity)
        {
            case Rarity.Common: rare = "일반"; break;
            case Rarity.Rare: rare = "희귀"; break;
            case Rarity.Epic: rare = "영웅"; break;
            case Rarity.Legendary: rare = "전설"; break;
        }

        return rare;
    }

    public bool ChangeSoul(int amount)
    {
        // 소모하려는 양이 현재 골드보다 많으면 (돈 부족)
        if (amount < 0 && Context.player.soul < Mathf.Abs(amount))
        {
            Debug.Log("혼백이 부족합니다!");
            return false; // 처리에 실패했다고 알려줌
        }

        // 실제 골드 반영
        Context.player.soul += amount;

        // 혹시 모를 상황을 대비한 최솟값 방어 (0 이하로 안 내려가게)
        Context.player.soul = Mathf.Max(0, Context.player.soul);

        // UI 갱신
        GameUiManager.Instance.UpdateSoulUi();

        return true; // 성공적으로 변경됨
    }


    //배틀버튼 눌렀을 때 콜되는 함수
    public void Button_Battle()
    {
        // UI 매니저가 이미 페이드 중이라면 함수를 종료 (중복 클릭 방지)
        if (!TryBeginContentTransition()) return;

        Context.currentDay++;
        StartCoroutine(BattleStartCoroutine());
    }

    //배틀 시작할때 이동되는 코루틴
    public IEnumerator BattleStartCoroutine()
    {

        // 1. 화면 덮기
        yield return GameUiManager.Instance.FadeIn();

        // 2. 환경 설정

        GameUiManager.Instance.AllPanelOff();
        Monster_So monster = Context?.cur_dungeon?.GetRandomMonster();
        if (monster == null)
        {
            GameUiManager.Instance.MapUiOpen(false);
            yield return GameUiManager.Instance.FadeOut();
            EndContentTransition();
            yield break;
        }

        BattleManager.instance.BattleStart(monster);
        //맵 노드 이동
        GameUiManager.Instance.MapNodeMove();
        // 3. 잠시 대기 (너무 빠르면 깜빡이는 느낌이 들 수 있음)
        yield return new WaitForSeconds(0.1f);

        // 4. 화면 치우기
        yield return GameUiManager.Instance.FadeOut();
        EndContentTransition();
    }

    //배틀버튼 눌렀을 때 콜되는 함수
    public void Button_Battle_Elite()
    {
        // UI 매니저가 이미 페이드 중이라면 함수를 종료 (중복 클릭 방지)
        if (!TryBeginContentTransition()) return;

        Context.currentDay++;
        StartCoroutine(Battle_Elite_StartCoroutine());
    }

    //네임드 배틀 시작할때 이동되는 코루틴
    public IEnumerator Battle_Elite_StartCoroutine()
    {
        // 1. 화면 덮기
        yield return GameUiManager.Instance.FadeIn();

        // 2. 환경 설정
        GameUiManager.Instance.AllPanelOff();
        Monster_So monster = Context?.cur_dungeon?.GetRandomMonster_Elite();
        if (monster == null)
        {
            GameUiManager.Instance.MapUiOpen(false);
            yield return GameUiManager.Instance.FadeOut();
            EndContentTransition();
            yield break;
        }

        BattleManager.instance.BattleStart(monster);
        //맵 노드 이동
        GameUiManager.Instance.MapNodeMove();
        // 3. 잠시 대기 (너무 빠르면 깜빡이는 느낌이 들 수 있음)
        yield return new WaitForSeconds(0.1f);

        // 4. 화면 치우기
        yield return GameUiManager.Instance.FadeOut();
        EndContentTransition();
    }

    //배틀버튼 눌렀을 때 콜되는 함수
    public void Button_Battle_Boss()
    {
        // UI 매니저가 이미 페이드 중이라면 함수를 종료 (중복 클릭 방지)
        if (!TryBeginContentTransition()) return;

        Context.currentDay++;
        StartCoroutine(Battle_Boss_StartCoroutine());
    }

    //보스 배틀 시작할때 이동되는 코루틴
    public IEnumerator Battle_Boss_StartCoroutine()
    {
        // 1. 화면 덮기
        yield return GameUiManager.Instance.FadeIn();

        // 2. 환경 설정
        GameUiManager.Instance.AllPanelOff();
        Monster_So monster = Context?.cur_dungeon?.GetRandomMonster_Boss();
        if (monster == null)
        {
            GameUiManager.Instance.MapUiOpen(false);
            yield return GameUiManager.Instance.FadeOut();
            EndContentTransition();
            yield break;
        }

        BattleManager.instance.BattleStart(monster);
        //맵 노드 이동
        GameUiManager.Instance.MapNodeMove();
        // 3. 잠시 대기 (너무 빠르면 깜빡이는 느낌이 들 수 있음)
        yield return new WaitForSeconds(0.1f);

        // 4. 화면 치우기
        yield return GameUiManager.Instance.FadeOut();
        EndContentTransition();
    }

    public void Button_Shop()
    {
        // UI 매니저가 이미 페이드 중이라면 함수를 종료 (중복 클릭 방지)
        if (!TryBeginContentTransition()) return;

        Context.currentDay++;
        StartCoroutine(ShopStartCoroutine());
    }

    //샵 시작할때 이동되는 코루틴
    public IEnumerator ShopStartCoroutine()
    {
        // 1. 화면 덮기
        yield return GameUiManager.Instance.FadeIn();

        // 2. 환경 설정
        GameUiManager.Instance.AllPanelOff();
        GameUiManager.Instance.ShopUiOpen();
        //맵 노드 이동
        GameUiManager.Instance.MapNodeMove();
        // 3. 잠시 대기 (너무 빠르면 깜빡이는 느낌이 들 수 있음)
        yield return new WaitForSeconds(0.1f);

        // 4. 화면 치우기
        yield return GameUiManager.Instance.FadeOut();
        EndContentTransition();
    }

    public void Button_Event()
    {
        // UI 매니저가 이미 페이드 중이라면 함수를 종료 (중복 클릭 방지)
        if (!TryBeginContentTransition()) return;

        Context.currentDay++;
        StartCoroutine(EventStartCoroutine(null));
    }
    
    public void StartEvent(DialogueSequence eventDialouge)
    {
        if (!TryBeginContentTransition()) return;

        Debug.Log("이벤트 클릭");

        StartCoroutine(EventStartCoroutine(eventDialouge));
    }

    //이벤트 코루틴
    public IEnumerator EventStartCoroutine(DialogueSequence eventDialouge)
    {
        // 1. 화면 덮기
        yield return GameUiManager.Instance.FadeIn();

        // 2. 환경 설정
        GameUiManager.Instance.AllPanelOff();


        if(eventDialouge == null)
        {
            eventDialouge = EventManager.Instance.PickEvent();
        }

        if (eventDialouge == null)
        {
            GameUiManager.Instance.MapUiOpen(false);
            yield return GameUiManager.Instance.FadeOut();
            EndContentTransition();
            yield break;
        }

        DialogueManager.Instance.StartDialogue(eventDialouge);

        //맵 노드 이동
        if (Context.cur_node_Data != null)
            GameUiManager.Instance.MapNodeMove();


        // 3. 잠시 대기 (너무 빠르면 깜빡이는 느낌이 들 수 있음)
        yield return new WaitForSeconds(0.1f);

        // 4. 화면 치우기
        yield return GameUiManager.Instance.FadeOut();
        EndContentTransition();
    }


    public IEnumerator GameOverCoroutine()
    {
        // 1. 화면 덮기
        yield return GameUiManager.Instance.FadeIn();

        // 2. 환경 설정
        GameUiManager.Instance.AllPanelOff();
        GameUiManager.Instance.GameOver();
        // 3. 잠시 대기 (너무 빠르면 깜빡이는 느낌이 들 수 있음)
        yield return new WaitForSeconds(0.1f);

        // 4. 화면 치우기
        yield return GameUiManager.Instance.FadeOut();
    }

    //겜 초기화면으로 돌아감
    public void GameOut()
    {
        StartCoroutine(GameOutCouroutine());
    }
    public IEnumerator GameOutCouroutine()
    {
        // 1. 화면 덮기
        yield return GameUiManager.Instance.FadeIn();

        // 2. 환경 설정
        DOTween.KillAll();
        SceneManager.LoadScene("Main");
        // 3. 잠시 대기 (너무 빠르면 깜빡이는 느낌이 들 수 있음)
        yield return new WaitForSeconds(0.1f);

    }
}
