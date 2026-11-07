using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BattleManager;

public class BattleUiManager : MonoBehaviour
{
    public GameObject battlePannel;

    public Image monsterSprite;
    private Vector3 originPosition; // 원래 위치 저장용
    private Color originColor;      // 원래 색상 저장용

    public TMP_Text monsterName;

    public TMP_Text monsterHpText;
    public Image monsterHpbar;

    public Transform logParent;      // Scroll View의 Content
    public GameObject logPrefab;     // 로그 한 줄 프리팹
    public ScrollRect scrollRect;    // 스크롤 뷰 컴포넌트
    public float logSpeed = 0.3f;    // 한 줄씩 나오는 속도

    [Header("전투 진행 화면")]
    [SerializeField] private GameObject actionMenuPanel; //공격 메뉴 패널
    [SerializeField] private GameObject battleLogPanel; //배틀 로그 패널

    //적기본크기
    private Vector3 monsterBaseScale;
    //적 트윈 애니메이션
    private Tween monsterAttackScaleTween;


    [Header("행동력 UI")]
    [SerializeField] private TMP_Text actionPointText;


    [Header("스킬 선택 UI")]
    [SerializeField] private BattleSkillSelectUI skillSelectUI;

    [Header("적의 기척 UI")]
    [SerializeField] private TMP_Text enemyActionNameText;
    [SerializeField] private Transform enemyEffectContent;
    [SerializeField] private EnemyIntentEffectUI enemyEffectPrefab;
    //적의 버프/디버프를 표시할 ui
    [SerializeField] private Transform enemyStatusContent;
    // 현재 적 상태창에 표시 중인 슬롯
    private readonly List<BattleStatusSlotUI>
        activeEnemyStatusSlots = new();

    // 사용하지 않고 대기 중인 적 상태 슬롯
    private readonly Queue<BattleStatusSlotUI>
        inactiveEnemyStatusSlots = new();

    [Header("플레이어 상태 UI")]
    [SerializeField] private Transform playerStatusContent;
    [SerializeField] private BattleStatusSlotUI statusSlotPrefab;
    [SerializeField] private int initialStatusPoolSize = 10;
    // 현재 화면에 표시 중인 슬롯
    private readonly List<BattleStatusSlotUI>
        activePlayerStatusSlots = new();

    // 사용하지 않고 대기 중인 슬롯
    private readonly Queue<BattleStatusSlotUI>
        inactivePlayerStatusSlots = new();


    [Header("적 행동 아이콘")]
    [SerializeField] private Sprite attackIcon;
    [SerializeField] private Sprite gainAttackIcon;
    [SerializeField] private Sprite gainDefenseIcon;

    [Header("버튼들")]
    public GameObject btn_battle;
    public GameObject btn_talk;
    public GameObject btn_run;

    [Header("배틀종료버튼")]
    public GameObject battle_end_panel;
    // 전투 종료 버튼의 연속 입력으로 맵 복귀와 게임오버 코루틴이 중복 시작되는 것을 방지한다.
    private bool isExitingBattle;

    // 피격 색상 Tween이 겹치지 않도록 현재 Tween을 기억
    private Tween monsterDamageColorTween;


    // 외부(BattleManager 등)에서 이 함수를 호출해 로그를 쌓습니다.

    /*
public void AddLog(string message)
{
    // 1. 즉시 로그 생성 (밀리지 않게)
    GameObject newLog = Instantiate(logPrefab, logParent);
    newLog.GetComponent<TextMeshProUGUI>().text = message;

    // 2. 개수 제한 (메모리 관리)
    if (logParent.childCount > 50)
    {
        Destroy(logParent.GetChild(0).gameObject);
    }

    // 3. 스크롤 하단 이동은 코루틴으로 안전하게!
    StartCoroutine(ScrollToBottom());
}

private IEnumerator ScrollToBottom()
{
    // 한 프레임 기다려서 유니티가 UI 크기를 계산할 시간을 줍니다.
    yield return new WaitForEndOfFrame();

    Canvas.ForceUpdateCanvases();
    scrollRect.verticalNormalizedPosition = 0f;
}
*/
    void Awake()
    {
        // 시작할 때 원래 위치와 색상을 기억해둡니다.
        originPosition = monsterSprite.transform.localPosition;
        originColor = monsterSprite.color;
        monsterBaseScale = monsterSprite.transform.localScale;

        //전투 상태 슬롯 풀 초기화
        InitializeStatusSlotPool();
        InitializeEnemyStatusSlotPool();
    }

