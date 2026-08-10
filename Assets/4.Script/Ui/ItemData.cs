using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    [Header("Basic Information")]
    public int id;                // 고유 ID (저장/로드용)
    public string itemName;       // 아이템 이름
    [TextArea]
    public string description;    // 아이템 설명
    public Sprite icon;           // UI에 표시될 아이콘

    [Header("Economy")]
    public int gold_price;             // 상점 가격
    public int soul_price;
    public Rarity rarity;         // 등급 (Enum)

    // 아이템을 획득했을 때 실행될 가상 함수
    // 유물이라면 '공격 효과 리스트'에 추가하고, 스탯템이라면 '공격력'을 올리는 식
    public abstract void OnAcquire();
}

public enum Rarity { Common, Rare, Epic, Legendary }