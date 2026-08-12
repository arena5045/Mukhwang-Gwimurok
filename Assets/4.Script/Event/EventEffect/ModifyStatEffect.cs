using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 이벤트에서 현재 지원하는 플레이어 능력치 종류다.
/// 문자열이나 reflection으로 필드를 찾지 않고 명시적인 switch로 변경하기 위해
/// PlayerStats에 실제 존재하며 이벤트에서 사용할 값만 노출한다.
/// </summary>
public enum EventStatType
{
    [LabelText("공격력")]
    Attack,

    [LabelText("최대 체력")]
    MaxHp,

    [LabelText("최대 도력")]
    MaxMp,

    [LabelText("방어력")]
    Defense,

    [LabelText("속도")]
    Speed
}

/// <summary>
/// 동일한 방식으로 동작하는 능력치 증감 효과를 하나로 묶은 인라인 효과다.
/// 양수는 증가, 음수는 감소로 처리하므로 별도의 Buff/Debuff 클래스가 필요 없다.
/// </summary>
[System.Serializable]
public sealed class ModifyStatEffect : EventEffectBase
{
    [LabelText("대상 능력치")]
    public EventStatType stat;

    [LabelText("변화량")]
    public int amount;

    public override void Execute(GameContext context)
    {
        if (context?.player?.stats == null)
        {
            Debug.LogError("[ModifyStatEffect] 플레이어 능력치 데이터가 없어 효과를 적용할 수 없습니다.");
            return;
        }

        PlayerData player = context.player;

        switch (stat)
        {
            case EventStatType.Attack:
                player.stats.baseAdAttack += amount;
                break;

            case EventStatType.MaxHp:
                // 최대 체력은 UI 계산과 전투 초기화에서 분모로 사용되므로 최소 1을 보장한다.
                // 최대치가 감소했을 때 현재 체력이 새 최대치를 넘지 않도록 함께 보정한다.
                player.stats.MaxHp = Mathf.Max(1, player.stats.MaxHp + amount);
                player.currentHP = Mathf.Min(player.currentHP, player.stats.MaxHp);
                break;

            case EventStatType.MaxMp:
                // 최대 도력도 0으로 나누는 상황을 피하도록 최소 1을 유지하고 현재값을 보정한다.
                player.stats.MaxMp = Mathf.Max(1, player.stats.MaxMp + amount);
                player.currentMp = Mathf.Min(player.currentMp, player.stats.MaxMp);
                break;

            case EventStatType.Defense:
                player.stats.baseDefense += amount;
                break;

            case EventStatType.Speed:
                player.stats.baseSpeed += amount;
                break;

            default:
                Debug.LogError($"[ModifyStatEffect] 지원하지 않는 능력치입니다: {stat}");
                return;
        }

        Debug.Log($"[ModifyStatEffect] {stat} 능력치에 {amount:+#;-#;0}을 적용했습니다.");
    }
}
