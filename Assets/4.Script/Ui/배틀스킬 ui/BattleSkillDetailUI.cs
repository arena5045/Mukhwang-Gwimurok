using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleSkillDetailUI : MonoBehaviour
{
    [Header("스킬 정보")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text actionPointText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private TMP_Text initialCooldownText;
    [SerializeField] private TMP_Text tagText;

    /// <summary>
    /// 선택된 스킬의 상세 정보를 표시한다.
    /// </summary>
    public void SetSkill(BattleSkillState state)
    {
        if (state == null)
        {
            Clear();
            return;
        }

        SkillData skill = state.Data;

        SkillLevelData levelData =
            skill.GetLevelData(state.Level);

        if (levelData == null)
        {
            Clear();
            return;
        }

        // 아이콘
        iconImage.sprite = skill.icon;
        iconImage.enabled = skill.icon != null;

        // 기본 정보
        nameText.text = skill.skillName;
        descriptionText.text = skill.skillDescription;

        // 행동력 비용
        actionPointText.text =
            levelData.actionPointCost.ToString();

        // 재사용 관련 정보
        cooldownText.text =
            $"재사용 대기시간: {levelData.cooldownTurns}";

        initialCooldownText.text =
            $"준비시간: {levelData.initialCooldownTurns}";

        // 일단 기본 문자열로 표시
        tagText.text =
            skill.tags.ToString();
    }

    /// <summary>
    /// 선택된 스킬이 없을 때 상세 정보를 비운다.
    /// </summary>
    public void Clear()
    {
        iconImage.sprite = null;
        iconImage.enabled = false;

        actionPointText.text = "";
        nameText.text = "";
        descriptionText.text = "";
        cooldownText.text = "";
        initialCooldownText.text = "";
        tagText.text = "";
    }
}