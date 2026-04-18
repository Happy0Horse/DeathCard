using System.Collections.Generic;
using UnityEngine;

public class RoomInteriorDecorator : MonoBehaviour
{
    public List<RoomPrefabSettings> prefabs;

    [Header("Doors")]
    public List<WeightedPrefab> doorVariants;

    public void DecorateRooms(List<RoomData> rooms, MazeCell[,] grid)
    {
        MazeGenerator gen = FindFirstObjectByType<MazeGenerator>();
        if (gen == null) return;

        foreach (RoomData room in rooms)
        {
            GameObject prefab = FindPrefabForSize(room);
            if (prefab == null) continue;

            float targetX = (room.bounds.x + (room.bounds.width - 1) / 2f) * gen.cellSize;
            float targetZ = (room.bounds.y + (room.bounds.height - 1) / 2f) * gen.cellSize;
            Vector3 targetPos = gen.transform.position + new Vector3(targetX, 0, targetZ);

            GameObject roomInstance = Instantiate(prefab, targetPos, Quaternion.identity, transform);
            room.entrances = GetValidPerimeterPoints(room, gen);
            ApplyRotation(room, roomInstance);
            SpawnDoors(room, roomInstance, gen);
        }
    }

    private void SpawnDoors(RoomData room, GameObject roomInstance, MazeGenerator gen)
    {
        if (doorVariants == null || doorVariants.Count == 0) return;

        float rotationY = roomInstance.transform.eulerAngles.y;
        List<Vector2Int> spawnedDoorCoords = new();
        int maxDoors = room.isBig ? Random.Range(2, 4) : 1;
        int minGridDistance = 2;

        room.finalDoorPositions.Clear();

        RoomCell[] allCells = roomInstance.GetComponentsInChildren<RoomCell>();
        List<(RoomCell cell, RoomCell.DoorWall wall, Vector2Int localCoord)> candidates = new();

        foreach (var cell in allCells)
        {
            if (cell.doorWalls == null) continue;

            Vector3 localPos = roomInstance.transform.InverseTransformPoint(cell.transform.position);
            int calcX = Mathf.RoundToInt(localPos.x / gen.cellSize + (room.bounds.width - 1) / 2f);
            int calcY = Mathf.RoundToInt(localPos.z / gen.cellSize + (room.bounds.height - 1) / 2f);
            Vector2Int pos = new Vector2Int(calcX, calcY);

            foreach (var dw in cell.doorWalls)
            {
                candidates.Add((cell, dw, pos));
            }
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            var temp = candidates[i];
            int randomIndex = Random.Range(i, candidates.Count);
            candidates[i] = candidates[randomIndex];
            candidates[randomIndex] = temp;
        }

        int doorsSpawned = 0;
        foreach (var candidate in candidates)
        {
            if (doorsSpawned >= maxDoors) break;
            if (spawnedDoorCoords.Contains(candidate.localCoord)) continue;

            Side worldSide = GetWorldSide(candidate.wall.side, rotationY);
            Vector2Int offset = GetOffsetFromSide(worldSide);

            int gx = room.bounds.x + candidate.localCoord.x;
            int gy = room.bounds.y + candidate.localCoord.y;
            int tx = gx + offset.x;
            int ty = gy + offset.y;

            if (tx >= 0 && tx < gen.width && ty >= 0 && ty < gen.height && gen.GetCell(tx, ty) != null)
            {
                if (IsTooClose(candidate.localCoord, spawnedDoorCoords, minGridDistance)) continue;

                candidate.wall.wallObject.SetActive(false);

                GameObject chosenDoor = GetWeightedPrefab(doorVariants);
                if (chosenDoor != null)
                {
                    Instantiate(chosenDoor, candidate.wall.wallObject.transform.position, candidate.wall.wallObject.transform.rotation, roomInstance.transform);
                }

                room.finalDoorPositions.Add(new Vector2Int(gx, gy));
                spawnedDoorCoords.Add(candidate.localCoord);
                doorsSpawned++;
            }
        }
    }

    private GameObject GetWeightedPrefab(List<WeightedPrefab> list)
    {
        int totalWeight = 0;
        foreach (var item in list) totalWeight += item.weight;
        int roll = Random.Range(0, totalWeight);
        int current = 0;
        foreach (var item in list)
        {
            current += item.weight;
            if (roll < current) return item.prefab;
        }
        return null;
    }

    private bool IsTooClose(Vector2Int current, List<Vector2Int> existing, int minDist)
    {
        foreach (var coord in existing)
        {
            if (Vector2Int.Distance(current, coord) < minDist) return true;
        }
        return false;
    }

    private Side GetWorldSide(Side localSide, float rotationY)
    {
        int rotSteps = Mathf.RoundToInt(rotationY / 90f) % 4;
        if (rotSteps < 0) rotSteps += 4;
        int worldSideIndex = ((int)localSide + rotSteps) % 4;
        return (Side)worldSideIndex;
    }

    private Vector2Int GetOffsetFromSide(Side side)
    {
        return side switch
        {
            Side.Top => new Vector2Int(0, 1),
            Side.Right => new Vector2Int(1, 0),
            Side.Bottom => new Vector2Int(0, -1),
            Side.Left => new Vector2Int(-1, 0),
            _ => Vector2Int.zero
        };
    }

    private void ApplyRotation(RoomData room, GameObject instance)
    {
        if (room.entrances == null || room.entrances.Count == 0) return;

        Vector2Int ent = room.entrances[0];
        float angle = 0;

        if (ent.y == room.bounds.y) angle = 90f;
        else if (ent.y == room.bounds.y + room.bounds.height - 1) angle = 0f;
        else if (ent.x == room.bounds.x) angle = 270f;
        else if (ent.x == room.bounds.x + room.bounds.width - 1) angle = 90f;

        instance.transform.localRotation = Quaternion.Euler(0, angle, 0);
    }

    private GameObject FindPrefabForSize(RoomData room)
    {
        foreach (var p in prefabs)
        {
            if (p.size.x == room.bounds.width && p.size.y == room.bounds.height)
                return p.prefab;
        }
        return null;
    }

    private List<Vector2Int> GetValidPerimeterPoints(RoomData room, MazeGenerator gen)
    {
        List<Vector2Int> validPoints = new List<Vector2Int>();
        for (int x = 0; x < room.bounds.width; x++)
        {
            for (int y = 0; y < room.bounds.height; y++)
            {
                if (!(x == 0 || x == room.bounds.width - 1 || y == 0 || y == room.bounds.height - 1)) continue;

                int gx = room.bounds.x + x;
                int gy = room.bounds.y + y;

                if (HasMazeNeighbor(gx, gy, gen)) validPoints.Add(new Vector2Int(gx, gy));
            }
        }
        return validPoints;
    }

    private bool HasMazeNeighbor(int x, int y, MazeGenerator gen)
    {
        return gen.GetCell(x + 1, y) != null || gen.GetCell(x - 1, y) != null ||
               gen.GetCell(x, y + 1) != null || gen.GetCell(x, y - 1) != null;
    }
}