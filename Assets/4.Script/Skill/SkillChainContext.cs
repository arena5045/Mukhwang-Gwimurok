using System.Collections.Generic;

public sealed class SkillChainContext
{
    // 이번 한 타격에서 시작된 연쇄에 이미 등장한 스킬들
    private readonly HashSet<BattleSkillState> usedSkills = new();

    public bool HasUsed(BattleSkillState state)
    {
        return usedSkills.Contains(state);
    }

    public void MarkUsed(BattleSkillState state)
    {
        usedSkills.Add(state);
    }
}