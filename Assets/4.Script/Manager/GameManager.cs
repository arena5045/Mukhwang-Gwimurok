using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameContext Context { get; private set; }

    public DungeonData testDungeon;

    public bool canClick = true;
    public bool isGameOver= false;

    [Header("인게임 매니저들")]
    public MapManager mapManager { get; set; }

    public enum goodsType {gold,soul,count}
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeGame();
    }

    private void Start()
    {
        Refresh_HpMp();
        Debug.Log("씬이동후 페이드 실행");
        // Start에서 바로 하지 말고 코루틴 호출
        StartCoroutine(StartAnimationRoutine());

        //StartEvent(Context.cur_dungeon.Call_StartEvent());
    }

    void InitializeGame()
    {
        Context = new GameContext();
        Context.player = new PlayerData();           // <- 여기서 생성

        // 아이템, 맵 상태, 초기 노드 등도 여기서 설정 가능
        Context.cur_dungeon = testDungeon;

        Context.player.stats.MaxHp = 100;
        Context.player.stats.MaxMp = 100;
        Context.player.stats.baseAdAttack = 15;
    }

    IEnumerator StartAnimationRoutine()
    {
        // 1. 유니티가 화면을 완전히 한 번 그릴 때까지 대기 (렉 방지)
        yield return new WaitForEndOfFrame();

        // 2. 혹은 확실하게 0.1~0.2초 정도 여유를 줌
        // yield return new WaitForSeconds(0.1f);

        // 3. 이제 연출 시작
        StartEvent(Context.cur_dungeon.Call_StartEvent());
    }

    public void SetMap(List<List<Vector2Int>> mapData, List<MapNodeData> nodeData, Dictionary<int, MapNodeData> nodeids)
    {
        Context.maps = mapData;
        Context.map_nodes = nodeData;
        Context.map_nodes_id = nodeids;
        // 예: 저장 로그나 후처리
    }

    public void Refresh_HpMp()
    {
        Context.player.currentHP = Context.player.stats.MaxHp;
        Context.player.currentMp = Context.player.stats.MaxMp;

        GameUiManager.Instance.RefreshUi();
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
        if (GameUiManager.Instance.IsFading) return;

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
        BattleManager.instance.BattleStart(Context.cur_dungeon.GetRandomMonster());
        //맵 노드 이동
        GameUiManager.Instance.MapNodeMove();
        // 3. 잠시 대기 (너무 빠르면 깜빡이는 느낌이 들 수 있음)
        yield return new WaitForSeconds(0.1f);

        // 4. 화면 치우기
        yield return GameUiManager.Instance.FadeOut();
    }

    //배틀버튼 눌렀을 때 콜되는 함수
    public void Button_Battle_Elite()
    {
        // UI 매니저가 이미 페이드 중이라면 함수를 종료 (중복 클릭 방지)
        if (GameUiManager.Instance.IsFading) return;

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
        BattleManager.instance.BattleStart(Context.cur_dungeon.GetRandomMonster_Elite());
        //맵 노드 이동
        GameUiManager.Instance.MapNodeMove();
        // 3. 잠시 대기 (너무 빠르면 깜빡이는 느낌이 들 수 있음)
        yield return new WaitForSeconds(0.1f);

        // 4. 화면 치우기
        yield return GameUiManager.Instance.FadeOut();
    }

    //배틀버튼 눌렀을 때 콜되는 함수
    public void Button_Battle_Boss()
    {
        // UI 매니저가 이미 페이드 중이라면 함수를 종료 (중복 클릭 방지)
        if (GameUiManager.Instance.IsFading) return;

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
        BattleManager.instance.BattleStart(Context.cur_dungeon.GetRandomMonster_Boss());
        //맵 노드 이동
        GameUiManager.Instance.MapNodeMove();
        // 3. 잠시 대기 (너무 빠르면 깜빡이는 느낌이 들 수 있음)
        yield return new WaitForSeconds(0.1f);

        // 4. 화면 치우기
        yield return GameUiManager.Instance.FadeOut();
    }

    public void Button_Shop()
    {
        // UI 매니저가 이미 페이드 중이라면 함수를 종료 (중복 클릭 방지)
        if (GameUiManager.Instance.IsFading) return;

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
    }

    public void Button_Event()
    {
        // UI 매니저가 이미 페이드 중이라면 함수를 종료 (중복 클릭 방지)
        if (GameUiManager.Instance.IsFading) return;

        Context.currentDay++;
        StartCoroutine(EventStartCoroutine(null));
    }
    
    public void StartEvent(DialogueSequence eventDialouge)
    {
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
        DialogueManager.Instance.StartDialogue(eventDialouge);

        //맵 노드 이동
        if (Context.cur_node_Data != null)
            GameUiManager.Instance.MapNodeMove();


        // 3. 잠시 대기 (너무 빠르면 깜빡이는 느낌이 들 수 있음)
        yield return new WaitForSeconds(0.1f);

        // 4. 화면 치우기
        yield return GameUiManager.Instance.FadeOut();
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