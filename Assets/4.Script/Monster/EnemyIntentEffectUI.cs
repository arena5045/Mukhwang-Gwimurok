using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyIntentEffectUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text valueText;

    public void Setup(
        Sprite icon,
        string value)
    {
        bool hasIcon = icon != null;

        iconImage.gameObject.SetActive(hasIcon);

        if (hasIcon)
        {
            iconImage.sprite = icon;
        }

        valueText.text = value;
    }
}