using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public MapGenerator generator;
    public MapUIManager uiManager;

    public List<List<Vector2Int>> maps; // 맵 데이터 저장
    Dictionary<int, List<MapNodeData>> floorNodes = new(); // 각 층의 노드

    public MapNodeData cur_nodedata = null;
    public GameObject cur_mapnode = null;

    public int width = 7;       // 그리드 가로 크기 (열 개수)
    public int height = 15;     // 그리드 세로 크기 (층 수)
    public int pathCount = 6;   // 생성할 경로 수

    public int spacingX = 250; // 가로 간격
    public int spacingY = 300; // 세로 간격
    public int offsetY = -600; //보정값

    public bool isVertical;

    

    private void Awake()
    {
        // GameManager가 싱글턴으로 이미 준비되어 있다고 가정
        if (GameManager.Instance != null)
        {
            // GameManager의 MapManager 속성(Property)에 현재 인스턴스(this)를 할당
            GameManager.Instance.mapManager = this;
        }
        else
        {
            Debug.LogError("GameManager 인스턴스가 MapManager보다 먼저 초기화되지 않았습니다.");
        }
    }

    void Start()
    {
        //var mapData = generator.GenerateMap();
        //uiManager.CreateMapUI(mapData);

        var pathGen = new GridPathGenerator(width, height, pathCount);
        maps = pathGen.GeneratePaths(isVertical);


        var nodeConverter = new GridToNodeConverter(width, height);
        var nodeList = nodeConverter.ConvertToNodes(maps, isVertical, spacingX, spacingY, offsetY, out MapNodeData startNode);

        // ID를 Key로 하여 모든 노드에 접근할 수 있는 전역 딕셔너리
        Dictionary<int, MapNodeData> NodeDataById = new();

        // 모든 노드를 하나씩 확인
        foreach (var node in nodeList)
        {
            // 이 노드가 속한 층 정보를 가져옴 (ex. Y 좌표 기준으로 설정되어 있음)
            int floor = node.floor;

            // 아직 이 층(floor)에 대한 리스트가 없다면 새로 생성해서 Dictionary에 추가
            if (!floorNodes.ContainsKey(floor))
                floorNodes[floor] = new List<MapNodeData>();

            // 해당 층 리스트에 현재 노드를 추가
            floorNodes[floor].Add(node);

            //노드 id를 딕셔너리에 노드와 추가
            NodeDataById.Add(node.id, node);
        }
        // 기존 UI 시스템 재활용
        uiManager.CreateMapUI(nodeList);
        //매니저 컨텍스트에 값 저장
        GameManager.Instance.SetMap(maps, nodeList, NodeDataById);

        //현재 위치 = 시작위치
        Move_Node(startNode);
    }
    
    public void Move_Map(RectTransform node)
    {
        //노드이동 테스트
        uiManager.ScrollToNode(node);
    }
    //맵 데이터 이동
    public void Move_Node(MapNodeData node)
    {
        //이전 노드 상태 해제
        if (cur_nodedata != null && cur_nodedata.mapNodeBtn != null)
        {
            //이전 현재 노드를 클리어 상태로 변경
            cur_nodedata.mapNodeBtn.SetcheckData(NodeState.X);

            //이전 노드에서 보였던 다음 노드들의 강조 해제
            foreach (int num in cur_nodedata.connectedNodeIds)
            {
                // Next 상태에서 Unvisited로 되돌리거나, 혹은 다음 층 노드가 아니라면 Cleared 처리
                GameManager.Instance.Context.map_nodes_id[num].mapNodeBtn.SetcheckData(NodeState.None);
            }
        }
        //새 노드로 상태 업데이트
        cur_nodedata = node;
        cur_mapnode = node.mapNodeOb;

        //새 노드와 다음 노드 강조
        node.mapNodeBtn.SetcheckData(NodeState.Here);

        foreach(int num in node.connectedNodeIds)
        {
            GameManager.Instance.Context.map_nodes_id[num].mapNodeBtn.SetcheckData(NodeState.Next);
        }
    }
}