using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 보상으로 등장할 수 있는 무공 목록을 보관한다.
/// 캐릭터 전용 풀과 모든 캐릭터 공용 풀에서 함께 사용한다.
/// </summary>
[CreateAssetMenu(
    fileName = "SkillRewardPool_",
    menuName = "Data/Skill Reward Pool")]
public sealed class SkillRewardPoolData : ScriptableObject
{
    [Header("보상 후보 무공")]
    [SerializeField]
    private List<SkillData> skills = new();

    public IReadOnlyList<SkillData> Skills =>
        skills;
}
