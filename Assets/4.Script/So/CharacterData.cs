using UnityEngine;

[CreateAssetMenu(fileName = "Char_", menuName = "Data/Character")]
public class CharacterData : ScriptableObject
{
    public string charName;
    public Sprite portrait;     // 초상화
    public Sprite fullBody;     // 전체 모습
    public int startHp;
    public int startAtk;
    [TextArea] public string description; // 캐릭터 설명
}