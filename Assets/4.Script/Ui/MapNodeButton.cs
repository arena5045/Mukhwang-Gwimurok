using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MapNodeButton : MonoBehaviour
{
    public Button btn;

    [Header("버튼 스프라이트")]
    public Sprite start_btn;
    public Sprite battle_btn;
    public Sprite e_battle_btn;
    public Sprite event_btn;
    public Sprite boss_btn;
    public Sprite shop_btn;

    [Header("체크 스프라이트")]
    public Sprite o_check;
    public Sprite x_check;
    public Sprite v_check;


    [Header("버튼 스프라이트 타겟 이미지")]
    public Image target_image;

    [Header("체크 스프라이트 타겟 이미지")]
    public Image check_image;

    Vector3 icon_scale = Vector3.zero;
    public MapNodeData node_data;


    public void SetNodeData(MapNodeData data)
    {
        data.mapNodeBtn = this;
        node_data = data;

        if (target_image == null)
            target_image = GetComponent<Image>();

        switch (data.nodeType)
        {
            case NodeType.Start:
                if (start_btn != null)
                {
                    target_image.sprite = start_btn;
                }
                break;
            case NodeType.Battle:
                if (battle_btn != null)
                {
                    target_image.sprite = battle_btn;

                    GetComponent<Button>().onClick.AddListener(BattleBtn);
                }
                break;
            case NodeType.Elite_Battle:
                if (battle_btn != null)
                {
                    target_image.sprite = e_battle_btn;
                    GetComponent<RectTransform>().localScale = new Vector3(1.5f, 1.5f, 1.5f);

                    GetComponent<Button>().onClick.AddListener(Battle_Elite_Btn);
                }
                break;
            case NodeType.Event:
                if (event_btn != null)
                {
                    target_image.sprite = event_btn;

                    GetComponent<Button>().onClick.AddListener(EventBtn);
                }
                break;
            case NodeType.Boss:
                if (boss_btn != null)
                {
                    target_image.sprite = boss_btn;
                    GetComponent<RectTransform>().localScale = new Vector3(2f, 2f, 2f);

                    GetComponent<Button>().onClick.AddListener(Battle_Boss_Btn);
                }
                break;
            case NodeType.Shop:
                if (shop_btn != null)
                {
                    target_image.sprite = shop_btn;

                    GetComponent<Button>().onClick.AddListener(ShopBtn);
                }
                break;
        }
        //투명도 테스트
        Color currentColor = target_image.color;
        currentColor.a = 0.5f;
        target_image.color = currentColor;

        //스케일 저장
        icon_scale = GetComponent<RectTransform>().localScale;
    }


    //노드 상태 변경
    public void SetcheckData(NodeState state)
    {
        node_data.state = state;
        transform.DOKill(true);
        GetComponent<RectTransform>().localScale = icon_scale;

        Color currentColor = target_image.color;
        currentColor.a = 1f;
        switch (node_data.state)
        {
            case NodeState.Here:
                target_image.color = currentColor;

                check_image.sprite = o_check;
                check_image.gameObject.SetActive(true);

                btn.interactable = false;
                break;
            case NodeState.X:
                check_image.sprite = x_check;
                check_image.gameObject.SetActive(true);

                btn.interactable = false;
                break;
            case NodeState.V:
                check_image.sprite = v_check;
                check_image.gameObject.SetActive(true);

                btn.interactable = false;
                break;
            case NodeState.None:
                currentColor.a = 0.5f;
                target_image.color = currentColor;

                check_image.sprite = null;
                check_image.gameObject.SetActive(false);

                btn.interactable = false;
                break;
            case NodeState.Next:
                target_image.color = currentColor;
                btn.interactable = true;
                StartPulse(1.5f);
                break;
        }
    }

    //깜빡이는 효과
    void StartPulse(float duration)
    {
        // 이전에 실행 중인 트윈이 있다면 모두 멈춥니다.
        transform.DOKill(true);

        // 시퀀스 생성
        Sequence sequence = DOTween.Sequence();

        // ********************************************
        // 1. 커지는 동작: 1.0배 -> 1.1배
        // ********************************************
        sequence.Append(
            transform.DOScale(icon_scale * 1.1f, duration / 2) // 속도 절반
                .SetEase(Ease.OutSine) // 부드럽게 커짐
        );

        // ********************************************
        // 2. 작아지는 동작: 1.1배 -> 0.9배
        // ********************************************
        sequence.Append(
            transform.DOScale(icon_scale * 0.9f, duration) // 전체 시간
                .SetEase(Ease.InOutSine)
        );

        // ********************************************
        // 3. 복귀 동작: 0.9배 -> 1.0배 (원래 크기)
        // ********************************************
        sequence.Append(
            transform.DOScale(icon_scale, duration / 2) // 속도 절반
                .SetEase(Ease.InSine) // 부드럽게 돌아옴
        );

        // 시퀀스 전체를 무한 반복하고 재생
        sequence.SetLoops(-1, LoopType.Restart)
                .SetId(transform) // 해당 Transform에 ID를 부여하여 DOKill로 쉽게 제어
                .Play();
    }

    public void Click()
    {
        //데이터 전달
        GameManager.Instance.Context.cur_node = this;
        GameManager.Instance.Context.cur_node_Data = node_data;


        //GameManager.Instance.MapManager.Move_Node(node_data);
        //GameManager.Instance.MapManager.Move_Map(GetComponent<RectTransform>());
    }

    //배틀버튼
    public void BattleBtn()
    {
        if(GameManager.Instance.canClick)
        GameManager.Instance.Button_Battle();
    }

    public void Battle_Elite_Btn()
    {
        if (GameManager.Instance.canClick)
            GameManager.Instance.Button_Battle_Elite();
    }
    public void Battle_Boss_Btn()
    {
        if (GameManager.Instance.canClick)
            GameManager.Instance.Button_Battle_Boss();
    }
    public void EventBtn()
    {
        if (GameManager.Instance.canClick)
            GameManager.Instance.Button_Event();
    }
    public void ShopBtn()
    {
        if (GameManager.Instance.canClick)
            GameManager.Instance.Button_Shop();
    }
}
