using System.Collections.Generic;
using UnityEngine;

public class GameContext
{
    // 플레이어 정보
    public PlayerData player;

    // 현재 위치 정보 (노드, 지역, 스테이지)
    //public MapNode currentNode;
    //public WorldMap worldMap;

    // 이벤트/선택지
    //public EventData currentEvent;
    //public List<Item> inventory;

    // 상태 관련
    //public List<StatusEffect> playerStatusEffects;

    // 전투 (있다면)
    //public Enemy currentEnemy;
    //public List<Enemy> enemies; // 전투일 경우

    // 로그 기록 (텍스트 기반일 때 중요)
    //public GameLog log;
    
    //맵상태
    public DungeonData cur_dungeon;

    public List<List<Vector2Int>> maps;
    public List<MapNodeData> map_nodes;
    public Dictionary<int, MapNodeData> map_nodes_id;
    public MapNodeButton cur_node;
    public MapNodeData cur_node_Data;

    

    // 턴 / 진행도
    public int currentDay;
    public int currentFloor;

    // 기타 시스템들
   // public FlagSystem flagSystem;        // "왕을 만난 적 있음" 같은 플래그
    //public RandomState random;           // 시드 기반 결정 통일용
}