    public void BattleUiRefresh()
    {
        monsterHpText.text = "체력 : " + instance.currentMonsterInfo.currentHp.ToString();
    }

    public void BattleUiSetting(MonsterSetInfo monsinfo)
    {
        ClearLog();
        ResetMonsterVisual();

        // 이전 전투에서 생성된 상태 슬롯을 제거한다.
        ClearBattleStatusUI();

        monsterName.text = monsinfo.name;

        Debug.Log(monsinfo.maxHp);
        monsterHpText.text = "체력 : " + monsinfo.maxHp.ToString();

        monsterSprite.sprite = monsinfo.sprite;
        monsterSprite.SetNativeSize();
        monsterSprite.GetComponent<Transform>().localScale = Vector3.one * monsinfo.imgsize;

        // 이번 몬스터의 정상 크기 기억
        monsterBaseScale = monsterSprite.transform.localScale;

        monsterHpbar.fillAmount = 1f;

        /*
        string startlog = monsinfo.name + "(이)가 나타났다! \n\n 여기서는...";
        AddLog(startlog);


        btn_battle.SetActive(true);
        btn_talk.SetActive(true);
        btn_run.SetActive(true);
        */


        // 이번 전투에서 사용할 스킬 목록 생성
        skillSelectUI.RebuildSkillList();

        battle_end_panel.SetActive(false);
        battlePannel.SetActive(true);

        // 새 전투는 항상 기본 행동 화면에서 시작
        ShowActionMenu();
    }

    private IEnumerator BattleUiExit_Coroutine()
    {
        yield return GameUiManager.Instance.FadeIn();

        battlePannel.SetActive(false);
        battle_end_panel.SetActive(false);
        ClearLog();
        ResetMonsterVisual();

        // 전투에서 유지된 현재 체력을 상단 HUD에 반영한다.
        GameManager.Instance.RefreshPlayerUi();

        GameUiManager.Instance.MapUiOpen(false);

        yield return GameUiManager.Instance.FadeOut();
        isExitingBattle = false;
    }
    public void BattleUiExit()
    {
        if (isExitingBattle) return;

        isExitingBattle = true;
        if(GameManager.Instance.isGameOver)
        {
            Debug.Log("으앙쥬금");
            StartCoroutine(GameManager.Instance.GameOverCoroutine());
        }
        else
        {
            StartCoroutine(BattleUiExit_Coroutine());
        }
    }



    public void BattleEndUi_Open()
    {
        battle_end_panel.SetActive(true);
    }

    public void BattleBtnsOff()
    {
        btn_battle.SetActive(false);
        btn_talk.SetActive(false);
        btn_run.SetActive(false);
    }
    public void BattleBtnsOn()
    {
        btn_battle.SetActive(true);
        btn_talk.SetActive(true);
        btn_run.SetActive(true);
    }

    /// <summary>
    /// 현재 행동력을 전투 HUD에 표시한다.
    /// </summary>
    public void UpdateActionPointUI(
        int current,
        int max)
    {
        actionPointText.text =
            $"행동력 {current}/{max}";
    }


