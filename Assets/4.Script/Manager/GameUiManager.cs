using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
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

    //인 게임에서 ui를 관리하는 싱글턴
    public static GameUiManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        // 화면 해상도의 너비를 가져옵니다.
        screenWidth = GameObject.Find("Canvas").GetComponent<RectTransform>().rect.width;
        // 시작 시 화면 오른쪽에 대기시킵니다.
        fadeImage.anchoredPosition = new Vector2(-2 * screenWidth, 0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(FadeIn());
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(FadeOut());
        }
    }

    public void RefreshUi()
    {
        GameContext context = GameManager.Instance.Context;

        playerhpText.text = $"체력 : {context.player.currentHP}";
        playerhpBar.fillAmount = context.player.currentHP / context.player.stats.MaxHp;

        playempText.text = $"도력 : {context.player.currentMp}";
        playermpBar.fillAmount = context.player.currentMp / context.player.stats.MaxMp;

        playerSoulText.text = context.player.soul.ToString();
        playerGoldText.text = context.player.gold.ToString();
    }

    public void UpdateGoldUi()
    {
        // 현재 텍스트의 숫자를 읽어와서 targetGold까지 부드럽게 변화시킴
        int currentGold = int.Parse(playerGoldText.text.Replace(",", ""));

        DOTween.To(() => currentGold, x => {
            currentGold = x;
            playerGoldText.text = currentGold.ToString("N0");
        }, GameManager.Instance.Context.player.gold, 0.5f);
    }

    public void UpdateSoulUi()
    {
        // 현재 텍스트의 숫자를 읽어와서 targetGold까지 부드럽게 변화시킴
        int currentSoul = int.Parse(playerSoulText.text.Replace(",", ""));

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
        StartCoroutine(ShopUiExit_Coroutine());
    }

    private IEnumerator ShopUiExit_Coroutine()
    {
        yield return Instance.FadeIn();

        shopPanel.SetActive(false);
        ShopManager.Instance.CloseShop();
        Instance.MapUiOpen(false);

        yield return Instance.FadeOut();
    }

    public void UpdatePlayerHPUI_battle(int current, bool isSmooth = true)
    {
        int maxhp = BattleManager.instance.currentPlayerInfo.maxhp;
        float targetFill = (float)current / maxhp;

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
