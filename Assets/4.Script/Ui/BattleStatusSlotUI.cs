using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 상태 하나를 표시하는 UI 슬롯.
/// 실제 상태를 소유하지 않고 어떤 상태를 표시 중인지만 기억한다.
/// </summary>
public sealed class BattleStatusSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text amountText;

    // 현재 이 슬롯이 표시 중인 상태 타입
    public BattleStatusType BoundType { get; private set; }

    // 현재 슬롯에 상태가 연결되어 있는지 확인
    public bool IsBound { get; private set; }

    /// <summary>
    /// 슬롯을 지정한 상태에 연결하고 현재 값을 표시한다.
    /// 새 상태 연결과 기존 상태 갱신 모두 이 함수를 사용한다.
    /// </summary>
    public void Setup(BattleStatusState state)
    {
        if (state == null ||
            state.Data == null)
        {
            Release();
            return;
        }

        BoundType = state.Data.type;
        IsBound = true;

        iconImage.sprite = state.Data.icon;
        iconImage.enabled = state.Data.icon != null;

        amountText.text = state.Amount.ToString();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 현재 상태와의 연결을 해제하고 풀에서 다시 사용할 수 있게 한다.
    /// </summary>
    public void Release()
    {
        IsBound = false;
        amountText.text = string.Empty;
        iconImage.sprite = null;

        gameObject.SetActive(false);
    }

    public bool IsBoundTo(BattleStatusType type)
    {
        return IsBound &&
               BoundType == type;
    }
}