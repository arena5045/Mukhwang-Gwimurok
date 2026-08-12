using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDungeon", menuName = "ScriptableObjects/DungeonData")]
public class DungeonData : ScriptableObject
{
    public string dungeonName;
    public Sprite dungeonSprite;

    public List<Monster_So> normalMonsters;
    public List<Monster_So> eliteMonsters;
    public List<Monster_So> bossMonsters;

    public DialogueSequence startEvent;
    // 던전 진입 시 랜덤으로 몬스터 한 마리 뽑기
    public Monster_So GetRandomMonster()
    {
        return GetRandomMonsterFrom(normalMonsters, "일반");
    }

    public Monster_So GetRandomMonster_Elite()
    {
        return GetRandomMonsterFrom(eliteMonsters, "정예");
    }

    public Monster_So GetRandomMonster_Boss()
    {
        return GetRandomMonsterFrom(bossMonsters, "보스");
    }

    private Monster_So GetRandomMonsterFrom(List<Monster_So> monsters, string category)
    {
        // 던전 콘텐츠가 미완성인 동안 빈 목록이나 빠진 참조가 들어올 수 있다.
        // 호출부가 null을 받아 맵으로 복귀할 수 있도록 여기서는 예외 대신 실패를 반환한다.
        if (monsters == null || monsters.Count == 0)
        {
            Debug.LogError($"[DungeonData] {dungeonName} 던전에 {category} 몬스터가 없습니다.", this);
            return null;
        }

        int startIndex = Random.Range(0, monsters.Count);
        for (int offset = 0; offset < monsters.Count; offset++)
        {
            Monster_So monster = monsters[(startIndex + offset) % monsters.Count];
            if (monster != null) return monster;
        }

        Debug.LogError($"[DungeonData] {dungeonName} 던전의 {category} 몬스터 참조가 모두 비어 있습니다.", this);
        return null;
    }

    public DialogueSequence Call_StartEvent()
    {

        return startEvent;
    }
}
