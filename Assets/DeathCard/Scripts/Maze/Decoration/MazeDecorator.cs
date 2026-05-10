using System.Collections.Generic;
using UnityEngine;

public class MazeDecorator : MonoBehaviour
{
    [Header("Master Controls")]
    public bool enableDecorations = true;
    public bool hideEmptyMounts = true;

    [Header("Ruleset")]
    public List<DecorationRule> rules = new List<DecorationRule>();

    public void Decorate(MazeCell[,] grid, int width, int height, List<RoomData> rooms, System.Random _rng)
    {
        if (grid == null || !enableDecorations) return;
        ResetRuleCounters(_rng);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                MazeCell cell = grid[x, y];
                if (cell == null) continue;

                MazeCellDecor decor = cell.GetComponent<MazeCellDecor>();
                if (decor == null) continue;

                bool isInRoom = RoomGenerator.IsInsideRoom(x, y, rooms);

                if (!isInRoom)
                {
                    ApplyRules(cell, decor, _rng);
                }

                if (hideEmptyMounts)
                {
                    decor.HideUnusedMounts();
                }
            }
        }
    }

    private void ResetRuleCounters(System.Random _rng)
    {
        foreach (var rule in rules)
        {
            rule.stepsSinceLast = 0;
            rule.currentTargetInterval = rule.interval + _rng.Next(-rule.intervalRandomness, rule.intervalRandomness + 1);
        }
    }

    private void ApplyRules(MazeCell cell, MazeCellDecor decor, System.Random _rng)
    {
        bool isDeadEnd = IsDeadEnd(cell);
        foreach (var rule in rules)
        {
            if (!rule.isEnabled || rule.prefab == null) continue;
            if (rule.deadEndsOnly && !isDeadEnd) continue;

            bool shouldSpawn = false;
            if (rule.interval > 0)
            {
                rule.stepsSinceLast++;
                if (rule.stepsSinceLast < rule.currentTargetInterval) continue;
            }

            if (rule.chance <= 0 || _rng.Next(0, 100) < rule.chance) shouldSpawn = true;

            if (shouldSpawn)
            {
                bool success = decor.PlaceProp(rule.prefab, rule.mountKey, _rng);
                if (success)
                {
                    rule.stepsSinceLast = 0;
                    rule.currentTargetInterval = rule.interval + _rng.Next(-rule.intervalRandomness, rule.intervalRandomness + 1);
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