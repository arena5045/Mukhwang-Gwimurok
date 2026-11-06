using UnityEngine;

public enum BattleStatusType
{
    Momentum,   // 기세
    Vigor       // 활력
}

[CreateAssetMenu(
    fileName = "Status_",
    menuName = "Data/Battle Status")]
public class BattleStatusData : ScriptableObject
{
    public BattleStatusType type;

    public string statusName;

    public Sprite icon;

    [TextArea]
    public string description;
}