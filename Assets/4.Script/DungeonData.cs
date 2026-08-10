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
        // 여기에 확률 로직을 넣어 엘리트나 보스가 가끔 나오게 할 수 있습니다.
        // 지금은 단순하게 일반 몬스터 중 하나를 뽑는 예시입니다.
        int rand = Random.Range(0, normalMonsters.Count);
        return normalMonsters[rand];
    }

    public Monster_So GetRandomMonster_Elite()
    {
        // 여기에 확률 로직을 넣어 엘리트나 보스가 가끔 나오게 할 수 있습니다.
        // 지금은 단순하게 일반 몬스터 중 하나를 뽑는 예시입니다.
        int rand = Random.Range(0, eliteMonsters.Count);
        return eliteMonsters[rand];
    }

    public Monster_So GetRandomMonster_Boss()
    {
        // 여기에 확률 로직을 넣어 엘리트나 보스가 가끔 나오게 할 수 있습니다.
        // 지금은 단순하게 일반 몬스터 중 하나를 뽑는 예시입니다.
        int rand = Random.Range(0, bossMonsters.Count);
        return bossMonsters[rand];
    }

    public DialogueSequence Call_StartEvent()
    {

        return startEvent;
    }
}
