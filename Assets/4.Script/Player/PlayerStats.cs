public class PlayerStats
{
    public int MaxHp;
    public int MaxMp;

    public float baseAdAttack;
    public float baseApAttack;

    public float baseDefense;

    public float baseSpeed;

    public float bonusAdFromItems;
    public float bonusApFromItems;
    public float bonusDefenseFromItems;
    public float bonusSpeedFromItems;

    public float bonusAdFromBuffs=1f;
    public float bonusApFromBuffs = 1f;
    public float bonusDefenseFromBuffs = 1f;
    public float bonusSpeedFromBuffss = 1f;

    public float TotalAdAttack => (baseAdAttack + bonusAdFromItems) * bonusAdFromBuffs;
    public float TotalApAttack => (baseApAttack + bonusApFromItems) * bonusApFromBuffs;

    public float TotalDefense => (baseDefense + bonusDefenseFromItems) * bonusDefenseFromBuffs;
    public float TotalSpeed => (baseSpeed + bonusSpeedFromItems) * bonusSpeedFromBuffss;
}