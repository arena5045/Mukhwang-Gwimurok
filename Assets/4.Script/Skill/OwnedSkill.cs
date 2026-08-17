using System;

[Serializable]
public class OwnedSkill
{
    public SkillData data;
    public int level = 1;

    public OwnedSkill(SkillData data)
    {
        this.data = data;
        level = 1;
    }
}