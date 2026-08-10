using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int numberOfLayers = 6;
    public int minNodesPerLayer = 2;
    public int maxNodesPerLayer = 4;
    public float spacingX = 250f;
    public float spacingY = 300f;
    public float startOffsetY = -600f;

    public float chanceToConnectTwoLayersUp = 0.2f;

    private int nodeIdCounter = 0;

    public List<MapNodeData> GenerateMap()
    {
        List<List<MapNodeData>> layeredNodes = new();
        List<MapNodeData> allNodes = new();

        // 1. 노드 생성
        for (int layer = 0; layer < numberOfLayers - 1; layer++)
        {
            int nodesInLayer = Random.Range(minNodesPerLayer, maxNodesPerLayer + 1);
            List<MapNodeData> currentLayer = new();

            for (int i = 0; i < nodesInLayer; i++)
            {
                var node = new MapNodeData
                {
                    id = nodeIdCounter++,
                    nodeType = (NodeType)Random.Range(0, 3),
                    position = new Vector2(i * spacingX, layer * spacingY + startOffsetY),
                    connectedNodeIds = new List<int>()
                };

                currentLayer.Add(node);
                allNodes.Add(node);
            }

            layeredNodes.Add(currentLayer);
        }

        // 2. 연결 설정 (위로 1층, 확률로 2층까지)
        for (int layer = 0; layer < numberOfLayers - 2; layer++)
        {
            List<MapNodeData> currentLayer = layeredNodes[layer];
            List<MapNodeData> nextLayer = layeredNodes[layer + 1];

            foreach (MapNodeData node in currentLayer)
            {
                int connections = Random.Range(1, Mathf.Min(3, nextLayer.Count + 1)); // 최소 1개 연결
                HashSet<int> connected = new();

                while (connected.Count < connections)
                {
                    int targetIndex = Random.Range(0, nextLayer.Count);
                    int targetId = nextLayer[targetIndex].id;
                    if (!connected.Contains(targetId))
                    {
                        connected.Add(targetId);
                        node.connectedNodeIds.Add(targetId);
                    }
                }

                // 확률적으로 2층 위 노드 연결
                if (layer + 2 < numberOfLayers - 1 && Random.value < chanceToConnectTwoLayersUp)
                {
                    var upperLayer = layeredNodes[layer + 2];
                    var target = upperLayer[Random.Range(0, upperLayer.Count)];
                    node.connectedNodeIds.Add(target.id);
                }
            }
        }

        // === 연결 후 보정 단계: 들어오는 연결이 없는 노드 강제 연결 ===
        for (int layer = 1; layer < numberOfLayers - 1; layer++)
        {
            var currentLayer = layeredNodes[layer];
            var prevLayer = layeredNodes[layer - 1];

            foreach (var node in currentLayer)
            {
                bool hasInbound = false;
                foreach (var prev in prevLayer)
                {
                    if (prev.connectedNodeIds.Contains(node.id))
                    {
                        hasInbound = true;
                        break;
                    }
                }

                if (!hasInbound)
                {
                    // 무작위 이전 층 노드 중 하나가 연결해줌
                    var fallback = prevLayer[Random.Range(0, prevLayer.Count)];
                    fallback.connectedNodeIds.Add(node.id);
                }
            }
        }

        // === 마지막 층: 보스 노드 1개 생성 ===
        List<MapNodeData> bossLayer = new();
        MapNodeData bossNode = new MapNodeData
        {
            id = nodeIdCounter++,
            nodeType = NodeType.Boss,
            position = new Vector2(0, (numberOfLayers - 1) * spacingY + startOffsetY),
            connectedNodeIds = new List<int>()
        };
        bossLayer.Add(bossNode);
        allNodes.Add(bossNode);
        layeredNodes.Add(bossLayer);

        // === 그 전 층의 모든 노드가 보스 노드와 연결되도록 강제 설정 ===
        List<MapNodeData> preBossLayer = layeredNodes[numberOfLayers - 2];
        foreach (var node in preBossLayer)
        {
            node.connectedNodeIds.Add(bossNode.id);
        }


        return allNodes;
    }
}
