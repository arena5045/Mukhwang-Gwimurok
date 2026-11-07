using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
public enum BattleStatusTarget
{
    Player,
    Enemy
}

[Serializable]
public sealed class GainBattleStatusEffect
    : SkillEffectBase
{
    [LabelText("적용 대상")]
    public BattleStatusTarget target;

    [LabelText("획득 상태")]
    public BattleStatusData statusData;

    [LabelText("획득량")]
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

        if (target == BattleStatusTarget.Player)
        {
            context.Battle.AddPlayerStatus(
                statusData,
                amount);
        }
        else
        {
            context.Battle.AddMonsterStatus(
                statusData,
                amount);
        }

        string targetName =
        target == BattleStatusTarget.Player
        ? "자신"
        : $"<color=red>{context.Battle.currentMonsterInfo.name}</color>";

        string changeText =
            amount > 0
            ? "증가했다."
            : "감소했다.";

        context.Battle.buiManager.AddLog(
            $"{targetName}의 {statusData.statusName}가 " +
            $"<color=#99FF99>{Mathf.Abs(amount)}</color> " +
            $"{changeText}");

        yield return new WaitForSeconds(0.5f);

    }
}