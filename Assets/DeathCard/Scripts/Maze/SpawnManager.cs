using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float distanceBetweenPlayers = 0.25f;
    public float playerHeight = 1f;
    public int playerCount = 4;

    [Header("Materials")]
    public Material spawnMaterial;

    public void SetupSpawns(MazeCell[,] grid, int width, int height, List<RoomData> rooms, System.Random rng)
    {
        // Удаляем старые спавнпоинты
        foreach (var sp in FindObjectsOfType<NetworkStartPosition>())
            Destroy(sp.gameObject);

        List<Vector2Int> spawnPoints = new List<Vector2Int>();

        // Первая точка
        Vector2Int first = new Vector2Int(rng.Next(0, width), rng.Next(0, height));
        int safety = 0;
        while ((grid[first.x, first.y] == null || RoomGenerator.IsInsideRoom(first.x, first.y, rooms)) && safety < 1000)
        {
            first = new Vector2Int(rng.Next(0, width), rng.Next(0, height));
            safety++;
        }
        spawnPoints.Add(first);

        // Остальные точки
        float targetDist = Mathf.Sqrt(width * width + height * height) * distanceBetweenPlayers;
        for (int i = 1; i < playerCount; i++)
            spawnPoints.Add(GetPointAtApproxDistance(grid, width, height, spawnPoints, targetDist, rooms));

        // Создаём NetworkStartPosition на каждой точке
        foreach (Vector2Int sp in spawnPoints)
        {
            if (grid[sp.x, sp.y] == null) continue;

            grid[sp.x, sp.y].SetFloorMaterial(spawnMaterial);

            Vector3 pos = grid[sp.x, sp.y].transform.position + Vector3.up * playerHeight;
            GameObject spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.position = pos;
            spawnPoint.AddComponent<NetworkStartPosition>();

            NetworkManager.startPositions.Add(spawnPoint.transform);
        }
    }

    private Vector2Int GetPointAtApproxDistance(MazeCell[,] grid, int width, int height, List<Vector2Int> existing, float target, List<RoomData> rooms)
    {
        Vector2Int best = existing[0];
        float bestScore = float.MaxValue;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null || RoomGenerator.IsInsideRoom(x, y, rooms)) continue;

                Vector2Int current = new Vector2Int(x, y);
                if (existing.Contains(current)) continue;

                float avgDist = 0;
                foreach (Vector2Int sp in existing)
                    avgDist += Vector2Int.Distance(current, sp);
                avgDist /= existing.Count;

                float score = Mathf.Abs(avgDist - target);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = current;
                }
            }
        }
        return best;
    }
}