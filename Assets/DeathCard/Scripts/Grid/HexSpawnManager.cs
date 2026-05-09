using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Mirror;

public class HexSpawnManager : MonoBehaviour
{
    public HexGrid grid;
    public Material spawnPointMaterial;
    public float playerHeightOffset = 2.5f;

    [Range(2, 4)] public int spawnCount = 2;

    private void Start()
    {
        StartCoroutine(SetupNextFrame());
    }

    private System.Collections.IEnumerator SetupNextFrame()
    {
        yield return null;
        SetupSpawnPoints();
    }

    public void SetupSpawnPoints()
    {
        // Удаляем старые спавнпоинты
        foreach (var sp in FindObjectsOfType<NetworkStartPosition>())
            Destroy(sp.gameObject);
        NetworkManager.startPositions.Clear();

        List<GameObject> points = CalculateSpawnPoints();
        if (points == null || points.Count == 0)
        {
            Debug.LogError("[HexSpawnManager] No spawn points found!");
            return;
        }

        HighlightPoints(points);

        foreach (var point in points)
        {
            Vector3 spawnPos = point.transform.position + Vector3.up * playerHeightOffset;
            GameObject spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.position = spawnPos;
            spawnPoint.AddComponent<NetworkStartPosition>();
            NetworkManager.startPositions.Add(spawnPoint.transform);
        }

        Debug.Log($"[HexSpawnManager] Создано {points.Count} спавнпоинтов");
    }

    private List<GameObject> CalculateSpawnPoints()
    {
        var generator = FindFirstObjectByType<HexGridGenerator>();
        if (generator == null) return new List<GameObject>();

        if (grid == null) grid = generator.GetComponent<HexGrid>();
        int currentRadius = generator.radius;

        List<HexCell> allCells = grid.Cells.Values.ToList();
        if (allCells.Count == 0) return new List<GameObject>();

        List<HexCell> edgeCells = allCells.Where(cell =>
            GetHexDistance(cell.coordinates, Vector2Int.zero) == currentRadius).ToList();
        if (edgeCells.Count == 0) return new List<GameObject>();

        List<HexCell> corners = new List<HexCell> { edgeCells[0] };
        for (int i = 1; i < spawnCount; i++)
        {
            HexCell bestCorner = edgeCells
                .Where(c => !corners.Contains(c))
                .OrderByDescending(c => corners.Min(corner => Vector3.Distance(c.transform.position, corner.transform.position)))
                .FirstOrDefault();

            if (bestCorner != null) corners.Add(bestCorner);
        }

        List<GameObject> shiftedPoints = new List<GameObject>();
        foreach (var corner in corners)
        {
            HexCell shiftedPoint = corner.neighbors.FirstOrDefault(n =>
                GetHexDistance(n.coordinates, Vector2Int.zero) == currentRadius &&
                n != corner);

            shiftedPoints.Add(shiftedPoint != null ? shiftedPoint.gameObject : corner.gameObject);
        }

        return shiftedPoints;
    }

    private void HighlightPoints(List<GameObject> points)
    {
        foreach (var point in points)
        {
            Renderer r = point.GetComponentInChildren<Renderer>();
            if (r != null && spawnPointMaterial != null)
                r.sharedMaterial = spawnPointMaterial;
        }
    }

    private int GetHexDistance(Vector2Int a, Vector2Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.x + a.y - (b.x + b.y)) + Mathf.Abs(a.y - b.y)) / 2;
    }

    public GameObject SpawnObjectOnNeighborCell(HexCell fromCell, GameObject prefab, float heightOffset = 1f)
    {
        if (fromCell == null || prefab == null) return null;

        HexCell neighbor = fromCell.neighbors.Find(n => n != null && n.canWalkOn);
        if (neighbor == null)
        {
            Debug.LogWarning("No available neighbor cell found");
            return null;
        }

        Vector3 spawnPosition = neighbor.transform.position + Vector3.up * heightOffset;
        return Instantiate(prefab, spawnPosition, Quaternion.identity);
    }
}