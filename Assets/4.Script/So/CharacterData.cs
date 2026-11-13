using UnityEngine;

[CreateAssetMenu(fileName = "Char_", menuName = "Data/Character")]
public class CharacterData : ScriptableObject
{
    public string charName;
    public Sprite portrait;     // 초상화
    public Sprite fullBody;     // 전체 모습
    public int startHp;
    public int startAtk;

    [Header("무공 보상")]
    [Tooltip("이 캐릭터를 선택했을 때만 전투 보상으로 등장하는 무공풀")]
    public SkillRewardPoolData rewardSkillPool;

    [TextArea] public string description; // 캐릭터 설명
}