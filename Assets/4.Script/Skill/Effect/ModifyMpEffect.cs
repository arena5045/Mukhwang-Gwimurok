using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public sealed class ModifyMpEffect : SkillEffectBase
{
    [LabelText("현재 도력 증감")]
    public int currentMpAmount;

    [LabelText("도력 리젠 보정 증감")]
    public int regenModifierAmount;

    public override IEnumerator Execute(SkillContext context)
    {
        BattleManager.PlayerSetInfo player =
            context.Battle.currentPlayerInfo;

        if (player == null)
        {
            yield break;
        }

        int previousMp = player.currentmp;

        // 현재 도력은 과부하 같은 효과로 최대 도력을 초과할 수 있다.
        // 단, 도력이 음수가 되지는 않게 막는다.
        player.currentmp = Mathf.Max(
            0,
            player.currentmp + currentMpAmount);

        // 리젠 보정은 다음 PlayerTurn부터 도력 리필량 계산에 사용된다.
        player.mpRegenModifier += regenModifierAmount;

        // 실제로 변화한 현재 도력량
        int appliedMpAmount =
            player.currentmp - previousMp;

        // 변경된 현재 도력을 바로 전투 UI에 반영한다.
        GameUiManager.Instance?.UpdatePlayerMPUI_battle(
            player.currentmp);

        // 스킬 효과로 도력을 얻었을 때만 푸른빛 연출
        if (appliedMpAmount > 0)
        {
            GameUiManager.Instance?.FlashMpGain();
        }

        // 실제 효과를 전투 로그에서 바로 확인할 수 있게 한다.
        string effectLog = "";

        if (appliedMpAmount != 0)
        {
            effectLog +=
                $"도력 {(appliedMpAmount > 0 ? "+" : "")}{appliedMpAmount}";
        }

        if (regenModifierAmount != 0)
        {
            if (effectLog.Length > 0)
            {
                effectLog += " / ";
            }

            effectLog +=
                $"도력 리젠 {(regenModifierAmount > 0 ? "+" : "")}{regenModifierAmount}";
        }

        context.Battle.buiManager.AddLog(
            $"<color=#99FF99>{context.State.Data.skillName}</color> 발동! \n{effectLog}");

        yield break;
    }
}