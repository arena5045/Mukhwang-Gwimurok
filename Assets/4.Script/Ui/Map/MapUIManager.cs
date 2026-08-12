using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapUIManager : MonoBehaviour
{
    public GameObject nodePrefab;
    public Transform nodeParent;
    public LineRenderer linePrefab;
    public GameObject uiLinePrefab;

    public ScrollRect scrollRect;         // 맵화면 ScrollRect 컴포넌트
    public RectTransform viewport;        // 맵호면 뷰포트의 컴포넌트
    public RectTransform content;         // 맵화면 Content의 RectTransform

    public float mapview_ratio = 0.5f; // 맵뷰 비율 0~1ㄹ 

    [SerializeField] float dashLen = 32f;      // 세그먼트 길이(px)
    [SerializeField] float gapLen = 24f;       // 세그먼트 사이 간격(px)
    [SerializeField] float thickness = 10f;    // 세그먼트 두께(px)

    private Dictionary<int, GameObject> nodeObjects = new();

    public void CreateMapUI(List<MapNodeData> mapData)
    {
        nodeObjects.Clear();

        foreach (MapNodeData nodeData in mapData)
        {
            GameObject node = Instantiate(nodePrefab, nodeParent);
            node.name = nodeData.nodeType.ToString()+ $"{{{nodeData.dataPosition}}}";

            float x_rand = Random.Range(-40f, 40f);         // ±5px 길이 흔들림
            float y_rand = Random.Range(-40f, 40f);         // ±5px 길이 흔들림

            nodeData.position += new Vector2(x_rand, y_rand);

            node.GetComponent<RectTransform>().anchoredPosition = nodeData.position;
            //node.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = nodeData.nodeType.ToString();
            nodeObjects[nodeData.id] = node;

            nodeData.mapNodeOb  = node;
            node.GetComponent<MapNodeButton>().SetNodeData(nodeData);
        }


        foreach (MapNodeData nodeData in mapData)
        {
            Vector2 from = nodeObjects[nodeData.id].GetComponent<RectTransform>().anchoredPosition;

            foreach (int targetId in nodeData.connectedNodeIds)
            {
                Vector2 to = nodeObjects[targetId].GetComponent<RectTransform>().anchoredPosition;
                DrawUILine(from, to);
            }
        }
     
    }
    /*
        void DrawUILine(Vector2 from, Vector2 to)
    {
        // 방향/길이/각도
        Vector2 dir = (to - from);
        float distance = dir.magnitude;
        if (distance < 1f) return;
        dir /= distance;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // === 선 길이 줄이기 ===
        float shortenPercent = 0.1f; // 10%씩 앞뒤 자르면 총 20% 줄어듦
        from += dir * (distance * shortenPercent);
        to -= dir * (distance * shortenPercent);

        // 업데이트된 거리 다시 계산
        dir = (to - from);
        distance = dir.magnitude;
        dir /= distance;

        // 몇 개를 놓을 것인가
        float step = dashLen + gapLen;                       // 한 칸 길이
        int count = Mathf.Max(1, Mathf.FloorToInt((distance + gapLen) / step));

        // 좌우 가장자리 여백 정렬(가운데 정렬)
        float used = count * dashLen + (count - 1) * gapLen; // 실제 사용 길이
        float startOffset = (distance - used) * 0.5f;

        for (int i = 0; i < count; i++)
        {
            // 각 세그먼트의 중심 위치
            float along = startOffset + (dashLen * 0.5f) + i * step;
            Vector2 pos = from + dir * along;

            // === 무작위 흔들림 파라미터 ===
            float angleOffset = Random.Range(-10f, 10f);        // ±3도 각도 흔들림
            float dashJitter = Random.Range(-5f, 5f);         // ±5px 길이 흔들림
            float thicknessJitter = Random.Range(-1f, 1f);    // ±1px 두께 흔들림
            Vector2 positionJitter = new Vector2(
                Random.Range(-2f, 2f), // X 좌우 흔들림
                Random.Range(-2f, 2f)  // Y 상하 흔들림
            );

            // 세그먼트 생성(가능하면 오브젝트 풀 사용)
            var seg = Instantiate(uiLinePrefab, nodeParent).GetComponent<RectTransform>();
            seg.anchoredPosition = pos + positionJitter;
            seg.sizeDelta = new Vector2(dashLen + dashJitter, thickness + thicknessJitter);
            seg.rotation = Quaternion.Euler(0, 0, angle + angleOffset);
            seg.SetAsFirstSibling();
        }
    }
    */

    void DrawUILine(Vector2 from, Vector2 to)
    {
        Vector2 dir = (to - from);
        float distance = dir.magnitude;
        if (distance < 1f) return;

        dir /= distance;

        // 양 끝을 고정값만큼 잘라냄
        float trimAmount = 50f;
        Vector2 trimmedFrom = from + dir * trimAmount;
        float trimmedDistance = distance - trimAmount * 2f;

        // 점선 개수 계산
        float step = dashLen + gapLen;
        int count = Mathf.Max(1, Mathf.FloorToInt((trimmedDistance + gapLen) / step));

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        for (int i = 0; i < count; i++)
        {
            // 각 세그먼트의 중심 위치
            float along = (dashLen * 0.5f) + i * step;
            Vector2 pos = trimmedFrom + dir * along;

            // === 무작위 흔들림 파라미터 ===
            float angleOffset = Random.Range(-10f, 10f);        // ±3도 각도 흔들림
            float dashJitter = Random.Range(-5f, 5f);         // ±5px 길이 흔들림
            float thicknessJitter = Random.Range(-1f, 1f);    // ±1px 두께 흔들림
            Vector2 positionJitter = new Vector2(
                Random.Range(-2f, 2f), // X 좌우 흔들림
                Random.Range(-2f, 2f)  // Y 상하 흔들림
            );

            // 세그먼트 생성(가능하면 오브젝트 풀 사용)
            var seg = Instantiate(uiLinePrefab, nodeParent).GetComponent<RectTransform>();
            seg.anchoredPosition = pos + positionJitter;
            seg.sizeDelta = new Vector2(dashLen + dashJitter, thickness + thicknessJitter);
            seg.rotation = Quaternion.Euler(0, 0, angle + angleOffset);
            seg.SetAsFirstSibling();
        }
    }

    public void ScrollToNode(RectTransform target)
    {
        // 1. 버튼(target)의 현재 월드 좌표를 콘텐츠(content) 기준의 로컬 좌표로 변환합니다.
        // 계층 구조가 복잡해도(자식의 자식이라도) 정확한 위치를 찾아줍니다.
        Vector3 targetLocalPos = content.InverseTransformPoint(target.position);

        // 2. 버튼이 뷰포트 중앙에 오기 위한 콘텐츠의 목표 X 좌표 계산
        // 버튼의 위치만큼 반대로 가되, 뷰포트 너비의 절반만큼 더해주면 중앙에 옵니다.
        float targetX = -targetLocalPos.x + (viewport.rect.width * mapview_ratio);

        // 3. 콘텐츠가 이동 가능한 범위를 제한 (Clamping)
        // 콘텐츠가 너무 왼쪽이나 오른쪽으로 가서 빈 공간이 보이지 않게 막아줍니다.
        float contentWidth = content.rect.width;
        float viewportWidth = viewport.rect.width;

        // 콘텐츠의 Pivot이 왼쪽(0)일 때 기준 범위
        float minX = -(contentWidth - viewportWidth);
        float maxX = 0;

        targetX = Mathf.Clamp(targetX, minX, maxX);

        // 4. 즉시 적용 (이 한 줄이 DOTween 없이 바로 이동시키는 핵심입니다)
        content.anchoredPosition = new Vector2(targetX, content.anchoredPosition.y);
    }
}
