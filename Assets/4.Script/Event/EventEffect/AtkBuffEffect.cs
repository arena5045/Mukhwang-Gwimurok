
using Sirenix.OdinInspector;
using UnityEngine;

// 기존 "Atk Buff Effect(int)" SO 에셋을 사용자가 새 구조 확인 전까지
// 열어볼 수 있도록 보존하는 호환 클래스다. 새 EventSet에서는 이 클래스를
// 사용하지 않으며, 공격력 변경은 인라인 ModifyStatEffect가 담당한다.
// 새 구형 효과 에셋이 더 생기지 않도록 CreateAssetMenu는 노출하지 않는다.
public class AtkBuffEffect : ScriptableObject, IEventEffect
{
    [ReadOnly]
    [Tooltip("이 효과는 공격력을 증가시킵니다.")]
    [TextArea]
    public string description = "공격력을 int값 만큼 증가시킵니다.";

    public void Execute(GameContext context, EffectParam param)
    {
        int amount = param.intValue;
        context.player.stats.baseAdAttack += amount;
        Debug.Log($"공격력이 {amount} 올랐습니다! 현재 공격력: {context.player.stats.baseAdAttack}");
    }

}
