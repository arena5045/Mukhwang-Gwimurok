using Sirenix.OdinInspector;
public enum SkillTrigger
{
    [LabelText("턴 시작")]
    TurnStart,

    [LabelText("기본 공격")]
    BasicAttack,

    [LabelText("타격 후")]
    AfterHit,

    [LabelText("크리티컬 발생")]
    Critical,

    [LabelText("스킬 발동 후")]
    AfterSkill,

    [LabelText("턴 종료")]
    TurnEnd
}