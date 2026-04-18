using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RoomGenerator
{
    public static List<RoomData> Generate(
        int width,
        int height,
        float multiplier,
        WeightedRoomSize[] allowedSizes,
        int min,
        int max)
    {
        List<RoomData> rooms = new();
        int attempts = Mathf.RoundToInt(Mathf.Max(3, (width * height) / 100) * multiplier);

        List<WeightedRoomSize> validPool = new();
        int totalWeight = 0;

        foreach (var s in allowedSizes)
        {
            if (s.size.x >= min && s.size.x <= max && s.size.y >= min && s.size.y <= max)
            {
                validPool.Add(s);
                totalWeight += s.weight;
            }
        }

        if (validPool.Count == 0) return rooms;

        for (int i = 0; i < attempts; i++)
        {
            Vector2Int size = GetWeightedSize(validPool, totalWeight);

            int sx = Random.Range(1, width - size.x - 1);
            int sy = Random.Range(1, height - size.y - 1);

            if (IsAreaAvailable(rooms, sx, sy, size.x, size.y))
            {
                RoomData room = new RoomData(sx, sy, size.x, size.y, (size.x * size.y) > 25);
                rooms.Add(room);
                CreateEntrance(width, height, sx, sy, size.x, size.y, room);
            }
        }
        return rooms;
    }

    private static Vector2Int GetWeightedSize(List<WeightedRoomSize> pool, int totalWeight)
    {
        int roll = Random.Range(0, totalWeight);
        int current = 0;
        foreach (var item in pool)
        {
            current += item.weight;
            if (roll < current) return item.size;
        }
        return pool[0].size;
    }

    private static bool IsAreaAvailable(List<RoomData> rooms, int sx, int sy, int w, int h)
    {
        foreach (var room in rooms)
        {
            if (sx < room.bounds.x + room.bounds.width + 1 && sx + w + 1 > room.bounds.x &&
                sy < room.bounds.y + room.bounds.height + 1 && sy + h + 1 > room.bounds.y)
                return false;
        }
        return true;
    }

    static void CreateEntrance(int width, int height, int sx, int sy, int w, int h, RoomData room)
    {
        for (int x = sx; x < sx + w; x++)
        {
            if (sy > 0) { room.AddEntrance(x, sy); return; }
            if (sy + h < height) { room.AddEntrance(x, sy + h - 1); return; }
        }
    }

    public static bool IsInsideRoom(int x, int y, List<RoomData> rooms)
    {
        if (rooms == null) return false;
        for (int i = 0; i < rooms.Count; i++)
        {
            if (x >= rooms[i].bounds.x && x < rooms[i].bounds.x + rooms[i].bounds.width &&
                y >= rooms[i].bounds.y && y < rooms[i].bounds.y + rooms[i].bounds.height)
            {
                return true;
            }
        }
        return false;
    }
}