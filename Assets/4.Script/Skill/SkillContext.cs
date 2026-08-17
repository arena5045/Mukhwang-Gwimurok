using UnityEngine;

public sealed class SkillContext
{
    public BattleManager Battle { get; }
    public BattleSkillState State { get; }
    public SkillTrigger Trigger { get; }

    public SkillContext(BattleManager battle, BattleSkillState state, SkillTrigger trigger)
    {
        Battle = battle;
        State = state;
        Trigger = trigger;
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