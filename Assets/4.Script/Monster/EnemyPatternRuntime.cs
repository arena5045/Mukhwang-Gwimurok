using UnityEngine;

/// <summary>
/// 전투 중 적 패턴의 현재 진행 상태를 관리한다.
/// Monster_So 원본 데이터는 수정하지 않는다.
/// </summary>
public sealed class EnemyPatternRuntime
{
    private readonly EnemyPatternSetData patternData;

    // 현재 진행 중인 패턴 단계
    public int PhaseIndex { get; private set; }

    // 현재 단계 안에서 다음에 선택할 행동 순서
    private int sequenceIndex;

    // 현재 단계에서 적이 실제로 행동한 횟수
    private int actionsUsedInPhase;

    // 플레이어에게 현재 예고 중인 행동
    public EnemyActionData CurrentAction { get; private set; }

    public EnemyPatternRuntime(
        EnemyPatternSetData data)
    {
        patternData = data;

        // 모든 몬스터는 첫 번째 단계에서 시작
        PhaseIndex = 0;
        sequenceIndex = 0;
        actionsUsedInPhase = 0;
    }

    /// <summary>
    /// 다음 적 행동을 선택한다.
    /// 이미 예고된 행동이 있다면 같은 행동을 그대로 반환한다.
    /// </summary>
    public EnemyActionData PrepareNextAction()
    {
        if (CurrentAction != null)
        {
            return CurrentAction;
        }

        EnemyPatternPhaseData phase =
            GetCurrentPhase();

        if (phase == null ||
            phase.actions == null ||
            phase.actions.Count == 0)
        {
            return null;
        }

        int actionIndex;

        switch (phase.mode)
        {
            case EnemyPatternMode.Loop:
                // 1 → 2 → 3 → 1 → 2 → 3
                actionIndex =
                    sequenceIndex %
                    phase.actions.Count;
                break;

            case EnemyPatternMode.Random:
                // 목록 안에서 무작위 선택
                actionIndex =
                    Random.Range(
                        0,
                        phase.actions.Count);
                break;

            case EnemyPatternMode.Prepare:
                // 1 → 2 → 3 → 3 → 3
                actionIndex =
                    Mathf.Min(
                        sequenceIndex,
                        phase.actions.Count - 1);
                break;

            default:
                return null;
        }

        CurrentAction =
            phase.actions[actionIndex];

        return CurrentAction;
    }

    /// <summary>
    /// 현재 예고된 적 행동이 실제로 끝났을 때 호출한다.
    /// 행동 순서를 진행하고 필요하면 다음 단계로 전환한다.
    /// </summary>
    public void CompleteCurrentAction()
    {
        if (CurrentAction == null)
        {
            return;
        }

        CurrentAction = null;

        sequenceIndex++;
        actionsUsedInPhase++;

        EnemyPatternPhaseData phase =
            GetCurrentPhase();

        if (phase == null)
        {
            return;
        }

        // 0이면 현재 단계를 계속 유지
        if (phase.transitionAfterActions <= 0)
        {
            return;
        }

        if (actionsUsedInPhase <
            phase.transitionAfterActions)
        {
            return;
        }

        ChangePhase(
            phase.nextPhaseIndex);
    }

    /// <summary>
    /// 현재 단계 데이터를 가져온다.
    /// </summary>
    private EnemyPatternPhaseData GetCurrentPhase()
    {
        if (patternData == null ||
            patternData.phases == null ||
            PhaseIndex < 0 ||
            PhaseIndex >= patternData.phases.Count)
        {
            return null;
        }

        return patternData.phases[PhaseIndex];
    }

    /// <summary>
    /// 유효한 다음 단계로 전환하고
    /// 단계 안의 진행 상태를 초기화한다.
    /// </summary>
    private void ChangePhase(int nextPhaseIndex)
    {
        if (patternData == null ||
            patternData.phases == null ||
            nextPhaseIndex < 0 ||
            nextPhaseIndex >= patternData.phases.Count)
        {
            return;
        }

        PhaseIndex = nextPhaseIndex;
        sequenceIndex = 0;
        actionsUsedInPhase = 0;
        CurrentAction = null;
    }
}