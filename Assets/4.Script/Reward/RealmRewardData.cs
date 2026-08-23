using UnityEngine;

[CreateAssetMenu(
    fileName = "RealmReward",
    menuName = "Data/Realm Reward")]
public class RealmRewardData : ScriptableObject
{
    [Header("기본 정보")]
    public string rewardName;

    [TextArea]
    public string description;

    public Rarity rarity;

    public Sprite icon;

    [Header("능력치 증가")]
    public int maxHp;
    public int maxMp;
    public int attack;
    public int defense;
    public int speed;
    public int luck;

    public void Apply(PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        player.stats.MaxHp += maxHp;
        player.currentHP += maxHp;

        player.stats.MaxMp += maxMp;
        player.currentMp += maxMp;

        player.stats.baseAdAttack += attack;
        player.stats.baseDefense += defense;
        player.stats.baseSpeed += speed;

        player.stats.luck += luck;

        // 혹시 음수 보상 등이 생겨도 최대치를 넘지 않게 보정
        player.currentHP =
            Mathf.Clamp(player.currentHP, 0, player.stats.MaxHp);

        player.currentMp =
            Mathf.Clamp(player.currentMp, 0, player.stats.MaxMp);
    }
}