
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class EventSet
{
    // SerializeReference는 추상 기반 타입 자리에 실제 효과 타입과 그 필드 값을
    // 함께 저장한다. 따라서 Project 창에서 효과 SO를 별도로 만들 필요가 없다.
    // Odin Inspector는 이 필드에서 파생 효과 타입 선택 메뉴를 제공한다.
    [LabelText("효과")]
    [SerializeReference]
    public EventEffectBase effect;

    /// <summary>
    /// EventManager와 EventChoice가 모두 거치는 실제 효과 호출 지점이다.
    /// 호출부가 효과 타입이나 파라미터 형식을 알 필요가 없도록 하고,
    /// EventSet 하나당 효과 하나만 정확히 한 번 실행되게 한다.
    /// </summary>
    public void Execute(GameContext context)
    {
        if (effect == null)
        {
            Debug.LogWarning("[EventSet] 실행할 인라인 효과가 설정되지 않았습니다.");
            return;
        }

        effect.Execute(context);
    }
}
