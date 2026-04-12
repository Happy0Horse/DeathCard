using System.Collections.Generic;
using UnityEngine;

public class MazeCellDecor : MonoBehaviour
{
    public List<DecorationGroup> mountGroups = new List<DecorationGroup>();
    private HashSet<Transform> _usedTransforms = new HashSet<Transform>();
    private MazeCell _cell;

    public bool PlaceProp(GameObject prefab, string key)
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

        Transform target = validTransforms[Random.Range(0, validTransforms.Count)];
        _usedTransforms.Add(target);

        Instantiate(prefab, target.position, target.rotation, transform);

        return true;
    }

    private bool IsMountVisible(DecorationMount m)
    {
        if (m == null || m.transform == null) return false;

        if (m.wallDirection >= 4) return true;

        if (_cell == null) _cell = GetComponent<MazeCell>();

        if (_cell == null) return false;

        return _cell.IsWallActive(m.wallDirection);
    }

    public void HideUnusedMounts()
    {
        if (_cell == null) _cell = GetComponent<MazeCell>();

        foreach (var group in mountGroups)
        {
            if (group == null || group.mounts == null) continue;

            foreach (var m in group.mounts)
            {
                if (m == null || m.transform == null) continue;

                bool isVisible = IsMountVisible(m);
                bool isUsed = _usedTransforms.Contains(m.transform);

                if (!isVisible || !isUsed)
                {
                    m.transform.gameObject.SetActive(false);
                }
            }
        }
    }
}