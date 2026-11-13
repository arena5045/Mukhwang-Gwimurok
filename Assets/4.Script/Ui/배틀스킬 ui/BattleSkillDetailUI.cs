using System.Text;
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

    [Header("현재 스킬 상태")]
    [SerializeField] private GameObject cooldownOverlay;
    [SerializeField] private TMP_Text cooldownRemainingText;

    private const string AttackValueColor = "#E05454";
    private const string DefenseValueColor = "#4FA8FF";
    private const string SpecialValueColor = "#F2C94C";

    private static string ColorValue(
        int value,
        string color)
    {
        return $"<color={color}><b>{value}</b></color>";
    }

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
        // 스킬이 선택되면 상세 정보창을 표시
        gameObject.SetActive(true);


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
        descriptionText.text = BuildDescription(skill, levelData);

        // 행동력 비용
        actionPointText.text =
            levelData.actionPointCost.ToString();

        // 재사용 관련 정보
        cooldownText.text =
            $"재사용 대기시간: {levelData.cooldownTurns}";

        initialCooldownText.text =
            $"준비시간: {levelData.initialCooldownTurns}";

        // 현재 남은 재사용 대기시간 표시
        if (state.CooldownRemaining > 0)
        {
            cooldownOverlay.SetActive(true);
            cooldownRemainingText.text =
                state.CooldownRemaining.ToString();
        }
        else
        {
            cooldownOverlay.SetActive(false);
        }


        // 일단 기본 문자열로 표시
        tagText.text =
            skill.tags.ToString();
    }

    /// <summary>
    /// 현재 스킬의 효과를 읽어 전투 상황이 반영된 설명을 만든다.
    /// 아직 자동 설명을 지원하지 않는 스킬은 기존 설명을 유지한다.
    /// </summary>
    /// <summary>
    /// 현재 스킬의 효과를 순서대로 읽어 설명을 만든다.
    /// 아직 자동 설명을 지원하지 않는 효과가 포함되어 있으면
    /// 기존 스킬 설명을 사용한다.
    /// </summary>
    private string BuildDescription(
        SkillData skill,
        SkillLevelData levelData)
    {
        BattleManager battle =
            BattleManager.instance;

        if (battle == null ||
            battle.currentPlayerInfo == null ||
            battle.currentMonsterInfo == null ||
            levelData.effects == null ||
            levelData.effects.Count == 0)
        {
            return skill.skillDescription;
        }

        // 조건부 스킬은 아직 자동 설명하지 않는다.
        if (levelData.conditions != null &&
            levelData.conditions.Count > 0)
        {
            return skill.skillDescription;
        }

        StringBuilder description =
            new StringBuilder();

        int normalTotal = 0;
        int additionalTotal = 0;

        bool hasAttack = false;

        // 활력은 이번 차례의 첫 공격 타격에만 적용된다.
        bool canApplyVigor =
            !battle.currentPlayerInfo.hasAttackedThisTurn;


        foreach (SkillEffectBase effect in levelData.effects)
        {
            if (effect == null)
            {
                continue;
            }


            // ─────────────────────
            // 공격
            // ─────────────────────
            if (effect is AttackEffect attack)
            {
                if (attack.hitCount <= 0)
                {
                    return skill.skillDescription;
                }

                int hitDamage =
                    battle.CalculatePlayerAttackDamage(
                        attack.damageMultiplier,
                        false);

                normalTotal +=
                    hitDamage * attack.hitCount;

                if (canApplyVigor)
                {
                    int firstHitDamage =
                        battle.CalculatePlayerAttackDamage(
                            attack.damageMultiplier,
                            true);

                    additionalTotal +=
                        firstHitDamage - hitDamage;

                    canApplyVigor = false;
                }

                if (description.Length > 0)
                {
                    description.AppendLine();
                }

                float coefficient =
                    attack.damageMultiplier * 100f;

                description.Append(
                    $"{ColorValue(hitDamage, AttackValueColor)} " +
                    $"(<sprite=0> {coefficient:0.##}%)의 피해");

                if (attack.hitCount == 1)
                {
                    description.Append(
                        "를 입힌다.");
                }
                else
                {
                    description.Append(
                        $"로 <b>{attack.hitCount}</b>회 공격한다.");
                }

                hasAttack = true;
                continue;
            }


            // ─────────────────────
            // 방어도 획득
            // ─────────────────────
            if (effect is GainGuardEffect guard)
            {
                int guardAmount =
                    battle.CalculatePlayerGuard(
                        guard.defensePercent);

                if (description.Length > 0)
                {
                    description.AppendLine();
                }

                description.Append(
                    $"{ColorValue(guardAmount, DefenseValueColor)} " +
                    $"(<sprite=\"DefenseICON\" index=0> {guard.defensePercent:0.##}%)의 " +
                    $"방어도를 얻는다.");

                continue;
            }


            // ─────────────────────
            // 전투 상태 획득 / 감소
            // ─────────────────────
            if (effect is GainBattleStatusEffect statusEffect)
            {
                if (statusEffect.statusData == null)
                {
                    return skill.skillDescription;
                }

                string statusName =
                    statusEffect.statusData.statusName;

                string targetText =
                    statusEffect.target == BattleStatusTarget.Enemy
                    ? "적이 "
                    : "";

                string particle =
                    GetObjectParticle(statusName);

                string changeText =
                    statusEffect.amount >= 0
                    ? "얻는다."
                    : "잃는다.";

                if (description.Length > 0)
                {
                    description.AppendLine();
                }

                description.Append(
                    $"{targetText}" +
                    $"{statusName}{particle} " +
                    $"{ColorValue(Mathf.Abs(statusEffect.amount), SpecialValueColor)} " +
                    $"{changeText}");

                continue;
            }


            // 아직 자동 설명을 지원하지 않는 효과가 하나라도 있으면
            // 일부 효과만 표시하지 않고 기존 설명을 사용한다.
            return skill.skillDescription;
        }


        if (description.Length == 0)
        {
            return skill.skillDescription;
        }


        // ─────────────────────
        // 예상 총 피해
        // ─────────────────────
        if (hasAttack)
        {
            int totalDamage =
                normalTotal + additionalTotal;

            description.Append(
                $"\n\n예상 총 피해: " +
                $"{ColorValue(totalDamage, AttackValueColor)}");

            if (additionalTotal > 0)
            {
                description.Append(
                    $" ({ColorValue(normalTotal, AttackValueColor)} + " +
                    $"{ColorValue(additionalTotal, SpecialValueColor)})");
            }
            else if (additionalTotal < 0)
            {
                description.Append(
                    $" ({ColorValue(normalTotal, AttackValueColor)} - " +
                    $"{ColorValue(Mathf.Abs(additionalTotal), SpecialValueColor)})");
            }
        }

        return description.ToString();
    }
    private static string GetObjectParticle(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return "을";
        }

        char lastChar =
            word[word.Length - 1];

        // 한글 음절 범위가 아니면 기본값
        if (lastChar < 0xAC00 ||
            lastChar > 0xD7A3)
        {
            return "을";
        }

        // 종성이 있으면 을, 없으면 를
        return (lastChar - 0xAC00) % 28 != 0
            ? "을"
            : "를";
    }

    /// <summary>
    /// 선택된 스킬이 없으면 상세 정보창을 숨긴다.
    /// </summary>
    public void Clear()
    {
        gameObject.SetActive(false);
    }
}