    public void AddLog(string message)
    {
        // 1. 새 로그 생성 및 설정
        GameObject newLog = Instantiate(logPrefab, logParent);
        TextMeshProUGUI textComp = newLog.GetComponent<TextMeshProUGUI>();

        // 2. 초기 세팅 (줄바꿈 방지를 위해 텍스트는 미리 다 넣음)
        textComp.text = message;
        textComp.maxVisibleCharacters = 0; // 처음엔 안 보이게

        // 3. DOTween 타이핑 연출 (0.2~0.3초 추천)
        // 글자 수(maxVisibleCharacters)를 0에서 끝까지 채움
        DOTween.To(() => textComp.maxVisibleCharacters,
                   x => textComp.maxVisibleCharacters = x,
                   message.Length, logSpeed)
               .SetEase(Ease.Linear)
               .OnUpdate(() =>
               {
                   // 글자가 써지는 동안 실시간으로 스크롤 하단 고정
                   scrollRect.verticalNormalizedPosition = 1f;
               });

        // 4. (선택) 개수 제한 로직
        if (logParent.childCount > 50) Destroy(logParent.GetChild(0).gameObject);
    }

    /// <summary>
    /// 타이핑 연출 없이 로그를 즉시 표시한다.
    /// 행동 구분선처럼 바로 보여야 하는 정보에 사용한다.
    /// </summary>
    public void AddLogImmediate(string message)
    {        
        // 1. 새 로그 생성 및 설정
        GameObject newLog = Instantiate(logPrefab, logParent);
        TextMeshProUGUI textComp = newLog.GetComponent<TextMeshProUGUI>();

        // 2. 초기 세팅 (줄바꿈 방지를 위해 텍스트는 미리 다 넣음)
        textComp.text = message;
        textComp.maxVisibleCharacters = message.Length;

        // 3. (선택) 개수 제한 로직
        if (logParent.childCount > 50)
        {
            Destroy(logParent.GetChild(0).gameObject);
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// 로그 사이에 실제 여백을 추가한다.
    /// </summary>
    public void AddLogSpacer(float height = 25f)
    {
        GameObject spacer =
            new GameObject(
                "Log Spacer",
                typeof(RectTransform),
                typeof(LayoutElement));

        spacer.transform.SetParent(logParent, false);

        LayoutElement layout =
            spacer.GetComponent<LayoutElement>();

        layout.preferredHeight = height;
        layout.minHeight = height;
    }


    public void ClearLog()
    {
        // 1. Content(logParent) 자식으로 있는 모든 로그 오브젝트 파괴
        foreach (Transform child in logParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 스크롤 위치를 맨 위(1f)로 초기화
        // (위에서 아래로 밀려나는 구조라면 1f, 아래에서 위로 쌓인다면 0f)
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }


    /// <summary>
    /// 플레이어가 다음 행동을 고르는 기본 화면을 표시한다.
    /// </summary>
    public void ShowActionMenu()
    {
        actionMenuPanel.SetActive(true);
        battleLogPanel.SetActive(false);
    }

    /// <summary>
    /// 플레이어 또는 적의 행동이 실행되는 동안
    /// 전투 로그 화면을 표시한다.
    /// </summary>
    public void ShowResolutionLog()
    {
        actionMenuPanel.SetActive(false);
        battleLogPanel.SetActive(true);
    }

    /// <summary>
    /// 전투 로그 화면만 숨긴다.
    /// 스킬 선택창은 BattleSkillSelectUI가 직접 연다.
    /// </summary>
    public void HideResolutionLog()
    {
        battleLogPanel.SetActive(false);
    }


    internal void UpdateMonsterHP(int monsterHp)
    {  
        float hp_value = Mathf.Clamp01((float)monsterHp / instance.currentMonsterInfo.maxHp);

        monsterHpbar.DOFillAmount(hp_value, 0.5f).OnComplete(() =>
        {
            if (monsterHp <= 0)
            {
                PlayDeathAnimation();
                AddLog(instance.currentMonsterInfo.name +"(을)를 쓰러트렸다.");
            }
        });

        // 3. 텍스트 업데이트 (체력이 음수로 찍히지 않도록 방어)
        int displayHp = Mathf.Max(0, monsterHp);
        monsterHpText.text = "체력 : " + displayHp.ToString();
    }

    /// <summary>
    /// 다음 적 행동의 이름과 효과들을 표시한다.
    /// </summary>
    public void UpdateEnemyIntentUI(
        EnemyActionData action)
    {
        enemyActionNameText.text =
            action != null
            ? action.actionName
            : "";

        ClearEnemyIntentEffects();

        if (action == null ||
            action.effects == null)
        {
            return;
        }

        foreach (EnemyActionEffectData effect
            in action.effects)
        {
            CreateEnemyIntentEffect(effect);
        }
    }
    private void CreateEnemyIntentEffect(
      EnemyActionEffectData effect)
    {
        string value;

        switch (effect.effectType)
        {
            case EnemyActionEffectType.Attack:
                {
                    int damage =
                        BattleManager.instance
                            .CalculateEnemyPreviewDamage(
                                effect.damageMultiplier);

                    value =
                        effect.hitCount > 1
                        ? $"공격 {damage} × {effect.hitCount}"
                        : $"공격 {damage}";

                    break;
                }

            case EnemyActionEffectType.GainAttack:
                value =
                    $"공격력 +{effect.amount:0.#}";
                break;

            case EnemyActionEffectType.GainDefense:
                value =
                    $"방어력 +{effect.amount:0.#}";
                break;

            case EnemyActionEffectType.ApplyBattleStatus:
                {
                    if (effect.statusData == null)
                    {
                        return;
                    }

                    string targetText =
                        effect.statusTarget ==
                        EnemyStatusTarget.Self
                        ? "자신"
                        : "상대";

                    string sign =
                        effect.statusAmount > 0
                        ? "+"
                        : "";

                    value =
                        $"{targetText} " +
                        $"{effect.statusData.statusName} " +
                        $"{sign}{effect.statusAmount}";

                    break;
                }

            default:
                return;
        }

        EnemyIntentEffectUI slot =
            Instantiate(
                enemyEffectPrefab,
                enemyEffectContent);

        // 아이콘은 아직 사용하지 않는다.
        slot.Setup(null, value);
    }

    private void ClearEnemyIntentEffects()
    {
        foreach (Transform child
            in enemyEffectContent)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 상태 UI 슬롯을 미리 생성한다.
    /// 상태 변경 중 Instantiate가 반복되지 않도록 전투 전에 준비한다.
    /// </summary>
    private void InitializeStatusSlotPool()
    {
        for (int i = 0;i < initialStatusPoolSize;i++)
        {
            BattleStatusSlotUI slot =
                Instantiate(
                    statusSlotPrefab,
                    playerStatusContent);

            slot.Release();

            inactivePlayerStatusSlots.Enqueue(slot);
        }
    }
    /// <summary>
    /// 적 상태 UI 슬롯을 미리 생성하고
    /// 비활성 풀에 보관한다.
    /// </summary>
    private void InitializeEnemyStatusSlotPool()
    {
        for (int i = 0;i < initialStatusPoolSize; i++)
        {
            BattleStatusSlotUI slot =
                Instantiate(
                    statusSlotPrefab,
                    enemyStatusContent);

            slot.Release();

            inactiveEnemyStatusSlots.Enqueue(
                slot);
        }
    }


    /// <summary>
    /// 사용하지 않는 슬롯을 반환한다.
    /// 준비한 슬롯이 모두 사용 중이면 하나를 추가 생성해 풀을 확장한다.
    /// </summary>
    private BattleStatusSlotUI GetPlayerStatusSlot()
    {
        if (inactivePlayerStatusSlots.Count > 0)
        {
            return inactivePlayerStatusSlots.Dequeue();
        }

        // 상태가 예상보다 많아져도 표시가 누락되지 않게
        // 필요한 경우에만 풀을 한 칸 확장한다.
        return Instantiate(
            statusSlotPrefab,
            playerStatusContent);
    }

    /// <summary>
    /// 적 상태 UI에 사용할 빈 슬롯을 반환한다.
    /// 준비한 슬롯을 모두 사용 중이면 필요한 만큼 추가 생성한다.
    /// </summary>
    private BattleStatusSlotUI GetEnemyStatusSlot()
    {
        if (inactiveEnemyStatusSlots.Count > 0)
        {
            return inactiveEnemyStatusSlots.Dequeue();
        }

        return Instantiate(
            statusSlotPrefab,
            enemyStatusContent);
    }


    /// <summary>
    /// 지정한 상태의 슬롯이 있으면 수치만 갱신하고,
    /// 없으면 풀에서 빈 슬롯을 꺼내 새로 연결한다.
    /// </summary>
    public void UpdatePlayerStatusUI(
        BattleStatusState state)
    {
        if (state == null ||
            state.Data == null)
        {
            return;
        }

        // 수치가 0 이면 표시할 필요가 없으므로 제거한다.
        if (state.Amount == 0)
        {
            RemovePlayerStatusUI(
                state.Data.type);

            return;
        }

        foreach (BattleStatusSlotUI slot
            in activePlayerStatusSlots)
        {
            if (slot.IsBoundTo(
                state.Data.type))
            {
                // 이미 존재하는 상태는 슬롯을 다시 만들지 않고
                // 현재 수치와 아이콘만 갱신한다.
                slot.Setup(state);
                return;
            }
        }

        // 처음 추가된 상태이므로 빈 슬롯을 가져온다.
        BattleStatusSlotUI newSlot =
            GetPlayerStatusSlot();

        newSlot.Setup(state);

        activePlayerStatusSlots.Add(
            newSlot);
    }

    /// <summary>
    /// 지정한 적 상태의 슬롯이 있으면 숫자만 갱신하고,
    /// 없으면 풀에서 빈 슬롯을 가져와 새로 연결한다.
    /// </summary>
    public void UpdateEnemyStatusUI(
        BattleStatusState state)
    {
        if (state == null ||
            state.Data == null)
        {
            return;
        }

        // 0인 상태는 표시할 필요가 없으므로 제거한다.
        // 음수 상태는 적에게 걸린 디버프이므로 그대로 표시한다.
        if (state.Amount == 0)
        {
            RemoveEnemyStatusUI(
                state.Data.type);

            return;
        }

        foreach (BattleStatusSlotUI slot
            in activeEnemyStatusSlots)
        {
            if (slot.IsBoundTo(
                state.Data.type))
            {
                slot.Setup(state);
                return;
            }
        }

        BattleStatusSlotUI newSlot =
            GetEnemyStatusSlot();

        newSlot.Setup(state);

        activeEnemyStatusSlots.Add(
            newSlot);
    }


    /// <summary>
    /// 지정한 타입의 상태 슬롯을 화면에서 제거하고
    /// 재사용할 수 있도록 비활성 풀에 반환한다.
    /// </summary>
    public void RemovePlayerStatusUI(
        BattleStatusType type)
    {
        for (int i =
                 activePlayerStatusSlots.Count - 1;
             i >= 0;
             i--)
        {
            BattleStatusSlotUI slot =
                activePlayerStatusSlots[i];

            if (!slot.IsBoundTo(type))
            {
                continue;
            }

            activePlayerStatusSlots.RemoveAt(i);

            slot.Release();

            inactivePlayerStatusSlots.Enqueue(slot);

            return;
        }
    }

    /// <summary>
    /// 지정한 적 상태 슬롯을 화면에서 해제하고
    /// 다시 사용할 수 있도록 비활성 풀에 반환한다.
    /// </summary>
    public void RemoveEnemyStatusUI(
        BattleStatusType type)
    {
        for (int i =
                 activeEnemyStatusSlots.Count - 1;
             i >= 0;
             i--)
        {
            BattleStatusSlotUI slot =
                activeEnemyStatusSlots[i];

            if (!slot.IsBoundTo(type))
            {
                continue;
            }

            activeEnemyStatusSlots.RemoveAt(i);

            slot.Release();

            inactiveEnemyStatusSlots.Enqueue(
                slot);

            return;
        }
    }


    /// <summary>
    /// 현재 화면에 표시 중인 플레이어 상태 슬롯을 모두 해제하고
    /// 다음 상태 표시에서 재사용할 수 있도록 비활성 풀에 반환한다.
    /// 풀링 슬롯은 Destroy하지 않는다.
    /// </summary>
    private void ReleaseAllPlayerStatusSlots()
    {
        foreach (BattleStatusSlotUI slot
            in activePlayerStatusSlots)
        {
            // 슬롯이 외부 요인으로 파괴된 경우에는
            // 다시 풀에 넣지 않고 건너뛴다.
            if (slot == null)
            {
                continue;
            }

            slot.Release();

            inactivePlayerStatusSlots.Enqueue(
                slot);
        }

        activePlayerStatusSlots.Clear();
    }

    /// <summary>
    /// 현재 표시 중인 적 상태 슬롯을 모두 해제하고
    /// 다음 전투에서 재사용할 수 있도록 풀에 반환한다.
    /// </summary>
    private void ReleaseAllEnemyStatusSlots()
    {
        foreach (BattleStatusSlotUI slot
            in activeEnemyStatusSlots)
        {
            if (slot == null)
            {
                continue;
            }

            slot.Release();

            inactiveEnemyStatusSlots.Enqueue(
                slot);
        }

        activeEnemyStatusSlots.Clear();
    }

    /// <summary>
    /// 실제 플레이어 상태 목록을 기준으로
    /// 상태 슬롯 전체를 다시 연결한다.
    /// 전투 시작, 저장 데이터 로드, 강제 UI 복구 때 사용한다.
    /// </summary>
    public void RefreshPlayerStatusUI(
        List<BattleStatusState> statuses)
    {
        // 현재 활성 슬롯을 파괴하지 않고
        // 모두 비활성 풀로 반환한다.
        ReleaseAllPlayerStatusSlots();

        if (statuses == null)
        {
            return;
        }

        foreach (BattleStatusState status
            in statuses)
        {
            // 양수와 음수 상태는 모두 표시하며,
            // 정확히 0인 상태만 제외한다.
            if (status == null ||
                status.Data == null ||
                status.Amount == 0)
            {
                continue;
            }

            UpdatePlayerStatusUI(status);
        }
    }

    /// <summary>
    /// 현재 적에게 적용 중인 상태를
    /// 적 상태 영역에 다시 표시한다.
    /// 적 상태 시스템 연결 후 사용한다.
    /// </summary>
    public void RefreshEnemyStatusUI(
        List<BattleStatusState> statuses)
    {
        RebuildStatusUI(
            enemyStatusContent,
            statuses);
    }

    /// <summary>
    /// 지정한 상태 표시 영역을 비우고
    /// 현재 상태 목록으로 슬롯을 다시 생성한다.
    /// 상태 변경은 자주 일어나지 않으므로 지금은 풀링하지 않는다.
    /// </summary>
    private void RebuildStatusUI(
        Transform content,
        List<BattleStatusState> statuses)
    {
        if (content == null)
        {
            return;
        }

        ClearStatusUI(content);

        if (statuses == null)
        {
            return;
        }

        foreach (BattleStatusState status
            in statuses)
        {
            if (status == null ||
                status.Data == null ||
                status.Amount == 0)
            {
                continue;
            }

            BattleStatusSlotUI slot =
                Instantiate(
                    statusSlotPrefab,
                    content);

            slot.Setup(status);
        }
    }

    /// <summary>
    /// 지정한 영역에 생성된 기존 상태 슬롯을 모두 제거한다.
    /// 같은 프레임에 다시 갱신되어도 이전 슬롯이 화면에 남지 않게
    /// 먼저 비활성화한 뒤 제거한다.
    /// </summary>
    private void ClearStatusUI(
        Transform content)
    {
        if (content == null)
        {
            return;
        }

        foreach (Transform child
            in content)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 새로운 전투가 시작될 때 이전 전투의 상태 표시를 제거한다.
    /// </summary>
    public void ClearBattleStatusUI()
    {
        ReleaseAllPlayerStatusSlots();
        ReleaseAllEnemyStatusSlots();
    }

    public void MonsterAttackEffect()
    {

        // 이전 확대 연출이 남아있다면 중단
        monsterAttackScaleTween?.Kill();

        Sequence attackSequence = DOTween.Sequence();

        // 현재 크기에서 고정된 목표 크기까지 빠르게 확대
        attackSequence.Append(monsterSprite.transform .DOScale(monsterBaseScale * 1.1f,  0.13f)
                .SetEase(Ease.OutQuad));

        // 기준 크기로 부드럽게 복귀
        attackSequence.Append(monsterSprite.transform.DOScale(monsterBaseScale, 0.10f)
                .SetEase(Ease.InOutSine));

        attackSequence.OnComplete(() =>
        {
            // 최종 오차 방지
            monsterSprite.transform.localScale =
                monsterBaseScale;

            monsterAttackScaleTween = null;
        });

        monsterAttackScaleTween =
            attackSequence;

        monsterSprite
            .DOColor(
                new Color(1f, 1f, 1f, 0.8f),
                0.08f)
            .SetLoops(
                2,
                LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        /*

        // 현재 스케일 값을 저장
        Vector3 originalScale = monsterSprite.transform.localScale;

        // 현재 크기의 1.1배 계산
        Vector3 targetScale = originalScale * 1.1f;

        // 실행
        monsterSprite.transform.DOScale(targetScale, 0.2f)
            .SetLoops(2, LoopType.Yoyo);

        // 2. 색상 연출: 하얗게 번쩍이거나 투명도 조절
        monsterSprite.DOColor(new Color(1, 1, 1, 0.8f), 0.1f).SetLoops(2, LoopType.Yoyo);
    */
    }

    public void MonsterDamageEffect()
    {
        // 피격 연출: 좌우 흔들림
        monsterSprite.transform.DOPunchPosition(
            new Vector3(10, 0, 0),
            0.5f,
            10);

        // 이전 피격 색상 연출이 남아있다면 중단
        monsterDamageColorTween?.Kill();

        // 항상 빨강에서 원래 색으로 돌아오게 한다.
        monsterSprite.color = Color.red;
        monsterDamageColorTween =
            monsterSprite.DOColor(originColor, 0.2f);
    }

    // [연출 1] 몬스터가 쓰러질 때 (사망)
    public void PlayDeathAnimation()
    {
        // 1. 투명해지기 (Alpha 0)
        monsterSprite.DOFade(0, 0.7f);

        // 2. 아래로 이동하기 (현재 위치에서 y축으로 -100만큼)
        // .SetRelative(true)를 쓰면 현재 위치 기준 상대값으로 이동합니다.
        monsterSprite.transform.DOLocalMoveY(-100f, 0.7f).SetRelative(true);
    }

    // [연출 2] 다음 전투를 위해 원상복구 (초기화)
    public void ResetMonsterVisual()
    {
        // 색상 / 투명도 Tween 종료
        monsterSprite.DOKill();
        // 이동 / 스케일 Tween 종료
        monsterSprite.transform.DOKill();

        monsterAttackScaleTween = null;

        // 위치와 색상을 즉시 원래대로 돌림
        monsterSprite.transform.localPosition =
        originPosition;

        monsterSprite.transform.localScale =
            monsterBaseScale;

        monsterSprite.color =
            originColor;

        // 만약 나타날 때도 스르륵 나타나길 원한다면?
        // monsterImage.color = new Color(1, 1, 1, 0); // 일단 투명하게 만든 뒤
        // monsterImage.DOFade(1, 0.5f); // 0.5초 동안 서서히 나타나기

    }
}
