using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 플레이어의 방어력을 계수로 사용해 방어도를 획득한다.
/// </summary>
[Serializable]
public sealed class GainGuardEffect : SkillEffectBase
{
    [LabelText("기본 방어도")]
    [MinValue(0)]
    public int baseGuard;

    [LabelText("방어력 계수 (%)")]
    [MinValue(0f)]
    public float defensePercent = 0f;

    public override IEnumerator Execute(
        SkillContext context)
    {
        BattleManager.PlayerSetInfo player =
            context.Battle.currentPlayerInfo;

        if (player == null)
        {
            yield break;
        }

        // 전투 중 방어력 보정까지 포함한 현재 유효 방어력
        float effectiveDefense =
            (player.defense + player.buffdef) *
            player.multdef;

        int guardAmount =
            Mathf.Max(
                0,
                Mathf.RoundToInt(
                    baseGuard +
                    effectiveDefense *
                    defensePercent / 100f));

        int appliedGuard =
            context.Battle.GainPlayerGuard(
                guardAmount);
        // 먼저 발동 로그
        context.Battle.buiManager.AddLog(
            $"<color=#99FF99>{context.State.Data.skillName}</color> 발동!");

        // 공격의 선딜과 비슷하게 약간 기다림
        yield return new WaitForSeconds(0.5f);

        // 실제 효과 로그
        context.Battle.buiManager.AddLog(
            $"방어도 <color=#99FF99>{appliedGuard}</color>을 얻었다.");

        // 공격 연출이 없는 효과도 로그를 확인할 시간을 준다.
        yield return new WaitForSeconds(0.5f);
    }
}