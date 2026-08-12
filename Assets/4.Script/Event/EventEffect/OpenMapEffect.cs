using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 맵 UI를 여는 동작은 능력치 변경과 실행 방식이 다르므로 별도 효과로 유지한다.
/// canClose 값은 이 효과가 직접 소유하여 공용 Bool 파라미터 선택이 필요 없다.
/// </summary>
[System.Serializable]
public sealed class OpenMapEffect : EventEffectBase
{
    [LabelText("닫기 허용")]
    public bool canClose = true;

    public override void Execute(GameContext context)
    {
        if (GameUiManager.Instance == null)
        {
            Debug.LogWarning($"[OpenMapEffect] GameUiManager가 없어 맵을 열 수 없습니다. canClose: {canClose}");
            return;
        }

        GameUiManager.Instance.MapUiOpen(canClose);
        Debug.Log($"[OpenMapEffect] 맵 UI를 열었습니다. canClose: {canClose}");
    }
}
