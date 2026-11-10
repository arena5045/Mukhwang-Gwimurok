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
    private string BuildDescription(
        SkillData skill,
        SkillLevelData levelData)
    {
        BattleManager battle =
            BattleManager.instance;

        // 자동 설명을 만들 수 없는 상태라면 기존 설명 사용
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


        // ─────────────────────
        // 방어도 획득
        // ─────────────────────
        if (levelData.effects.Count == 1 &&
            levelData.effects[0] is GainGuardEffect guard)
        {
            int guardAmount =
                battle.CalculatePlayerGuard(
                    guard.defensePercent);

            return
                $"{ColorValue(guardAmount, DefenseValueColor)} " +
                $"(<sprite=\"DefenseICON\" index=0> {guard.defensePercent:0.##}%)의 " +
                $"방어도를 얻는다.";
        }


        // ─────────────────────
        // 전투 상태 획득 / 감소
        // ─────────────────────
        if (levelData.effects.Count == 1 &&
            levelData.effects[0] is GainBattleStatusEffect statusEffect &&
            statusEffect.statusData != null)
        {
            string statusName =
                statusEffect.statusData.statusName;

            int amount =
                statusEffect.amount;

            string targetText =
                statusEffect.target == BattleStatusTarget.Enemy
                ? "적이 "
                : "";

            string particle =
                GetObjectParticle(statusName);

            string changeText =
                amount >= 0
                ? "얻는다."
                : "잃는다.";

            return
                $"{targetText}" +
                $"{statusName}{particle} " +
                $"{ColorValue(Mathf.Abs(amount), SpecialValueColor)} " +
                $"{changeText}";
        }


        // ─────────────────────
        // 공격
        // ─────────────────────

        // 현재는 모든 효과가 AttackEffect인 스킬만
        // 공격 자동 설명을 지원한다.
        foreach (SkillEffectBase effect in levelData.effects)
        {
            if (!(effect is AttackEffect attack) ||
                attack.hitCount <= 0)
            {
                return skill.skillDescription;
            }
        }

        StringBuilder description =
            new StringBuilder();

        // 활력을 제외한 일반 타격 피해 총합
        int normalTotal = 0;

        // 활력처럼 일부 타격에만 추가되는 피해
        int additionalTotal = 0;

        // 이번 차례 첫 타격에 활력이 적용 가능한지
        bool canApplyVigor =
            !battle.currentPlayerInfo.hasAttackedThisTurn;


        foreach (SkillEffectBase effect in levelData.effects)
        {
            AttackEffect attack =
                (AttackEffect)effect;

            // 공격력 보정 + 기세까지 반영한 일반 1타 피해
            int hitDamage =
                battle.CalculatePlayerAttackDamage(
                    attack.damageMultiplier,
                    false);

            normalTotal +=
                hitDamage * attack.hitCount;


            // 활력은 첫 타격에만 적용
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


            // 공격 효과가 여러 개라면 줄바꿈
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
        }


        // ─────────────────────
        // 예상 총 피해
        // ─────────────────────

        int totalDamage =
            normalTotal + additionalTotal;

        description.Append(
            $"\n\n예상 총 피해: " +
            $"{ColorValue(totalDamage, AttackValueColor)}");


        // 활력 등 별도 추가 피해가 있을 때만 계산식 표시
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