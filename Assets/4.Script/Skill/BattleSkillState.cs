public sealed class BattleSkillState
{
    public OwnedSkill OwnedSkill { get; }

    // 이번 전투 전체에서 실제 발동한 횟수
    public int ActivationCount { get; private set; }

    // 현재 플레이어 행동 안에서 발동한 횟수
    public int ActivationsThisAction { get; private set; }

    public SkillData Data => OwnedSkill.data;
    public int Level => OwnedSkill.level;

    public BattleSkillState(OwnedSkill ownedSkill)
    {
        OwnedSkill = ownedSkill;
    }

    public bool CanActivateThisAction()
    {
        SkillLevelData levelData = Data.GetLevelData(Level);

        if (levelData == null)
        {
            return false;
        }

        return ActivationsThisAction < levelData.maxActivationsPerAction;
    }

    // 조건을 통과한 순간 횟수를 먼저 잡아둔다.
    // 연쇄 발동 중 같은 스킬이 다시 들어오는 것을 막기 위함.
    public void ReserveActivation()
    {
        ActivationsThisAction++;
    }

    // 실제 효과 실행이 끝났을 때 전투 전체 발동 횟수 증가
    public void MarkActivated()
    {
        ActivationCount++;
    }

    // 새 플레이어 행동이 시작되면 행동 단위 횟수만 초기화
    public void ResetAction()
    {
        ActivationsThisAction = 0;
    }
}