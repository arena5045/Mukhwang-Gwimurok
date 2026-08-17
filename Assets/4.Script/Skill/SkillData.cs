using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Data/Skill")]
public class SkillData : ScriptableObject
{
    [LabelText("스킬 이름")]
    public string skillName;

    [LabelText("스킬 설명")]
    [TextArea]
    public string skillDescription;

    [LabelText("아이콘")]
    public Sprite icon;

    [LabelText("스킬 타입")]
    public SkillType skillType;

    [LabelText("레어도")]
    public Rarity rarity;

    [LabelText("태그")]
    [EnumToggleButtons]
    public SkillTag tags;

    [LabelText("발동 타이밍")]
    public SkillTrigger trigger;

    [LabelText("레벨별 설정")]
    public List<SkillLevelData> levels = new();


    // 현재 스킬 레벨에 해당하는 데이터를 가져온다.
    public SkillLevelData GetLevelData(int level)
    {
        if (levels == null || levels.Count == 0)
        {
            return null;
        }

        int index = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        return levels[index];
    }


    public bool CanActive(SkillContext context)
    {
        SkillLevelData levelData =
            GetLevelData(context.State.Level);

        if (levelData == null)
        {
            return false;
        }

        foreach (SkillConditionBase condition in levelData.conditions)
        {
            if (condition != null && !condition.IsMet(context))
            {
                return false;
            }
        }

        return true;
    }
}