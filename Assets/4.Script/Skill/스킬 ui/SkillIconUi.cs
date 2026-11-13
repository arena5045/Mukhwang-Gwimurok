using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SkillIconUI : MonoBehaviour
{
    /// <summary>
    /// 무공 아이콘에 공통으로 표시되는 정보를 관리한다.
    ///
    /// 표시 내용:
    /// - 무공 이미지
    /// - 행동력 비용
    /// - 희귀도 테두리
    /// - 현재/최대 수련 단계
    /// - 남은 재사용 대기시간
    ///
    /// 전투 상태나 보상 규칙은 알지 못하며,
    /// 호출한 UI가 필요한 값만 전달한다.
    /// 
    /// </summary>
    private SkillData skill;
    private int skillLevel;

    [Header("무공 아이콘")]
    [SerializeField]
    private Image skillIcon;

    [Header("무공 테두리")]
    [SerializeField]
    private Image skillBorder;

    [SerializeField]
    private Sprite commonBorder;

    [SerializeField]
    private Sprite rareBorder;

    [SerializeField]
    private Sprite epicBorder;

    [SerializeField]
    private Sprite legendaryBorder;

    [Header("무공 비용")]
    [SerializeField]
    private TMP_Text costText;

    [Header("무공 사용 불가 표시")]
    [SerializeField]
    private Image unavailableOverlay;

    [SerializeField]
    private TMP_Text cooldownText;

    [Header("무공 수련도")]
    [SerializeField]
    private List<SkillLevelPipUI> levelPips;



    /// <summary>
    /// 지정한 무공을 현재 수련 단계에 맞춰 표시한다.
    ///
    /// 무공 수련 단계는 1부터 시작하며,
    /// 전달된 값이 범위를 벗어나면 실제 최대 단계 안으로 보정한다.
    /// </summary>
    public void SetSkill(
        SkillData skillData,
        int currentLevel)
    {
        if (skillData == null ||
            skillData.levels == null ||
            skillData.levels.Count == 0)
        {
            Clear();
            return;
        }

        int maxLevel =
            skillData.levels.Count;

        int displayLevel =
            Mathf.Clamp(
                currentLevel,
                1,
                maxLevel);

        SkillLevelData levelData =
            skillData.GetLevelData(
                displayLevel);

        if (levelData == null)
        {
            Clear();
            return;
        }

        // 무공 이미지
        skillIcon.sprite =
            skillData.icon;

        skillIcon.enabled =
            skillData.icon != null;

        // 현재 수련 단계의 행동력 비용
        costText.text =
            levelData.actionPointCost.ToString();

        // 희귀도별 테두리
        skillBorder.sprite =
            GetRarityBorder(
                skillData.rarity);

        skillBorder.enabled =
            skillBorder.sprite != null;

        // 현재 단계와 최대 단계에 맞춰 구슬을 표시
        SetSkillLevelPips(
            displayLevel,
            maxLevel);

        // 보상창이나 새 슬롯에서 이전 전투 상태가 남지 않게 초기화한다.
        SetBattleState(true,0);
    }

    /// <summary>
    /// 남은 재사용 대기시간을 표시한다.
    /// 0 이하라면 재사용 대기시간 UI를 숨긴다.
    /// </summary>
    public void SetCooldown(
        int cooldownRemaining)
    {
        int remaining =
            Mathf.Max(
                0,
                cooldownRemaining);

        bool hasCooldown =
            remaining > 0;

        unavailableOverlay.gameObject.SetActive(
            hasCooldown);

        if (hasCooldown)
        {
            cooldownText.text =
                remaining.ToString();
        }
        else
        {
            cooldownText.text =
                string.Empty;
        }
    }

    /// <summary>
    /// 전투 중 무공의 사용 가능 상태를 표시한다.
    ///
    /// 사용할 수 없으면 검은 페이드를 표시하고,
    /// 그 원인이 재사용 대기시간일 때만 남은 숫자를 함께 표시한다.
    /// </summary>
    public void SetBattleState(
        bool canUse,
        int cooldownRemaining)
    {
        int remaining =
            Mathf.Max(
                0,
                cooldownRemaining);

        // 어떤 이유로든 현재 사용할 수 없으면
        // 검은 페이드를 표시한다.
        unavailableOverlay.gameObject.SetActive(
            !canUse);

        // 재사용 대기시간이 있을 때만
        // 검은 페이드 위에 남은 숫자를 표시한다.
        bool showCooldownNumber =
            !canUse &&
            remaining > 0;

        cooldownText.gameObject.SetActive(
            showCooldownNumber);

        cooldownText.text =
            showCooldownNumber
            ? remaining.ToString()
            : string.Empty;
    }


    /// <summary>
    /// 현재 수련 단계와 최대 수련 단계에 맞춰
    /// 각 핍의 표시 여부와 채움 상태를 갱신한다.
    /// </summary>
    private void SetSkillLevelPips(
        int currentLevel,
        int maxLevel)
    {
        for (int i = 0;
             i < levelPips.Count;
             i++)
        {
            // 최대 단계 안에 있는 핍만 표시한다.
            bool isVisible =
                i < maxLevel;

            // 현재 단계까지의 핍만 구슬로 채운다.
            bool isFilled =
                i < currentLevel;

            levelPips[i].SetState(
                isVisible,
                isFilled);
        }
    }

    /// <summary>
    /// 무공 희귀도에 맞는 테두리 이미지를 반환한다.
    /// </summary>
    private Sprite GetRarityBorder(
        Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return commonBorder;

            case Rarity.Rare:
                return rareBorder;

            case Rarity.Epic:
                return epicBorder;

            case Rarity.Legendary:
                return legendaryBorder;

            default:
                return commonBorder;
        }
    }

    /// <summary>
    /// 유효한 무공 데이터가 없을 때
    /// 이전 무공의 표시가 남지 않게 초기화한다.
    /// </summary>
    private void Clear()
    {
        skillIcon.sprite = null;
        skillIcon.enabled = false;

        skillBorder.sprite = null;
        skillBorder.enabled = false;

        costText.text =
            string.Empty;

        SetCooldown(0);

        foreach (SkillLevelPipUI pip in levelPips)
        {
            pip.SetState(
                false,
                false);
        }
    }
}