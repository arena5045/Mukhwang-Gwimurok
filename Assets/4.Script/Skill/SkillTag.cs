using System;
using Sirenix.OdinInspector;

[Flags]
public enum SkillTag
{
    [LabelText("없음")]
    None = 0,

    // 큰 빌드 계열
    [LabelText("타격")]
    Attack = 1 << 0,

    [LabelText("혼")]
    Spirit = 1 << 1,

    [LabelText("기")]
    Gi = 1 << 2,

    [LabelText("체")]
    Body = 1 << 3,


    // 공격 성질
    [LabelText("참격")]
    Slash = 1 << 4,

    [LabelText("관통")]
    Pierce = 1 << 5,

    [LabelText("연타")]
    MultiHit = 1 << 6,

    [LabelText("추가타")]
    ExtraAttack = 1 << 7,

    [LabelText("치명타")]
    Critical = 1 << 8,

    [LabelText("반격")]
    Counter = 1 << 9,

    [LabelText("처형")]
    Execute = 1 << 10,


    // 상태이상
    [LabelText("출혈")]
    Bleed = 1 << 11,

    [LabelText("독")]
    Poison = 1 << 12,

    [LabelText("저주")]
    Curse = 1 << 13,

    [LabelText("화상")]
    Burn = 1 << 14,

    [LabelText("기절")]
    Stun = 1 << 15,


    // 생존 / 지원
    [LabelText("회복")]
    Heal = 1 << 16,

    [LabelText("흡혈")]
    LifeSteal = 1 << 17,

    [LabelText("보호")]
    Guard = 1 << 18,

    [LabelText("강화")]
    Buff = 1 << 19,

    [LabelText("약화")]
    Debuff = 1 << 20,


    // 자원 관련
    [LabelText("도력")]
    Mp = 1 << 21,

    [LabelText("체력")]
    Hp = 1 << 22,

    [LabelText("금화")]
    Gold = 1 << 23,

    [LabelText("혼백")]
    Soul = 1 << 24,

    [LabelText("경험치")]
    Exp = 1 << 25
}