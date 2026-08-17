using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

// 적에게 공격 피해를 주는 스킬 효과.

[Serializable]
public sealed class AttackEffect : SkillEffectBase
{
    [LabelText("피해 배율")]
    [MinValue(0f)]
    public float damageMultiplier = 1f;

    [LabelText("타격 횟수")]
    [MinValue(1)]
    public int hitCount = 1;

    public override IEnumerator Execute(SkillContext context)
    {
        // 이 단계에서는 스킬 자신의 이름을 전투 로그에 사용한다.
        string skillName = context.State.Data.skillName;

        // 실제 공격 계산은 BattleManager에게 맡긴다.
        // AttackEffect 자체는 "몇 배로 몇 번 공격할지"만 알고 있다.
        yield return context.Battle.ExecuteSkillAttack(
            skillName,
            damageMultiplier,
            hitCount);
    }
}