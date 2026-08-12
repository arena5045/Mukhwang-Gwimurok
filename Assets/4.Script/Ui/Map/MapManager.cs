using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public MapGenerator generator;
    public MapUIManager uiManager;

    public List<List<Vector2Int>> maps; // 생성된 경로의 격자 좌표 목록
    Dictionary<int, List<MapNodeData>> floorNodes = new(); // 층별 노드 조회용 런타임 캐시

    public MapNodeData cur_nodedata = null;
    public GameObject cur_mapnode = null;

    public int width = 7;       // 그리드 가로 크기 (열 개수)
    public int height = 15;     // 그리드 세로 크기 (층 수)
    public int pathCount = 6;   // 생성할 경로 수

    public int spacingX = 250; // 가로 간격
    public int spacingY = 300; // 세로 간격
    public int offsetY = -600; //보정값

    public bool isVertical;

    [Header("기능 플래그")]
    [Tooltip("상점 구현이 완료되기 전까지 절차 맵에서 상점 노드를 생성하지 않습니다.")]
    public bool enableShop = false;

    

    private void OnDestroy()
    {
        // GameManager와 MapManager는 같은 씬에 있지만 Unity의 파괴 순서는 보장되지 않는다.
        // GameManager가 아직 살아 있는 경우에만 등록을 해제하여 파괴된 맵 UI 참조를 남기지 않는다.
        GameManager.Instance?.UnregisterMap(this);
    }

    private void Start()
    {
        // Awake에서는 다른 매니저의 초기화 완료를 가정하지 않는다.
        // 모든 객체의 Awake가 끝난 Start 시점에 의존성을 확인하고 맵 생성 절차를 시작한다.
        if (GameManager.Instance == null || uiManager == null)
        {
            Debug.LogError("[MapManager] GameManager 또는 MapUIManager가 준비되지 않았습니다.", this);
            return;
        }

        // 1. 절차 경로와 노드 데이터를 메모리상에 생성한다.
        floorNodes.Clear();
        var pathGen = new GridPathGenerator(width, height, pathCount);
        maps = pathGen.GeneratePaths(isVertical);


        var nodeConverter = new GridToNodeConverter(width, height);
        var nodeList = nodeConverter.ConvertToNodes(maps, isVertical, enableShop, spacingX, spacingY, offsetY, out MapNodeData startNode);

        // 2. 연결 ID로 즉시 노드를 찾을 수 있도록 조회용 딕셔너리를 구성한다.
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
        // 3. GameManager가 새 런을 만들고 Context에 맵 데이터를 등록한다.
        // 등록이 실패하면 불완전한 Context로 UI가 동작하지 않도록 여기서 중단한다.
        if (!GameManager.Instance.RegisterMap(this, maps, nodeList, NodeDataById))
        {
            return;
        }

        // 4. 등록된 데이터로 실제 버튼과 연결선을 만든 뒤 시작 노드 상태를 적용한다.
        uiManager.CreateMapUI(nodeList);
        Move_Node(startNode);

        // 5. 노드의 GameObject 참조까지 모두 연결된 뒤에만 시작 이벤트와 UI 갱신을 허용한다.
        GameManager.Instance.NotifyMapReady(this);
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
