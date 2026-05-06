using System.Collections.Generic;
using UnityEngine;

public static class HollowGenerator
{
    public static void Generate(MazeCell[,] grid, int width, int height, float multiplier, WeightedRoomSize[] allowedSizes, int min, int max, System.Action<int, int> removeCell, System.Random _rng)
    {
        int attempts = Mathf.RoundToInt(Mathf.Max(1, (width * height) / 150) * multiplier);

        List<WeightedRoomSize> validPool = new();
        int totalWeight = 0;

        if (allowedSizes != null)
        {
            foreach (var s in allowedSizes)
            {
                if (s.size.x >= min && s.size.x <= max && s.size.y >= min && s.size.y <= max)
                {
                    validPool.Add(s);
                    totalWeight += s.weight;
                }
            }
        }

        for (int i = 0; i < attempts; i++)
        {
            Vector2Int size;
            if (validPool.Count > 0)
            {
                size = GetWeightedSize(validPool, totalWeight, _rng);
            }
            else
            {
                size = new Vector2Int(_rng.Next(min, max + 1), _rng.Next(min, max + 1));
            }

            int sx = _rng.Next(2, width - size.x - 2);
            int sy = _rng.Next(2, height - size.y - 2);

            if (IsAreaAvailable(grid, width, height, sx, sy, size.x, size.y))
            {
                for (int x = sx; x < sx + size.x; x++)
                {
                    for (int y = sy; y < sy + size.y; y++)
                    {
                        if (grid[x, y] != null)
                        {
                            if (x < width - 1 && y < height - 1) grid[x, y].RemovePillarTR();
                            removeCell(x, y);
                        }
                    }
                }
            }
        }
    }

    private static Vector2Int GetWeightedSize(List<WeightedRoomSize> pool, int totalWeight, System.Random _rng)
    {
        int roll = _rng.Next(0, totalWeight);
        int current = 0;
        foreach (var item in pool)
        {
            current += item.weight;
            if (roll < current) return item.size;
        }
        return pool[0].size;
    }

    private static bool IsAreaAvailable(MazeCell[,] grid, int width, int height, int sx, int sy, int w, int h)
    {
        for (int x = sx - 1; x < sx + w + 1; x++)
        {
            for (int y = sy - 1; y < sy + h + 1; y++)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) return false;
                if (grid[x, y] == null || grid[x, y].visited) return false;
            }
        }
        return true;
    }
}