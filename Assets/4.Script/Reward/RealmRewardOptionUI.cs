using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RealmRewardOptionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button selectButton;

    private RealmRewardData currentReward;
    private Action<RealmRewardData> onSelected;

    public void Setup(
        RealmRewardData reward,
        Action<RealmRewardData> selectAction)
    {
        currentReward = reward;
        onSelected = selectAction;

        rarityText.text = GameManager.Instance.RareString(reward.rarity);
        nameText.text = reward.rewardName;
        descriptionText.text = reward.description;

        if (iconImage != null)
        {
            iconImage.sprite = reward.icon;
            iconImage.enabled = reward.icon != null;
        }

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(Select);
    }

    private void Select()
    {
        if (currentReward == null)
        {
            return;
        }

        onSelected?.Invoke(currentReward);
    }


}