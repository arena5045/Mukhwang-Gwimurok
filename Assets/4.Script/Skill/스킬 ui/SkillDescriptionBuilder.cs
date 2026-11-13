using System;
using System.Text;
using UnityEngine;

/// <summary>
/// 무공의 효과 데이터를 읽어
/// 전투 상세창과 무공 보상창에 표시할 설명을 만든다.
///
/// 전투 설명:
/// - 현재 적 방어력 반영
/// - 전투 중 능력치 보정 반영
/// - 기세와 활력 반영
///
/// 보상 설명:
/// - 보상 선택 후 수련 단계 반영
/// - 플레이어의 영구 공격력과 방어력만 사용
/// - 특정 적과 임시 전투 상태는 제외
/// </summary>
public static class SkillDescriptionBuilder
{
    private const string AttackValueColor =
        "#E05454";

    private const string DefenseValueColor =
        "#4FA8FF";

    private const string SpecialValueColor =
        "#F2C94C";

    /// <summary>
    /// 현재 전투 상황을 반영한 무공 설명을 만든다.
    /// 자동 설명을 지원하지 않으면 기존 SO 설명을 반환한다.
    /// </summary>
    public static string BuildBattleDescription(
        SkillData skill,
        int currentLevel,
        BattleManager battle)
    {
        SkillLevelData levelData =
            GetLevelData(
                skill,
                currentLevel);

        if (!CanBuildAutomatically(levelData) ||
            battle == null ||
            battle.currentPlayerInfo == null ||
            battle.currentMonsterInfo == null)
        {
            return GetFallbackDescription(
                skill);
        }

        // 활력은 이번 플레이어 차례의
        // 첫 번째 직접 타격에만 적용된다.
        bool canApplyVigor =
            !battle.currentPlayerInfo
                .hasAttackedThisTurn;

        return BuildDescription(
            skill,
            levelData,

            // 실제 전투 피해 계산을 그대로 사용한다.
            (damageMultiplier, includeVigor) =>
                battle.CalculatePlayerAttackDamage(
                    damageMultiplier,
                    includeVigor),

            // 전투 중 방어력 보정까지 포함한다.
            defensePercent =>
                battle.CalculatePlayerGuard(
                    defensePercent),

            canApplyVigor,
            "예상 총 피해");
    }

    /// <summary>
    /// 무공 보상 선택지에 표시할 설명을 만든다.
    ///
    /// 방금 전투한 적이나 전투 중 기세·활력은 제외하고,
    /// 플레이어의 영구 능력치를 기준으로 계산한다.
    /// </summary>
    public static string BuildRewardDescription(
        SkillData skill,
        int previewLevel,
        PlayerData player)
    {
        SkillLevelData levelData =
            GetLevelData(
                skill,
                previewLevel);

        if (!CanBuildAutomatically(levelData) ||
            player == null ||
            player.stats == null)
        {
            return GetFallbackDescription(
                skill);
        }

        return BuildDescription(
            skill,
            levelData,

            // 보상창에는 특정 적이 없으므로
            // 플레이어의 공격력과 계수만 계산한다.
            (damageMultiplier, includeVigor) =>
                CalculateRewardAttackDamage(
                    player,
                    damageMultiplier),

            // 영구 방어력을 기준으로 획득 방어도를 계산한다.
            defensePercent =>
                CalculateRewardGuard(
                    player,
                    defensePercent),

            // 보상창에서는 전투 중 활력을 적용하지 않는다.
            false,
            "현재 공격력 기준 총 피해");
    }

    /// <summary>
    /// 보상을 선택했을 때 표시할 수련 단계를 반환한다.
    ///
    /// 미보유 무공:
    /// 수련 1
    ///
    /// 이미 보유한 무공:
    /// 현재 수련 단계 + 1
    /// </summary>
    public static int GetRewardPreviewLevel(
        SkillData skill,
        PlayerData player)
    {
        if (skill == null ||
            skill.levels == null ||
            skill.levels.Count == 0)
        {
            return 0;
        }

        OwnedSkill ownedSkill =
            player?.GetOwnedSkill(
                skill);

        int previewLevel =
            ownedSkill == null
            ? 1
            : ownedSkill.level + 1;

        return Mathf.Clamp(
            previewLevel,
            1,
            skill.levels.Count);
    }

    /// <summary>
    /// 효과 목록을 등록된 순서대로 읽어
    /// 실제 설명 문장을 구성한다.
    ///
    /// 전투용과 보상용은 계산 방식만 다르고,
    /// 문장 형식은 이 메서드를 함께 사용한다.
    /// </summary>
    private static string BuildDescription(
        SkillData skill,
        SkillLevelData levelData,
        Func<float, bool, int> calculateAttackDamage,
        Func<float, int> calculateGuard,
        bool canApplyVigor,
        string totalDamageLabel)
    {
        StringBuilder description =
            new StringBuilder();

        // 모든 타격에 공통으로 들어가는 피해 총합
        int normalTotal = 0;

        // 활력처럼 첫 타격에만 추가되는 피해 총합
        int additionalTotal = 0;

        bool hasAttack = false;

        foreach (SkillEffectBase effect in levelData.effects)
        {
            // 비어 있는 효과 칸은 건너뛴다.
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
                    return GetFallbackDescription(
                        skill);
                }

                int hitDamage =
                    calculateAttackDamage(
                        attack.damageMultiplier,
                        false);

                normalTotal +=
                    hitDamage *
                    attack.hitCount;

                // 여러 공격 효과가 있어도
                // 활력은 첫 번째 타격에만 적용한다.
                if (canApplyVigor)
                {
                    int firstHitDamage =
                        calculateAttackDamage(
                            attack.damageMultiplier,
                            true);

                    additionalTotal +=
                        firstHitDamage -
                        hitDamage;

                    canApplyVigor = false;
                }

                AppendLineIfNeeded(
                    description);

                float coefficient =
                    attack.damageMultiplier *
                    100f;

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
                    calculateGuard(
                        guard.defensePercent);

                AppendLineIfNeeded(
                    description);

                description.Append(
                    $"{ColorValue(guardAmount, DefenseValueColor)} " +
                    $"(<sprite=\"DefenseICON\" index=0> " +
                    $"{guard.defensePercent:0.##}%)의 " +
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
                    return GetFallbackDescription(
                        skill);
                }

                string statusName =
                    statusEffect.statusData.statusName;

                string targetText =
                    statusEffect.target ==
                    BattleStatusTarget.Enemy
                    ? "적이 "
                    : "";

                string particle =
                    GetObjectParticle(
                        statusName);

                string changeText =
                    statusEffect.amount >= 0
                    ? "얻는다."
                    : "잃는다.";

                AppendLineIfNeeded(
                    description);

                description.Append(
                    $"{targetText}" +
                    $"{statusName}{particle} " +
                    $"{ColorValue(Mathf.Abs(statusEffect.amount),SpecialValueColor)} " +
                    $"{changeText}");

                continue;
            }

