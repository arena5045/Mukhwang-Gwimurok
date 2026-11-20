using UnityEngine;

/// <summary>
/// 전투 승리 시 지급할 금화, 혼백, 경험치의 기본값.
///
/// DungeonData가 전투 종류별로 보관하며,
/// 무공 보상풀이나 경지 보상 규칙은 포함하지 않는다.
/// </summary>
[System.Serializable]
public struct BattleRewardAmounts
{
    [Tooltip("승리 시 지급할 금화 기본값")]
    [Min(0)]
    public int gold;

    [Tooltip("승리 시 지급할 혼백 기본값")]
    [Min(0)]
    public int soul;

    [Tooltip("승리 시 지급할 경험치")]
    [Min(0)]
    public int exp;
}