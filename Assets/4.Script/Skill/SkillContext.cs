using UnityEngine;

public sealed class SkillContext
{
    public BattleManager Battle { get; }
    public BattleSkillState State { get; }
    public SkillTrigger Trigger { get; }
    public BattleSkillState SourceSkill { get; }
    public SkillChainContext Chain { get; }

    /*
= 지금 발동하려는 스킬

SourceSkill
= 이 발동을 만들어낸 직전 스킬

Chain
= 지금 이어지고 있는 연쇄 기록
        */
    public SkillContext(
        BattleManager battle,
        BattleSkillState state,
        SkillTrigger trigger,
        BattleSkillState sourceSkill = null,
        SkillChainContext chain = null)
    {
        Battle = battle;
        State = state;
        Trigger = trigger;
        SourceSkill = sourceSkill;
        Chain = chain;
    }


    //플레이어 체력 비율 반환
    public float PlayerHpRatio
    { get
        {
            if(Battle?.currentPlayerInfo ==null || Battle.currentPlayerInfo.maxhp <= 0)
            { 
                return 0f;
            }

            return (float)Battle.currentPlayerInfo.currentHp / Battle.currentPlayerInfo.maxhp;

        }
    }
    public int PlayerActionCount =>
      Battle != null ? Battle.PlayerActionCount : 0;
}