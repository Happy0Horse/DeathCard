using System.Collections.Generic;
using UnityEngine;

public class MazeDecorator : MonoBehaviour
{
    [Header("Master Controls")]
    public bool enableDecorations = true;
    public bool hideEmptyMounts = true;

    [Header("Ruleset")]
    public List<DecorationRule> rules = new List<DecorationRule>();

    public void Decorate(MazeCell[,] grid, int width, int height)
    {
        if (grid == null || !enableDecorations) return;

        // Reset counters at the start of every new generation
        ResetRuleCounters();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                MazeCell cell = grid[x, y];
                if (cell == null) continue;

                MazeCellDecor decor = cell.GetComponent<MazeCellDecor>();
                if (decor == null) continue;

                ApplyRules(cell, decor);

                if (hideEmptyMounts)
                {
                    decor.HideUnusedMounts();
                }
            }
        }
    }

    private void ResetRuleCounters()
    {
        foreach (var rule in rules)
        {
            rule.stepsSinceLast = 0;
            rule.currentTargetInterval = rule.interval + Random.Range(-rule.intervalRandomness, rule.intervalRandomness + 1);
        }
    }

    private void ApplyRules(MazeCell cell, MazeCellDecor decor)
    {
        bool isDeadEnd = IsDeadEnd(cell);

        foreach (var rule in rules)
        {
            if (!rule.isEnabled || rule.prefab == null) continue;
            if (rule.deadEndsOnly && !isDeadEnd) continue;
            if (rule.roomsOnly && !cell.visited) continue;

            bool shouldSpawn = false;

            if (rule.interval <= 0)
            {
                if (Random.Range(0, 100) < rule.chance) shouldSpawn = true;
            }
            else
            {
                rule.stepsSinceLast++;

                if (rule.stepsSinceLast >= rule.currentTargetInterval)
                {
                    if (Random.Range(0, 100) < rule.chance)
                    {
                        shouldSpawn = true;
                    }
                }
            }

            if (shouldSpawn)
            {
                bool success = decor.PlaceProp(rule.prefab, rule.mountKey);

                if (success)
                {
                    rule.stepsSinceLast = 0;
                    rule.currentTargetInterval = rule.interval + Random.Range(-rule.intervalRandomness, rule.intervalRandomness + 1);
                }
            }
        }
    }

    private bool IsDeadEnd(MazeCell cell)
    {
        int wallCount = 0;
        if (cell.wallTop && cell.wallTop.activeSelf) wallCount++;
        if (cell.wallRight && cell.wallRight.activeSelf) wallCount++;
        if (cell.wallBottom && cell.wallBottom.activeSelf) wallCount++;
        if (cell.wallLeft && cell.wallLeft.activeSelf) wallCount++;
        return wallCount == 3;
    }
}
