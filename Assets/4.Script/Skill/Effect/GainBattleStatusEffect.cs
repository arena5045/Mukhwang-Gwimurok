using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public sealed class GainBattleStatusEffect
    : SkillEffectBase
{
    [LabelText("획득 상태")]
    public BattleStatusData statusData;

    [LabelText("획득량")]
    [MinValue(1)]
    public int amount = 1;

    public override IEnumerator Execute(
        SkillContext context)
    {
        if (statusData == null)
        {
            yield break;
        }

        context.Battle.buiManager.AddLog(
            $"<color=#99FF99>{context.State.Data.skillName}</color> 발동!");

        yield return new WaitForSeconds(0.5f);

        context.Battle.AddPlayerStatus(
            statusData,
            amount);

        context.Battle.buiManager.AddLog(
            $"{statusData.statusName}가 " +
            $"<color=#99FF99>{amount}</color> 증가했다.");

        yield return new WaitForSeconds(0.5f);
    }
}