            // 일부 효과만 표시하면 실제 무공 내용과 달라진다.
            // 지원하지 않는 효과가 있으면 기존 SO 설명을 사용한다.
            return GetFallbackDescription(
                skill);
        }

        if (description.Length == 0)
        {
            return GetFallbackDescription(
                skill);
        }

        // 공격이 포함된 무공만 총 피해를 표시한다.
        if (hasAttack)
        {
            int totalDamage =
                normalTotal +
                additionalTotal;

            description.Append(
                $"\n\n{totalDamageLabel}: " +
                $"{ColorValue(totalDamage,AttackValueColor)}");

            // 활력처럼 별도 추가분이 있을 때만
            // 총 피해의 계산식을 함께 표시한다.
            if (additionalTotal > 0)
            {
                description.Append(
                    $" ({ColorValue( normalTotal,AttackValueColor)} + " +
                    $"{ColorValue( additionalTotal, SpecialValueColor)})");
            }
            else if (additionalTotal < 0)
            {
                description.Append(
                    $" ({ColorValue(normalTotal,AttackValueColor)} - " +
                    $"{ColorValue(  Mathf.Abs(additionalTotal), SpecialValueColor)})");
            }
        }

        return description.ToString();
    }

    /// <summary>
    /// 자동 설명을 만들 수 있는 기본 조건인지 확인한다.
    /// 조건부 무공의 조건 문장은 아직 지원하지 않는다.
    /// </summary>
    private static bool CanBuildAutomatically(
        SkillLevelData levelData)
    {
        if (levelData == null ||
            levelData.effects == null ||
            levelData.effects.Count == 0)
        {
            return false;
        }

        return levelData.conditions == null ||
               levelData.conditions.Count == 0;
    }

    /// <summary>
    /// 무공과 수련 단계에 맞는 레벨 데이터를 가져온다.
    /// </summary>
    private static SkillLevelData GetLevelData(
        SkillData skill,
        int level)
    {
        if (skill == null ||
            level < 1)
        {
            return null;
        }

        return skill.GetLevelData(
            level);
    }

    /// <summary>
    /// 보상창에서 플레이어의 영구 공격력을 기준으로
    /// 한 타격의 피해를 계산한다.
    ///
    /// 특정 적이 없으므로 적 방어력은 적용하지 않는다.
    /// </summary>
    private static int CalculateRewardAttackDamage(
        PlayerData player,
        float damageMultiplier)
    {
        float attack =
            player.stats.TotalAdAttack;

        if (attack <= 0f ||
            damageMultiplier <= 0f)
        {
            return 0;
        }

        float rawDamage =
            attack *
            damageMultiplier;

        return Mathf.Max(
            1,
            Mathf.RoundToInt(
                rawDamage));
    }

    /// <summary>
    /// 보상창에서 플레이어의 영구 방어력을 기준으로
    /// 획득할 방어도를 계산한다.
    /// </summary>
    private static int CalculateRewardGuard(
        PlayerData player,
        float defensePercent)
    {
        float defense =
            player.stats.TotalDefense;

        return Mathf.Max(
            0,
            Mathf.RoundToInt(
                defense *
                defensePercent /
                100f));
    }

    /// <summary>
    /// 앞에 표시된 효과가 있으면
    /// 다음 효과를 새 줄에 작성한다.
    /// </summary>
    private static void AppendLineIfNeeded(
        StringBuilder description)
    {
        if (description.Length > 0)
        {
            description.AppendLine();
        }
    }

    /// <summary>
    /// 수치를 효과 종류에 맞는 색상과 굵기로 표시한다.
    /// </summary>
    private static string ColorValue(
        int value,
        string color)
    {
        return
            $"<color={color}><b>{value}</b></color>";
    }

    /// <summary>
    /// 한글 단어 마지막 글자의 받침 여부에 따라
    /// 목적격 조사 '을/를'을 선택한다.
    /// </summary>
    private static string GetObjectParticle(
        string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return "을";
        }

        char lastChar =
            word[word.Length - 1];

        if (lastChar < 0xAC00 ||
            lastChar > 0xD7A3)
        {
            return "을";
        }

        return (lastChar - 0xAC00) % 28 != 0
            ? "을"
            : "를";
    }

    /// <summary>
    /// 자동 설명을 지원하지 않을 때
    /// SkillData에 저장된 기존 설명을 사용한다.
    /// </summary>
    private static string GetFallbackDescription(
        SkillData skill)
    {
        return skill != null
            ? skill.skillDescription
            : string.Empty;
    }
}