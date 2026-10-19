using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class SkillLevelData
{
    [LabelText("도력 비용")]
    [MinValue(0)]
    public int mpCost = 0;

    [LabelText("행동당 최대 발동 횟수")]
    [MinValue(1)]
    public int maxActivationsPerAction = 1;

    [LabelText("행동력 비용")]
    [MinValue(0)]
    public int actionPointCost = 1;

    [LabelText("쿨타임")]
    [MinValue(0)]
    public int cooldownTurns = 0;

    [LabelText("시작 쿨타임")]
    [MinValue(0)]
    public int initialCooldownTurns = 0;

    [LabelText("발동 조건")]
    [SerializeReference]
    public List<SkillConditionBase> conditions = new();

    [LabelText("효과")]
    [SerializeReference]
    public List<SkillEffectBase> effects = new();
}