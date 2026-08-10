
using Sirenix.OdinInspector;
using UnityEngine;
[CreateAssetMenu(menuName = "EventEffects/AtkBuffEffect")]
public class AtkBuffEffect : EventEffectBase
{
    [ReadOnly]
    [Tooltip("이 효과는 공격력을 증가시킵니다.")]
    [TextArea]
    public string description = "공격력을 int값 만큼 증가시킵니다.";

    public override void Execute(GameContext context, EffectParam param)
    {
        int amount = param.intValue;
        context.player.stats.baseAdAttack += amount;
        Debug.Log($"공격력이 {amount} 올랐습니다! 현재 공격력: {context.player.stats.baseAdAttack}");
    }

}
