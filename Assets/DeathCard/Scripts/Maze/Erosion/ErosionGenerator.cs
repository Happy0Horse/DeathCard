using UnityEngine;

public static class ErosionGenerator
{
    public static void Apply(MazeCell[,] grid, int width, int height, float amount, float randomness, System.Action<int, int> removeCell, System.Random _rng)
    {
        Vector2 center = new Vector2(width / 2f, height / 2f);
        float maxDist = Vector2.Distance(Vector2.zero, center);
        float seed = (float)(_rng.NextDouble() * 100.0);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null) continue;
                float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                float noise = Mathf.PerlinNoise(x * 0.1f * randomness + seed, y * 0.1f * randomness + seed);
                if (dist + (noise * 0.2f * amount) > (1.2f - (amount * 0.7f))) removeCell(x, y);
            }
    }
}