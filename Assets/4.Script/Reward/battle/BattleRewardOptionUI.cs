using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleRewardOptionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button selectButton;

    // 현재 이 카드가 보여주고 있는 스킬
    private SkillData currentSkill;

    // 이 카드를 선택했을 때 부모에게 전달할 함수
    private Action<SkillData> onSelected;

    public void Setup(
        SkillData skill,
        Action<SkillData> selectAction)
    {
        currentSkill = skill;
        onSelected = selectAction;

        rarityText.text =
            GameManager.Instance.RareString(skill.rarity);

        nameText.text = skill.skillName;
        descriptionText.text = skill.skillDescription;

        iconImage.sprite = skill.icon;
        iconImage.enabled = skill.icon != null;

        // 이전에 연결된 선택 이벤트가 중복 실행되지 않게 갱신
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