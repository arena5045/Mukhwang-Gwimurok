using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewMonster", menuName = "ScriptableObjects/MonsterData")]
public class Monster_So : ScriptableObject
{
    [Header("기본 정보")]
    public string monsterName;
    public Sprite monsterSprite;
    public float scaleModifier = 1.0f; // 몬스터 크기 배율

    [Header("전투 능력치")]
    public int maxHp;
    public float attackPower;
    public float defencePower;
    public float attackSpeed;

    [Header("전투 외 수치")]
    public float negoProbability;
    public float runProbability;


    [Header("드랍 아이템 리스트")]
    public int soul_drop;
    public int gold_drop;
    public List<DropItem> dropTable; // 아이템과 확률을 담은 별도 클래스 일단 임시로 이거로 저장
}

[System.Serializable]
public class DropItem
{
    public string itemName;
    public float dropChance; // 0.0 ~ 1.0
    public int amount;
}
