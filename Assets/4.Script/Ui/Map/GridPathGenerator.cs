using System.Collections.Generic;
using UnityEngine;

public class GridPathGenerator
{
    public int width = 7;       // 그리드 가로 크기 (열 개수)
    public int height = 15;     // 그리드 세로 크기 (층 수)
    public int pathCount = 6;   // 생성할 경로 수

    private System.Random rng = new(); // C#용 랜덤 (Unity Random과 다름)

    public GridPathGenerator(int width, int height,int pathCount)
    {
        this.width = width;
        this.height = height;
        this.pathCount = pathCount;
    }

    // 실제 경로 생성 함수
    public List<List<Vector2Int>> GeneratePaths(bool vertical)
    {
        HashSet<int> usedStartXs = new();       // 시작 위치 중복 방지용
        List<List<Vector2Int>> allPaths = new(); // 최종 경로 리스트
        int attempts = 0;                       // 무한 루프 방지용 카운터

        // 경로가 충분히 생성될 때까지 반복
        while (allPaths.Count < pathCount && attempts < 100)
        {
            attempts++;

            // === 1. 시작 위치 선택 ===
            int startIndex;
            int range = vertical ? width : height;

            do
            {
                startIndex = rng.Next(0, range); // 0~6 중 랜덤 선택
            }
            // 최소 2개는 서로 다른 시작 위치를 보장
            while (usedStartXs.Count < 2 && usedStartXs.Contains(startIndex));

            usedStartXs.Add(startIndex);

            // === 2. 경로 만들기 ===
            List<Vector2Int> path = new();   // 이 경로의 좌표 리스트
            

            if(vertical)
            {
                int currentX = startIndex;

                for (int y = 0; y < height; y++)
                {
                    

                    path.Add(new Vector2Int(currentX, y)); // 현재 위치 추가

                    // 위로 올라갈 수 있는 x 좌표 선택: 좌, 중, 우
                    List<int> possibleNextXs = new();

                    if (currentX > 0) possibleNextXs.Add(currentX - 1); // 왼쪽 대각선
                    possibleNextXs.Add(currentX);                       // 위로 직진
                    if (currentX < width - 1) possibleNextXs.Add(currentX + 1); // 오른쪽 대각선

                    // 다음 위치 랜덤 선택
                    currentX = possibleNextXs[rng.Next(possibleNextXs.Count)];
                }
            }
            else
            {
                int currentY = startIndex;

                for (int x = 0; x < width; x++)
                {
                    path.Add(new Vector2Int(x, currentY));

                    // 다음 Y 결정: 위, 중간, 아래
                    List<int> nextYs = new();
                    if (currentY > 0) nextYs.Add(currentY - 1);
                    nextYs.Add(currentY);
                    if (currentY < height - 1) nextYs.Add(currentY + 1);

                    currentY = nextYs[rng.Next(nextYs.Count)];
                }
            }


            allPaths.Add(path); // 이 경로 저장
        }



        return allPaths; // 모든 경로 반환
    }
}