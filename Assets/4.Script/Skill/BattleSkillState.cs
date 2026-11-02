public sealed class BattleSkillState
{
    public OwnedSkill OwnedSkill { get; }

    // 이번 전투 전체에서 실제 발동한 횟수
    public int ActivationCount { get; private set; }

    // 현재 플레이어 행동 안에서 발동한 횟수
    public int ActivationsThisAction { get; private set; }

    // 현재 남은 쿨타임
    public int CooldownRemaining { get; private set; }

    // 이번 플레이어 턴에 직접 사용했는지
    public bool UsedActiveThisTurn { get; private set; }


    public SkillData Data => OwnedSkill.data;
    public int Level => OwnedSkill.level;

    public BattleSkillState(OwnedSkill ownedSkill)
    {
        OwnedSkill = ownedSkill;

        SkillLevelData levelData =
            Data.GetLevelData(Level);

        if (levelData != null)
        {
            // 첫 턴 BeginPlayerTurn()에서 1 감소하므로 +1
            CooldownRemaining =
                levelData.initialCooldownTurns + 1;
        }
    }

    //턴시작시 호출
    public void BeginPlayerTurn()
    {
        // 기존 행동당 패시브 발동 횟수도 새 턴에 초기화
        ActivationsThisAction = 0;
        UsedActiveThisTurn = false;

        if(CooldownRemaining > 0)
        {
            CooldownRemaining--;
        }
    }

    // 사용가능 여부 호출
    public bool CanUseActive()
    {
        // 기본기가 아닌 Manual 스킬은 한 차례에 한 번만 사용 가능
        bool alreadyUsed =
            UsedActiveThisTurn &&
            !Data.isBasicSkill;

        return
            !alreadyUsed &&
            CooldownRemaining <= 0;
    }

    // 액티브 스킬을 사용하고 난 뒤에 호출
    public void MarkActiveUsed()
    {
        // 일반 무공만 이번 차례 사용 완료로 기록한다.
        // 기본기는 행동력만 충분하면 반복 사용 가능하다.
        if (!Data.isBasicSkill)
        {
            UsedActiveThisTurn = true;
        }


        SkillLevelData levelData = Data.GetLevelData(Level);


        if (levelData == null)
        {
            return;
        }

        // 실제 쿨타임이 있는 스킬만 쿨타임을 시작한다.
        // 쿨타임 0인 기본기에는 CooldownRemaining 1을 넣지 않는다.
        if (levelData.cooldownTurns > 0)
        {
            CooldownRemaining =
                levelData.cooldownTurns + 1;
        }
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