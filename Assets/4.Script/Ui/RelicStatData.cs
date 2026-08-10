using UnityEngine;

[CreateAssetMenu(fileName = "Relic_Stat_", menuName = "Items/Relic/StatModifier")]
public class RelicStatData : RelicData
{
    [Header("스탯 수정 설정")]
    public StatType targetStat;  // 어떤 스탯을 바꿀 것인가?
    public float value;          // 얼마나 바꿀 것인가? (예: 10, 0.2f 등)
    public bool isPercentage;    // 퍼센트(%) 증가인가, 고정치 증가인가?

    public override void OnAcquire()
    {
        Debug.Log($"{itemName} 획득! {targetStat}이 {value}만큼 변화합니다.");
        // 실제 플레이어 스탯 매니저에 접근하여 값을 수정함
        // PlayerStats.Instance.ModifyStat(targetStat, value, isPercentage);
    }
}

// 스탯 종류를 정의하는 열거형
public enum StatType { Ad_Attack,AP_Attack ,Defense, MoveSpeed, MaxHp,MaxMp, CriticalChance,Dodge }