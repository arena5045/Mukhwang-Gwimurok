using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 무공 보상 선택지 하나를 표시한다.
///
/// 공통 아이콘 정보는 SkillIconUI에 맡기고,
/// 보상을 선택한 뒤 도달할 수련 단계의 효과를 미리 표시한다.
/// </summary>
/// 
public class BattleRewardOptionUI : MonoBehaviour
{
    [Header("공용 무공 아이콘")]
    [SerializeField]
    private SkillIconUI skillIconUI;

    [Header("보상 정보")]
    [SerializeField]
    private TMP_Text rarityText;

    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private Button selectButton;

    [SerializeField]
    private TMP_Text cooldownText;

    [SerializeField]
    private TMP_Text firstCooldownText;


    // 현재 이 카드가 보여주고 있는 스킬
    private SkillData currentSkill;

    // 이 카드를 선택했을 때 부모에게 전달할 함수
    private Action<SkillData> onSelected;

    public void Setup(
        SkillData skill,
        Action<SkillData> selectAction)
    {
        if (skill == null)
        {
            return;
        }

        currentSkill = skill;
        onSelected = selectAction;

        PlayerData player = GameManager.Instance.Context.player;

        // 보상을 실제로 선택한 뒤 도달할 수련 단계
        int previewLevel = SkillDescriptionBuilder.GetRewardPreviewLevel(skill, player);

        rarityText.text =
            GameManager.Instance.RareString(skill.rarity);

        nameText.text = skill.skillName;

        // 희귀도 테두리와 수련도 역시 선택 후의 결과를 미리 보여준다.
        skillIconUI.SetSkill(skill, previewLevel);

        // 선택 후 수련 단계의 효과와 수치를 표시한다.
        descriptionText.text = SkillDescriptionBuilder.BuildRewardSummary(
                    skill,
                    previewLevel,
                    player);

        // 선택지가 재사용돼도 이벤트가 중복되지 않게 한다.
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(Select);
    }

    private void Select()
    {
        if (currentSkill == null)
        {
            return;
        }

        // 어떤 스킬이 선택됐는지만 부모에게 알린다.
        onSelected?.Invoke(currentSkill);
    }
}