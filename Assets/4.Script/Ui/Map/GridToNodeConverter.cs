using System.Collections.Generic;
using UnityEngine;

public class GridToNodeConverter
{
    private int width;
    private int height;

    public GridToNodeConverter(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    /*
    public List<MapNodeData> ConvertToNodes(List<List<Vector2Int>> paths, float spacingX = 250f, float spacingY = 300f, float offsetY = -600f)
    {
        Dictionary<Vector2Int, MapNodeData> nodeMap = new();
        List<MapNodeData> allNodes = new();
        int idCounter = 0;

        // === 1. 먼저 모든 노드 생성 ===
        foreach (var path in paths)
        {
            foreach (var coord in path)
            {
                if (!nodeMap.ContainsKey(coord))
                {
                    var node = new MapNodeData
                    {
                        id = idCounter++,
                        nodeType = (NodeType)Random.Range(0, 3),
                        position = new Vector2(
                            coord.x * spacingX,
                            coord.y * spacingY + offsetY
                        ),
                        connectedNodeIds = new List<int>()
                    };

                    nodeMap[coord] = node;
                    allNodes.Add(node);
                }
            }
        }

        // === 2. 그 다음 연결 설정 ===
        foreach (var path in paths)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                var fromCoord = path[i];
                var toCoord = path[i + 1];

                var fromNode = nodeMap[fromCoord];
                var toNode = nodeMap[toCoord];

                if (!fromNode.connectedNodeIds.Contains(toNode.id))
                    fromNode.connectedNodeIds.Add(toNode.id);
            }
        }

        // === 3. 보스 노드 생성 및 연결 ===
        Vector2Int bossCoord = new Vector2Int(width / 2, height); // 중앙 보스
        MapNodeData bossNode = new MapNodeData
        {
            id = idCounter++,
            nodeType = NodeType.Boss,
            position = new Vector2(
                bossCoord.x * spacingX,
                bossCoord.y * spacingY + offsetY
            ),
            connectedNodeIds = new List<int>()
        };

        nodeMap[bossCoord] = bossNode;
        allNodes.Add(bossNode);

        // 모든 경로의 마지막 노드에서 보스 연결
        foreach (var path in paths)
        {
            var lastCoord = path[path.Count - 1];
            var fromNode = nodeMap[lastCoord];

            if (!fromNode.connectedNodeIds.Contains(bossNode.id))
                fromNode.connectedNodeIds.Add(bossNode.id);
        }

        return allNodes;
    }
    */

    public List<MapNodeData> ConvertToNodes(List<List<Vector2Int>> paths, bool vertical, bool includeShop, float spacingX, float spacingY, float offsetY, out MapNodeData startNode)
    {
        // includeShop은 맵 생성 단계의 기능 플래그다.
        // false이면 아직 구현 중인 상점 노드를 후보에서 제외하되 전투·정예·이벤트 비율은 유지한다.
        //반환할 맵 데이터
        Dictionary<Vector2Int, MapNodeData> nodeMap = new();

        List<MapNodeData> allNodes = new();
        int idCounter = 0;


        // === 1. 먼저 모든 노드 생성 ===
        foreach (var path in paths)
        {
            foreach (var coord in path)
            {
                if (!nodeMap.ContainsKey(coord))
                {
                    var node = new MapNodeData
                    {
                        id = idCounter++,
                        nodeType = (NodeType)Random.Range(1, includeShop ? 5 : 4),
                        position = vertical
                        ? new Vector2(coord.x * spacingX, coord.y * spacingY + offsetY)
         :                new Vector2(coord.x * spacingX + offsetY, coord.y * spacingY),
                        connectedNodeIds = new List<int>(),
                        dataPosition = coord,
                        floor = vertical ? coord.y : coord.x
                };

                    nodeMap[coord] = node;
                    allNodes.Add(node);
                }
            }
        }

        // === 2. 그 다음 연결 설정 ===
        foreach (var path in paths)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                var fromCoord = path[i];
                var toCoord = path[i + 1];

                var fromNode = nodeMap[fromCoord];
                var toNode = nodeMap[toCoord];

                if (!fromNode.connectedNodeIds.Contains(toNode.id))
                    fromNode.connectedNodeIds.Add(toNode.id);
            }
        }


        // === 0. 시작 노드 생성 및 연결 ===
        Vector2Int startCoord = vertical
            ? new Vector2Int(width / 2, -1)     // 세로 모드일 때 아래
            : new Vector2Int(-1, height / 2);   // 가로 모드일 때 왼쪽

        Vector2 startPosition = vertical
            ? new Vector2(startCoord.x * spacingX, startCoord.y * spacingY + offsetY)
            : new Vector2(startCoord.x * spacingX + offsetY, startCoord.y * spacingY);

        startNode = new MapNodeData
        {
            id = idCounter++,
            nodeType = NodeType.Start, // NodeType에 Start 추가 필요
            position = startPosition,
            connectedNodeIds = new List<int>(),
            dataPosition = new Vector2(startCoord.x, startCoord.y),
            floor = startCoord.x
        };

        nodeMap[startCoord] = startNode;
        allNodes.Add(startNode);

        // 모든 경로의 첫 노드와 연결
        foreach (var path in paths)
        {
            var firstCoord = path[0];
            var toNode = nodeMap[firstCoord];

            if (!startNode.connectedNodeIds.Contains(toNode.id))
                startNode.connectedNodeIds.Add(toNode.id);
        }


        // === 3. 보스 노드 생성 및 연결 ===
        Vector2Int bossCoord = vertical
            ? new Vector2Int(width / 2, height)
            : new Vector2Int(width, height / 2);

        Vector2 bossPosition = vertical
            ? new Vector2(bossCoord.x * spacingX, bossCoord.y * spacingY + offsetY)
            : new Vector2(bossCoord.x * spacingX + offsetY, bossCoord.y * spacingY);

        MapNodeData bossNode = new MapNodeData
        {
            id = idCounter++,
            nodeType = NodeType.Boss,
            position = bossPosition,
            connectedNodeIds = new List<int>(),
            dataPosition = new Vector2(bossCoord.x, bossCoord.y),
            floor = bossCoord.x
        };

        nodeMap[bossCoord] = bossNode;
        allNodes.Add(bossNode);

        // 모든 경로의 마지막 노드에서 보스 연결
        foreach (var path in paths)
        {
            var lastCoord = path[path.Count - 1];
            var fromNode = nodeMap[lastCoord];

            if (!fromNode.connectedNodeIds.Contains(bossNode.id))
                fromNode.connectedNodeIds.Add(bossNode.id);
        }

        return allNodes;
    }

}
