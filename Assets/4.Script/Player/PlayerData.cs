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

    public int realmLevel = 1;
    public int currentExp = 0;

    public int GetRequiredExp()
    {
        return 100;
    }

    public List<OwnedSkill> skills = new List<OwnedSkill>();


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

    public OwnedSkill GetOwnedSkill(SkillData skillData)
    {
        foreach (OwnedSkill ownedSkill in skills)
        {
            if (ownedSkill != null &&
                ownedSkill.data == skillData)
            {
                return ownedSkill;
            }
        }
        return null;
    }

    public bool CanReceiveSkill (SkillData skillData)
    {
        if (skillData == null ||
        skillData.levels == null ||
        skillData.levels.Count == 0)
        {
            return false;
        }

        OwnedSkill ownedSkill = GetOwnedSkill(skillData);

        // 아직 없는 스킬은 새로 획득할 수 있다.
        if (ownedSkill == null)
        {
            return true;
        }

        // levels 개수를 이 스킬의 최대 레벨로 사용한다.
        return ownedSkill.level < skillData.levels.Count;
    }

    // 미보유 스킬은 Lv1로 획득하고,
    // 이미 보유 중이면 한 레벨 올린다.
    public bool AcquireOrLevelUpSkill(SkillData skillData)
    {
        if (!CanReceiveSkill(skillData))
        {
            return false;
        }

        OwnedSkill ownedSkill =
            GetOwnedSkill(skillData);

        if (ownedSkill == null)
        {
            skills.Add(
                new OwnedSkill(skillData));

            return true;
        }

        ownedSkill.level++;

        return true;
    }
}