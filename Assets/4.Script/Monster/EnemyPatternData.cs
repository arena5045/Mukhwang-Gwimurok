using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 하나의 패턴 단계 안에서
/// 적 행동을 선택하는 방식.
/// </summary>
public enum EnemyPatternMode
{
    // 1 → 2 → 3 → 1 → 2 → 3
    [LabelText("반복")]
    Loop,

    // 등록된 행동 중 하나를 무작위 선택
    [LabelText("무작위")]
    Random,

    // 1 → 2 → 3 → 3 → 3
    [LabelText("준비")]
    Prepare
}

/// <summary>
/// 적 행동에 포함할 수 있는 효과 종류.
/// </summary>
public enum EnemyActionEffectType
{
    [LabelText("공격")]
    Attack,

    [LabelText("공격력 증가")]
    GainAttack,

    [LabelText("방어력 증가")]
    GainDefense,

    [LabelText("방어도 증가")]
    GainShield,

    [LabelText("체력 증가")]
    GainHealth,
}

/// <summary>
/// 적 행동 하나에 포함되는 실제 효과.
/// </summary>
[Serializable]
public class EnemyActionEffectData
{
    [LabelText("효과 종류")]
    public EnemyActionEffectType effectType;

    [LabelText("피해 배율")]
    [ShowIf(
        nameof(effectType),
        EnemyActionEffectType.Attack)]
    [Min(0f)]
    public float damageMultiplier = 1f;

    [LabelText("타격 횟수")]
    [ShowIf(
        nameof(effectType),
        EnemyActionEffectType.Attack)]
    [Min(1)]
    public int hitCount = 1;

    [LabelText("증가량")]
    [HideIf(
        nameof(effectType),
        EnemyActionEffectType.Attack)]
    public float amount;
}

/// <summary>
/// 적이 한 차례에 실행하는 행동 하나.
/// 하나의 행동은 여러 효과를 가질 수 있다.
/// </summary>
[Serializable]
public class EnemyActionData
{
    [LabelText("행동 이름")]
    public string actionName;

    [LabelText("기척 설명")]
    [TextArea(2, 4)]
    public string intentDescription;

    [LabelText("효과 목록")]
    public List<EnemyActionEffectData> effects =
        new List<EnemyActionEffectData>();
}

/// <summary>
/// 적 패턴의 한 단계.
/// 선택 방식과 사용할 행동 목록을 가진다.
/// </summary>
[Serializable]
public class EnemyPatternPhaseData
{
    [LabelText("선택 방식")]
    public EnemyPatternMode mode;

    [LabelText("행동 목록")]
    public List<EnemyActionData> actions =
        new List<EnemyActionData>();

    [LabelText("전환까지 행동 횟수")]
    [Tooltip(
        "0이면 현재 단계를 계속 유지한다. " +
        "6이면 적이 6번 행동한 후 다음 단계로 넘어간다.")]
    [Min(0)]
    public int transitionAfterActions;

    [LabelText("다음 단계 번호")]
    [Tooltip(
        "-1이면 다음 단계로 넘어가지 않는다. " +
        "단계 번호는 0부터 시작한다.")]
    public int nextPhaseIndex = -1;
}

/// <summary>
/// 몬스터 하나가 가진 전체 패턴 구성.
/// </summary>
[Serializable]
public class EnemyPatternSetData
{
    [LabelText("시작 단계 번호")]
    [Min(0)]
    public int startPhaseIndex;

    [LabelText("패턴 단계")]
    public List<EnemyPatternPhaseData> phases =
        new List<EnemyPatternPhaseData>();
}