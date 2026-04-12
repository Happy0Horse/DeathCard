using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Testing fields")]
    public Color spawnColor = Color.cyan;
    public int playerCount = 1;
    public float distanceBetweenPlayers = 0.25f;

    [Header("Spawn Settings")]
    public GameObject player;
    public float playerHeight = 1f;

    public static SpawnManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetupSpawns(MazeCell[,] grid, int width, int height)
    {
        List<Vector2Int> spawnPoints = new List<Vector2Int>();

        Vector2Int first = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        int safetyNet = 0;
        while (grid[first.x, first.y] == null && safetyNet < 1000)
        {
            first = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
            safetyNet++;
        }

        spawnPoints.Add(first);

        float maxDiagonal = Mathf.Sqrt(width * width + height * height);
        float targetDist = maxDiagonal * distanceBetweenPlayers;

        for (int i = 1; i < playerCount; i++)
        {
            spawnPoints.Add(GetPointAtApproxDistance(grid, width, height, spawnPoints, targetDist));
        }

        foreach (Vector2Int sp in spawnPoints)
        {
            if (grid[sp.x, sp.y] != null)
                grid[sp.x, sp.y].SetFloorColor(spawnColor);
        }

        Vector3 startPos = grid[spawnPoints[0].x, spawnPoints[0].y].transform.position + Vector3.up * playerHeight;
        Object.Instantiate(player, startPos, Quaternion.identity);
    }

    private Vector2Int GetPointAtApproxDistance(MazeCell[,] grid, int width, int height, List<Vector2Int> existing, float target)
    {
        Vector2Int bestCandidate = existing[0];
        float bestScore = float.MaxValue;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null) continue;

                Vector2Int current = new Vector2Int(x, y);
                if (existing.Contains(current)) continue;

                float avgDist = 0;
                foreach (Vector2Int sp in existing)
                {
                    avgDist += Vector2Int.Distance(current, sp);
                }
                avgDist /= existing.Count;

                float score = Mathf.Abs(avgDist - target);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestCandidate = current;
                }
            }
        }
        return bestCandidate;
    }
}