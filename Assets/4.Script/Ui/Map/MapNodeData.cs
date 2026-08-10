using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Start,Battle,Elite_Battle, Event, Shop,Treasure,Boss
}
public enum NodeState
{
    None,Here,X,V,Out,Next   
}

[System.Serializable]
public class MapNodeData
{
    public int id;  // 유니크 ID
    public NodeType nodeType;
    public NodeState state;
    public List<int> connectedNodeIds = new List<int>();
    public Vector2 position; // UI상 위치
    public Vector2 dataPosition; //데이터상 위치

    public int floor;//현재 층수

    public GameObject mapNodeOb;
    public MapNodeButton mapNodeBtn;
}