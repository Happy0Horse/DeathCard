using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomMap : MonoBehaviour
{
    private Dictionary<Vector2Int, RoomCell> cellMap = new();

    [ContextMenu("Map Room Cells")]
    public void MapCells()
    {
        cellMap.Clear();
        var cells = GetComponentsInChildren<RoomCell>().ToList();

        if (cells.Count == 0) return;

        float minX = cells.Min(c => c.transform.localPosition.x);
        float minZ = cells.Min(c => c.transform.localPosition.z);

        float cellSize = 4f;
        MazeGenerator gen = Object.FindFirstObjectByType<MazeGenerator>();
        if (gen != null) cellSize = gen.cellSize;

        foreach (var cell in cells)
        {
            int lx = Mathf.RoundToInt((cell.transform.localPosition.x - minX) / cellSize);
            int ly = Mathf.RoundToInt((cell.transform.localPosition.z - minZ) / cellSize);

            cell.localX = lx;
            cell.localY = ly;
            cellMap[new Vector2Int(lx, ly)] = cell;
        }
    }

    public RoomCell GetCell(int x, int y)
    {
        if (cellMap.Count == 0) MapCells();
        cellMap.TryGetValue(new Vector2Int(x, y), out RoomCell cell);
        return cell;
    }
}