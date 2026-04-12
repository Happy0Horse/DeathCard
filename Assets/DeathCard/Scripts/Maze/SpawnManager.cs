using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Testing fields")]
    public int playerCount = 1;
    public float distanceBetweenPlayers = 0.25f;

    [Header("Spawn Settings")]
    public GameObject player;
    public float playerHeight = 1f;

    [Header("Materials")]
    public Material spawnMaterial;
    public Material defaultMaterial;

    private List<GameObject> _spawnedObjects = new List<GameObject>();

    public void SetupSpawns(MazeCell[,] grid, int width, int height)
    {
        ClearSpawns();

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
                grid[sp.x, sp.y].SetFloorMaterial(spawnMaterial);
        }

        Vector3 startPos = grid[spawnPoints[0].x, spawnPoints[0].y].transform.position + Vector3.up * playerHeight;
        GameObject newPlayer = Instantiate(player, startPos, Quaternion.identity);
        _spawnedObjects.Add(newPlayer);
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

    public void ClearSpawns()
    {
        for (int i = _spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (_spawnedObjects[i] == null) continue;

            if (Application.isPlaying)
                Destroy(_spawnedObjects[i]);
            else
                DestroyImmediate(_spawnedObjects[i]);
        }
        _spawnedObjects.Clear();
    }
}