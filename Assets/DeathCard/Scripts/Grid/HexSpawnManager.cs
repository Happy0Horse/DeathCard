using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HexSpawnManager : MonoBehaviour
{
    public HexGrid grid;
    public GameObject playerPrefab;
    public Material spawnPointMaterial;
    public float playerHeightOffset = 2.5f;

    [Range(2, 4)] public int spawnCount = 2;

    private void Start()
    {
        SpawnPlayerAtStart();
    }

    public void SetupSpawns()
    {
        List<GameObject> points = CalculateSpawnPoints();
        HighlightPoints(points);
    }

    private List<GameObject> CalculateSpawnPoints()
    {
        var generator = Object.FindFirstObjectByType<HexGridGenerator>();
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
            {
                r.sharedMaterial = spawnPointMaterial;
            }
        }
    }

    private int GetHexDistance(Vector2Int a, Vector2Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.x + a.y - (b.x + b.y)) + Mathf.Abs(a.y - b.y)) / 2;
    }

    private void SpawnPlayerAtStart()
    {
        var generator = Object.FindFirstObjectByType<HexGridGenerator>();
        if (generator == null) return;

        if (grid == null) grid = generator.GetComponent<HexGrid>();

        if (grid.Cells.Count == 0)
        {
            PopulateGridFromChildren(generator.transform);
        }

        List<GameObject> points = CalculateSpawnPoints();

        if (points != null && points.Count > 0 && playerPrefab != null)
        {
            Vector3 spawnPos = points[0].transform.position + Vector3.up * playerHeightOffset;
            GameObject p = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

            var viewManager = p.GetComponent<HexViewManager>();
            var navigator = p.GetComponent<HexGridNavigator>();

            if (viewManager != null) viewManager.SetGridCenter(generator.transform);

            if (navigator != null && viewManager != null)
            {
                HexCell startCell = points[0].GetComponent<HexCell>();
                navigator.Initialize(grid, startCell.coordinates, viewManager);
            }
        }
        else
        {
            Debug.LogError("[HexSpawnManager] No spawn points found! Check if HexCells are attached to prefabs.");
        }
    }

    private void PopulateGridFromChildren(Transform root)
    {
        grid.Clear();
        foreach (Transform child in root)
        {
            HexCell cell = child.GetComponent<HexCell>();
            if (cell != null)
            {
                grid.AddCell(cell.coordinates, child.gameObject);
            }
        }
        grid.LinkNeighbors();
    }

    public GameObject SpawnObjectOnNeighborCell(
    HexCell fromCell,
    GameObject prefab,
    float heightOffset = 1f
)
    {
        if (fromCell == null || prefab == null)
            return null;

        HexCell neighbor = fromCell.neighbors
            .Find(n => n != null && n.canWalkOn);

        if (neighbor == null)
        {
            Debug.LogWarning("No available neighbor cell found");
            return null;
        }

        Vector3 spawnPosition = neighbor.transform.position;
        spawnPosition.y = neighbor.transform.position.y + heightOffset;

        GameObject spawnedObject = Instantiate(
            prefab,
            spawnPosition,
            Quaternion.identity
        );

        return spawnedObject;
    }
}