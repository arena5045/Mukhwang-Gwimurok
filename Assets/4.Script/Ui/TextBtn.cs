using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;

public class TextBtn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("버튼 클릭 시 호출될 이벤트")]
    public UnityEvent onClickEvent;

    private TextMeshProUGUI text;
    private Vector3 originalScale;
    private Color originalTextColor;
    private Color originalOutlineColor;

    private const float highlightScale = 1.1f;
    private const float transitionTime = 0.1f;
    private const float outlineWidth = 0.2f;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();

        // 머티리얼 인스턴스화 (공유 방지)
        text.fontMaterial = Instantiate(text.fontMaterial);

        // 초기 값 저장
        originalScale = transform.localScale;
        originalTextColor = text.color;
        originalOutlineColor = text.fontMaterial.GetColor(ShaderUtilities.ID_OutlineColor);

        // Outline이 꺼져 있다면 기본값 설정
        float currentOutlineWidth = text.fontMaterial.GetFloat(ShaderUtilities.ID_OutlineWidth);
        if (currentOutlineWidth <= 0.001f)
        {
            text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 색 반전
        text.color = Color.black;
        text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.white);

        // 확대 효과
        transform.DOScale(originalScale * highlightScale, transitionTime).SetEase(Ease.OutBack);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 색 복원
        text.color = originalTextColor;
        text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, originalOutlineColor);

        // 크기 복원
        transform.DOScale(originalScale, transitionTime).SetEase(Ease.OutBack);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(gameObject.name + "버튼 클릭");
        onClickEvent?.Invoke();
    }
}