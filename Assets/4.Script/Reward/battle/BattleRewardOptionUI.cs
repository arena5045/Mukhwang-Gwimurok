using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleRewardOptionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image iconImage;

    // 보상 후보 SkillData의 내용을 카드에 표시한다.
    public void Setup(SkillData skill)
    {
        rarityText.text =
            GameManager.Instance.RareString(skill.rarity);

        nameText.text = skill.skillName;
        descriptionText.text = skill.skillDescription;

        iconImage.sprite = skill.icon;
        iconImage.enabled = skill.icon != null;
    }
}