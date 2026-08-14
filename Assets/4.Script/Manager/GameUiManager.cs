using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameUiManager : MonoBehaviour
{
    public GameObject mapUi;
    public GameObject mapBtn;

    public GameObject battlePanel;
    public GameObject talkPanel;
    public GameObject shopPanel;

    [Header("상단바 요소들")]
    public GameObject Top_Bar;

    public Image playerhpBar;
    public TMP_Text playerhpText;

    public Image playermpBar;
    public TMP_Text playempText;

    public TMP_Text playerGoldText;
    public TMP_Text playerSoulText;

    [Header("추가 요소들")]
    public GameObject gameOver_panel;

    public RectTransform fadeImage;
    private float screenWidth;

    // 현재 페이드 애니메이션이 진행 중인지 체크
    public bool IsFading { get; private set; }

    // 닫기 버튼을 연속으로 눌러 동일한 페이드 코루틴이 여러 개 실행되는 것을 막는다.
    // 중복 실행되면 패널 활성 상태와 IsFading 해제 시점이 서로 어긋날 수 있다.
    private bool isClosingShop;

    //인 게임에서 ui를 관리하는 싱글턴
    public static GameUiManager Instance { get; private set; }

    private void Awake()
    {
        // 이 매니저의 필드는 현재 씬 Canvas와 패널을 직접 참조한다.
        // 따라서 씬을 넘어 유지하지 않고, 같은 씬 안에서 실수로 중복 배치된 경우만 제거한다.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // UI 참조는 씬 전용이므로 GameUiManager 자체도 씬과 함께 교체한다.
        Instance = this;

        ConfigureTopHudSafeArea();

        // 화면 해상도의 너비를 가져옵니다.
        screenWidth = GameObject.Find("Canvas").GetComponent<RectTransform>().rect.width;
        // 시작 시 화면 오른쪽에 대기시킵니다.
        fadeImage.anchoredPosition = new Vector2(-2 * screenWidth, 0);
    }

    /// <summary>
    /// 사용자가 만든 '상단 ui' 묶음에만 Android Safe Area 보정을 연결한다.
    /// 기존 Top_Bar는 이 묶음의 자식으로 남으므로 붉은 피격 플래시와 내부 UI 배치는 유지된다.
    /// </summary>
    private void ConfigureTopHudSafeArea()
    {
        if (Top_Bar == null)
        {
            Debug.LogWarning("상단 HUD Safe Area 보정에 필요한 Top_Bar 참조가 없습니다.", this);
            return;
        }

        RectTransform topHudRoot = Top_Bar.transform.parent as RectTransform;

        // Top_Bar가 실수로 메인 패널 바로 아래로 돌아간 상태에서 부모를 이동하면 다른 UI까지
        // 함께 움직일 수 있다. 정확히 '상단 ui' 묶음이 확인될 때만 컴포넌트를 추가한다.
        if (topHudRoot == null || topHudRoot.name != "상단 ui")
        {
            Debug.LogWarning(
                "상단 HUD Safe Area를 적용하지 못했습니다. Top_Bar의 부모 '상단 ui'를 확인하세요.",
                this);
            return;
        }

        if (topHudRoot.GetComponent<TopHudSafeArea>() == null)
        {
            topHudRoot.gameObject.AddComponent<TopHudSafeArea>();
        }
    }

    private void OnDestroy()
    {
        // 씬 종료 후 다른 코드가 파괴된 UI를 싱글턴으로 찾지 않도록 참조를 비운다.
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame)
        {
            StartCoroutine(FadeIn());
        }
        if (Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame)
        {
            StartCoroutine(FadeOut());
        }
#endif
    }

    public void RefreshUi()
    {
        GameContext context = GameManager.Instance.Context;

        playerhpText.text = $"체력 : {context.player.currentHP}";
        // 정수끼리 나누면 중간 체력이 모두 0으로 잘리므로 float로 변환한다.
        // 최대치가 잘못 설정된 콘텐츠도 0으로 나누지 않도록 별도로 방어한다.
        playerhpBar.fillAmount = context.player.stats.MaxHp > 0
            ? (float)context.player.currentHP / context.player.stats.MaxHp
            : 0f;

        playempText.text = $"도력 : {context.player.currentMp}";
        playermpBar.fillAmount = context.player.stats.MaxMp > 0
            ? (float)context.player.currentMp / context.player.stats.MaxMp
            : 0f;

        playerSoulText.text = context.player.soul.ToString();
        playerGoldText.text = context.player.gold.ToString();
    }

    public void UpdateGoldUi()
    {
        // 현재 텍스트의 숫자를 읽어와서 targetGold까지 부드럽게 변화시킴
        if (!int.TryParse(playerGoldText.text.Replace(",", ""), out int currentGold))
        {
            currentGold = GameManager.Instance.Context.player.gold;
        }

        DOTween.To(() => currentGold, x => {
            currentGold = x;
            playerGoldText.text = currentGold.ToString("N0");
        }, GameManager.Instance.Context.player.gold, 0.5f);
    }

    public void UpdateSoulUi()
    {
        // 현재 텍스트의 숫자를 읽어와서 targetGold까지 부드럽게 변화시킴
        if (!int.TryParse(playerSoulText.text.Replace(",", ""), out int currentSoul))
        {
            currentSoul = GameManager.Instance.Context.player.soul;
        }

        DOTween.To(() => currentSoul, x => {
            currentSoul = x;
            playerSoulText.text = currentSoul.ToString("N0");
        }, GameManager.Instance.Context.player.soul, 0.5f);
    }

    // 화면을 오른쪽에서 중앙으로 덮기
    public IEnumerator FadeIn(float duration = 0.5f)
    {
        IsFading = true; // 시작할 때 잠금

        // 오른쪽 끝에서 중앙(0)으로 이동
        yield return fadeImage.DOAnchorPosX(0, duration).SetEase(Ease.OutQuad).WaitForCompletion();
    }

    // 화면 중앙에서 왼쪽 끝으로 빠져나가기
    public IEnumerator FadeOut(float duration = 0.5f)
    {
        // 중앙에서 왼쪽(-너비)으로 이동
        yield return fadeImage.DOAnchorPosX(5 * screenWidth, duration).SetEase(Ease.InQuad).WaitForCompletion();

        // 다음 페이드를 위해 다시 오른쪽 끝으로 순간이동 시켜둡니다.
        fadeImage.anchoredPosition = new Vector2(-2 * screenWidth, 0);

        IsFading = false; // 완전히 끝났을 때 잠금 해제
    }


    //맵버튼 온
    public void MapUiOpen(bool canClose)
    {
        //canClose가 트루면 = 끌수있음 = 맵버튼on
        mapBtn.SetActive(canClose);
        mapUi.SetActive(true);
    }
    //맵 버튼 끄기
    public void MapUiClose(bool canOpen)
    {
        //canOpen 트루면 = 켤수있음 = 맵버튼on
        mapBtn.SetActive(canOpen);
        mapUi.SetActive(false);

    }

    public void MapNodeMove()
    {
        GameManager.Instance.mapManager.Move_Node(GameManager.Instance.Context.cur_node_Data);
        GameManager.Instance.mapManager.Move_Map(GameManager.Instance.Context.cur_node.GetComponent<RectTransform>());
    }

    public void AllPanelOff()
    {
        MapUiClose(true);
        battlePanel.SetActive(false);
        talkPanel.SetActive(false);
        shopPanel.SetActive(false);
    }

    public void GameOver()
    {
        gameOver_panel.SetActive(true);
    }

    public void ShopUiOpen()
    {
        ShopManager.Instance.OpenShop();
        shopPanel.SetActive(true);
    }
    public void ShopUiClose()
    {
        if (isClosingShop) return;

        isClosingShop = true;
        StartCoroutine(ShopUiExit_Coroutine());
    }

    private IEnumerator ShopUiExit_Coroutine()
    {
        yield return Instance.FadeIn();

        shopPanel.SetActive(false);
        ShopManager.Instance.CloseShop();
        Instance.MapUiOpen(false);

        yield return Instance.FadeOut();
        isClosingShop = false;
    }

    public void UpdatePlayerHPUI_battle(int current, bool isSmooth = true)
    {
        int maxhp = BattleManager.instance.currentPlayerInfo.maxhp;
        float targetFill = maxhp > 0 ? (float)current / maxhp : 0f;

        if (isSmooth)
        {
            playerhpBar.DOFillAmount(targetFill, 0.5f);
        }
        else
        {
            playerhpBar.fillAmount = targetFill;
        }
        // 1. 피격 연출: 좌우로 강하게 흔들림 (PunchPosition)
        // (강도, 시간, 진동횟수)
        playerhpBar.transform.DOPunchPosition(new Vector3(10, 0, 0), 0.5f, 10);
        // 2. 붉은색 페이드 (깜빡임)
        Top_Bar.GetComponent<Image>().DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo);

        // 3. 텍스트 업데이트 (체력이 음수로 찍히지 않도록 방어)
        int displayHp = Mathf.Max(0, current);

        playerhpText.text = $"체력 : {displayHp}";
    }

}
