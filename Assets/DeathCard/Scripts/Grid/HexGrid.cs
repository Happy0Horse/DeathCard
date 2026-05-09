using UnityEngine;
using System.Collections.Generic;

public class HexGrid : MonoBehaviour
{
    public Dictionary<Vector2Int, HexCell> Cells = new Dictionary<Vector2Int, HexCell>();

    void Awake()
    {
        Cells.Clear();
        foreach (Transform child in transform)
        {
            HexCell cell = child.GetComponent<HexCell>();
            if (cell != null && !Cells.ContainsKey(cell.coordinates))
                Cells.Add(cell.coordinates, cell);
        }
        LinkNeighbors();
    }

    public void Clear() => Cells.Clear();

    public void AddCell(Vector2Int coord, GameObject obj)
    {
        HexCell cell = obj.GetComponent<HexCell>();
        if (cell != null && !Cells.ContainsKey(coord))
        {
            cell.Initialize(coord);
            Cells.Add(coord, cell);
        }
    }

    public void LinkNeighbors()
    {
        Vector2Int[] directions = {
            new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, -1),
            new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0, 1)
        };

        foreach (var cell in Cells.Values)
        {
            foreach (var dir in directions)
            {
                if (Cells.TryGetValue(cell.coordinates + dir, out HexCell neighbor))
                {
                    cell.neighbors.Add(neighbor);
                }
            }
        }
    }

    public HexCell GetCell(Vector2Int coord)
    {
        Cells.TryGetValue(coord, out HexCell cell);
        return cell;
    }
}