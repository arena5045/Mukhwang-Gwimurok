using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class SkillConditionBase
{
    public abstract bool IsMet(SkillContext context);
}


//조건이 발동확률
[Serializable]
public sealed class ChanceCondition : SkillConditionBase
{
    [LabelText("발동 확률")]
    [Range(0f, 1f)]
    public float chance = 1f;


    public override bool IsMet(SkillContext context)
    {
        return UnityEngine.Random.value < chance;
    }
}


//조건이 체력비율
[Serializable]
public sealed class HpBelowCondition : SkillConditionBase
{
    [LabelText("체력 비율")]
    [Range(0f, 1f)]
    public float hpRatio = 0.2f;

    public override bool IsMet(SkillContext context)
    {
        return context.PlayerHpRatio <= hpRatio;
    }
}

//조건이 턴 간격
[Serializable]
public sealed class EveryNActionCondition : SkillConditionBase
{
    [LabelText("행동 간격")]
    [Min(1)]
    public int interval = 3;

    public override bool IsMet(SkillContext context)
    {
        return context.PlayerActionCount > 0 &&
               context.PlayerActionCount % interval == 0;
    }
}

// 직전에 적중한 스킬이 특정 태그를 가지고 있는지 검사
[Serializable]
public sealed class SourceSkillTagCondition : SkillConditionBase
{
    [LabelText("원본 스킬 태그")]
    [EnumToggleButtons]
    public SkillTag requiredTags;

    public override bool IsMet(SkillContext context)
    {
        if (context.SourceSkill == null ||
            requiredTags == SkillTag.None)
        {
            return false;
        }

        SkillTag sourceTags = context.SourceSkill.Data.tags;

        return (sourceTags & requiredTags) == requiredTags;
    }
}
