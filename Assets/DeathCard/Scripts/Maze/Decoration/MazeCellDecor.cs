using System.Collections.Generic;
using UnityEngine;

public class MazeCellDecor : MonoBehaviour
{
    [Header("Mount groups")]
    public List<DecorationGroup> mountGroups = new List<DecorationGroup>();

    private HashSet<Transform> _usedTransforms = new HashSet<Transform>();
    private MazeCell _cell;

    public bool PlaceProp(GameObject prefab, string key, System.Random _rng)
    {
        if (prefab == null) return false;
        if (_cell == null) _cell = GetComponent<MazeCell>();

        DecorationGroup group = mountGroups.Find(g => g.key.Equals(key, System.StringComparison.OrdinalIgnoreCase));
        if (group == null) return false;

        List<Transform> validTransforms = new List<Transform>();

        foreach (DecorationMount m in group.mounts)
        {
            if (m.transform == null || _usedTransforms.Contains(m.transform)) continue;

            if (IsMountVisible(m))
            {
                validTransforms.Add(m.transform);
            }
        }

        if (validTransforms.Count == 0) return false;

        Transform target = validTransforms[_rng.Next(0, validTransforms.Count)];
        _usedTransforms.Add(target);

        Instantiate(prefab, target.position, target.rotation, target);
        return true;
    }

    private bool IsMountVisible(DecorationMount m)
    {
        if (m == null || m.transform == null) return false;
        if (m.wallDirection >= 4) return true;

        if (_cell == null) _cell = GetComponent<MazeCell>();

        int x = _cell.gridX;
        int y = _cell.gridY;

        var gen = FindFirstObjectByType<MazeGenerator>();
        if (gen == null) return false;

        int nx = x;
        int ny = y;
        switch (m.wallDirection)
        {
            case 0: ny++; break;
            case 1: nx++; break;
            case 2: ny--; break;
            case 3: nx--; break;
        }

        if (IsDoorAt(nx, ny, gen)) return false;

        switch (m.wallDirection)
        {
            case 0:
                return (_cell.wallTop != null && _cell.wallTop.activeSelf);
            case 1:
                return (_cell.wallRight != null && _cell.wallRight.activeSelf);
            case 2:
                return (_cell.wallBottom != null && _cell.wallBottom.activeSelf);
            case 3:
                return (_cell.wallLeft != null && _cell.wallLeft.activeSelf);
            default:
                return false;
        }
    }

    private bool IsDoorAt(int x, int y, MazeGenerator gen)
    {
        if (gen.activeRooms == null) return false;
        foreach (var room in gen.activeRooms)
        {
            if (room.finalDoorPositions.Contains(new Vector2Int(x, y))) return true;
        }
        return false;
    }

    public void HideUnusedMounts()
    {
        if (_cell == null) _cell = GetComponent<MazeCell>();

        foreach (var group in mountGroups)
        {
            foreach (var m in group.mounts)
            {
                if (m == null || m.transform == null) continue;

                if (!IsMountVisible(m) || !_usedTransforms.Contains(m.transform))
                {
                    m.transform.gameObject.SetActive(false);
                }
            }
        }
    }
}