using DG.Tweening;
using System;
using System.Collections;
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

    [Header("버튼들")]
    public GameObject btn_battle;
    public GameObject btn_talk;
    public GameObject btn_run;

    [Header("배틀종료버튼")]
    public GameObject battle_end_panel;


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
    }

    public void BattleUiRefresh()
    {
        monsterHpText.text = "체력 : " + instance.currentMonsterInfo.currentHp.ToString();
    }

    public void BattleUiSetting(MonsterSetInfo monsinfo)
    {
        ClearLog();
        ResetMonsterVisual();

        monsterName.text = monsinfo.name;

        Debug.Log(monsinfo.maxHp);
        monsterHpText.text = "체력 : " + monsinfo.maxHp.ToString();

        monsterSprite.sprite = monsinfo.sprite;
        monsterSprite.SetNativeSize();
        monsterSprite.GetComponent<Transform>().localScale = Vector3.one * monsinfo.imgsize;

        monsterHpbar.fillAmount = 1f;

        string startlog = monsinfo.name + "(이)가 나타났다! \n\n 여기서는...";
        AddLog(startlog);


        btn_battle.SetActive(true);
        btn_talk.SetActive(true);
        btn_run.SetActive(true);

        battle_end_panel.SetActive(false);
        battlePannel.SetActive(true);
    }

    private IEnumerator BattleUiExit_Coroutine()
    {
        yield return GameUiManager.Instance.FadeIn();

        battlePannel.SetActive(false);
        battle_end_panel.SetActive(false);
        ClearLog();
        ResetMonsterVisual();

        GameManager.Instance.Refresh_HpMp();

        GameUiManager.Instance.MapUiOpen(false);

        yield return GameUiManager.Instance.FadeOut();
    }
    public void BattleUiExit()
    {
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

    public void MonsterAttackEffect()
    {
        // 현재 스케일 값을 저장
        Vector3 originalScale = monsterSprite.transform.localScale;

        // 현재 크기의 1.1배 계산
        Vector3 targetScale = originalScale * 1.1f;

        // 실행
        monsterSprite.transform.DOScale(targetScale, 0.2f)
            .SetLoops(2, LoopType.Yoyo);

        // 2. 색상 연출: 하얗게 번쩍이거나 투명도 조절
        monsterSprite.DOColor(new Color(1, 1, 1, 0.8f), 0.1f).SetLoops(2, LoopType.Yoyo);
    }

    public void MonsterDamageEffect()
    {

        // 1. 피격 연출: 좌우로 강하게 흔들림 (PunchPosition)
        // (강도, 시간, 진동횟수)
        monsterSprite.transform.DOPunchPosition(new Vector3(10, 0, 0), 0.5f, 10);

        // 2. 붉은색 페이드 (깜빡임)
        monsterSprite.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo);


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
        // 모든 트윈 중단 (혹시 실행 중일지 모를 애니메이션 방지)
        monsterSprite.DOKill();

        // 위치와 색상을 즉시 원래대로 돌림
        monsterSprite.transform.localPosition = originPosition;
        monsterSprite.color = originColor;

        // 만약 나타날 때도 스르륵 나타나길 원한다면?
        // monsterImage.color = new Color(1, 1, 1, 0); // 일단 투명하게 만든 뒤
        // monsterImage.DOFade(1, 0.5f); // 0.5초 동안 서서히 나타나기

    }
}
