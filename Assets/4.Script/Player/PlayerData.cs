using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public PlayerStats stats = new PlayerStats();  // 능력치 정보

    public int currentHP;

    public int currentMp;

    public int gold = 0;
    public int soul = 0;

    public List<OwnedSkill> skills = new List<OwnedSkill>();
    //public List<Item> inventory = new List<Item>();
    // public List<StatusEffect> statusEffects = new List<StatusEffect>();

    public void Initialize()
    {
        currentHP = stats.MaxHp;
        currentMp = stats.MaxMp;
    }

    public void IncreaseAdAttack(int amount)
    {
        stats.baseAdAttack += amount;
    }
    public void IncreaseApAttack(int amount)
    {
        stats.baseApAttack += amount;
    }
    public void IncreaseDefense(int amount)
    {
        stats.baseDefense += amount;
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, stats.MaxHp);
    }

    public void Damage(int amount)
    {
        currentHP = Mathf.Max(currentHP - amount, 0);
    }

    public void AddGold(int amount)
    {
        gold += amount;
    }
}