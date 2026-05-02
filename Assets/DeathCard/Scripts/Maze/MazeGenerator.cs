using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MazeGenerator : MonoBehaviour
{
    [Header("Editor & Workflow")]
    public bool autoRegenerateInEditor = true;
    public bool regenerateOnStart = true;
    public MazeDecorator decorator;
    public SpawnManager spawner;
    public RoomInteriorDecorator roomDecorator;
    public List<RoomData> activeRooms = new();
    public string seed = "DefaultSeed";
    public bool useRandomSeed = false;

    [Header("Settings")]
    public int width = 50;
    public int height = 50;
    public float cellSize = 4f;
    public MazeCell cellPrefab;

    [Header("Erosion")]
    [Range(0f, 1f)] public float erosionAmount = 0f;
    [Range(0.1f, 10f)] public float erosionRandomness = 1f;

    [Header("Features")]
    public bool useRooms = true;
    public bool useHollows = true;

    [Header("Feature Amounts")]
    [Range(0f, 5f)] public float roomAmountMultiplier = 1f;
    [Range(0f, 5f)] public float hollowAmountMultiplier = 1f;

    [Header("Feature Sizes")]
    [Range(2, 10)] public int minSize = 2;
    [Range(4, 15)] public int maxSize = 4;
    public WeightedRoomSize[] allowedSizes = { new WeightedRoomSize { size = new Vector2Int(3, 3), weight = 10 } };

    private MazeCell[,] grid;
    private System.Random _rng;

    private void Start()
    {
        if (decorator == null) decorator = GetComponent<MazeDecorator>();
        if (regenerateOnStart) Generate();
    }

#if UNITY_EDITOR
    private double _nextAllowedTime = 0;

    private void OnValidate()
    {
        if (Application.isPlaying || !autoRegenerateInEditor) return;
        if (EditorApplication.timeSinceStartup < _nextAllowedTime) return;
        _nextAllowedTime = EditorApplication.timeSinceStartup + 0.2f;
        EditorApplication.delayCall -= OnEditorUpdate;
        EditorApplication.delayCall += OnEditorUpdate;
    }

    private void OnDestroy()
    {
        EditorApplication.delayCall -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (this == null) return;
        Generate();
    }
#endif

    public void Generate()
    {
        ClearOldMaze();
        if (useRandomSeed)
        {
            seed = System.Guid.NewGuid().ToString().Substring(0, 8);
        }

        int seedHash = seed.GetHashCode();
        _rng = new System.Random(seedHash);

        Debug.Log(seed);
        Debug.Log(seed.GetHashCode());
        Debug.Log(seedHash);

        activeRooms = RoomGenerator.Generate(width, height, roomAmountMultiplier,
                    allowedSizes, minSize, maxSize, _rng);

        GenerateGrid();

        if (erosionAmount > 0) ErosionGenerator.Apply(grid, width, height, erosionAmount, erosionRandomness, RemoveAndEncloseCell, _rng);

        if (useHollows) HollowGenerator.Generate(grid, width, height, hollowAmountMultiplier, allowedSizes, minSize, maxSize, RemoveAndEncloseCell, _rng);

        GenerateMaze();

        if (spawner == null) spawner = GetComponent<SpawnManager>();
        if (spawner != null) spawner.SetupSpawns(grid, width, height, activeRooms, _rng);

        CleanUpLonelyPillars();
        ApplyWallVariations(_rng);

        if (decorator == null) decorator = GetComponent<MazeDecorator>();
        if (decorator != null) decorator.Decorate(grid, width, height, activeRooms, _rng);

        if (roomDecorator != null)
            roomDecorator.DecorateRooms(activeRooms, grid, _rng);
    }

    void ClearOldMaze()
    {
        MazeCell[] existingCells = GetComponentsInChildren<MazeCell>();
        for (int i = existingCells.Length - 1; i >= 0; i--)
        {
            if (existingCells[i] == null) continue;
            if (Application.isPlaying) Destroy(existingCells[i].gameObject);
            else DestroyImmediate(existingCells[i].gameObject, false);
        }
        grid = null;
    }

    void GenerateGrid()
    {
        grid = new MazeCell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (IsCellInRoom(x, y))
                {
                    grid[x, y] = null;
                    continue;
                }

                GameObject cellGo = Instantiate(cellPrefab.gameObject, transform.position + new Vector3(x * cellSize, 0, y * cellSize), Quaternion.identity, transform);
                cellGo.gameObject.name = $"[{x}][{y}]";
                grid[x, y] = cellGo.GetComponent<MazeCell>();
                grid[x, y].gridX = x;
                grid[x, y].gridY = y;
                grid[x, y].visited = false;
                InitializeCellWalls(x, y);
            }
        }
    }

    private bool IsCellInRoom(int x, int y)
    {
        if (activeRooms == null) return false;
        foreach (var room in activeRooms)
        {
            if (x >= room.bounds.x && x < room.bounds.x + room.bounds.width &&
                y >= room.bounds.y && y < room.bounds.y + room.bounds.height)
            {
                return true;
            }
        }
        return false;
    }

    private void ApplyWallVariations(System.Random _rng)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                MazeCell cell = grid[x, y];
                if (cell == null) continue;

                if (y < height - 1 && grid[x, y + 1] != null)
                {
                    if (cell.wallTop && cell.wallTop.activeSelf)
                        cell.TryReplaceWithVariant(cell.wallTop, _rng);
                }

                if (x < width - 1 && grid[x + 1, y] != null)
                {
                    if (cell.wallRight && cell.wallRight.activeSelf)
                        cell.TryReplaceWithVariant(cell.wallRight, _rng);
                }

                if (y > 0 && grid[x, y - 1] != null)
                {
                    if (cell.wallBottom && cell.wallBottom.activeSelf)
                        cell.TryReplaceWithVariant(cell.wallBottom, _rng);
                }

                if (x > 0 && grid[x - 1, y] != null)
                {
                    if (cell.wallLeft && cell.wallLeft.activeSelf)
                        cell.TryReplaceWithVariant(cell.wallLeft, _rng);
                }
            }
        }
    }

    void InitializeCellWalls(int x, int y)
    {
        grid[x, y].wallTop?.SetActive(true);
        grid[x, y].wallRight?.SetActive(true);
        grid[x, y].wallLeft?.SetActive(false);
        grid[x, y].wallBottom?.SetActive(false);
        grid[x, y].pillarTR?.SetActive(true);
        grid[x, y].pillarTL?.SetActive(false);
        grid[x, y].pillarBL?.SetActive(false);
        grid[x, y].pillarBR?.SetActive(false);

        if (IsCellInRoom(x, y + 1)) grid[x, y].wallTop?.SetActive(false);
        if (IsCellInRoom(x + 1, y)) grid[x, y].wallRight?.SetActive(false);

        if (x == 0)
        {
            grid[x, y].wallLeft?.SetActive(true);
            grid[x, y].pillarTL?.SetActive(true);
            grid[x, y].pillarBL?.SetActive(true);
        }
        if (y == 0)
        {
            grid[x, y].wallBottom?.SetActive(true);
            grid[x, y].pillarBR?.SetActive(true);
        }

        if (x == 0 && IsCellInRoom(x - 1, y)) grid[x, y].wallLeft?.SetActive(false);
        if (y == 0 && IsCellInRoom(x, y - 1)) grid[x, y].wallBottom?.SetActive(false);
    }

    public void RemoveAndEncloseCell(int x, int y)
    {
        if (grid[x, y] == null) return;
        if (x > 0 && grid[x - 1, y] != null) grid[x - 1, y].wallRight?.SetActive(true);
        if (y > 0 && grid[x, y - 1] != null) grid[x, y - 1].wallTop?.SetActive(true);
        if (x + 1 < width && grid[x + 1, y] != null) grid[x + 1, y].wallLeft?.SetActive(true);
        if (y + 1 < height && grid[x, y + 1] != null) grid[x, y + 1].wallBottom?.SetActive(true);

        if (Application.isPlaying) Destroy(grid[x, y].gameObject);
        else DestroyImmediate(grid[x, y].gameObject);
        grid[x, y] = null;
    }

    void GenerateMaze()
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int start = Vector2Int.zero;
        bool foundStart = false;
        for (int x = 0; x < width && !foundStart; x++)
            for (int y = 0; y < height && !foundStart; y++)
                if (grid[x, y] != null && !grid[x, y].visited) { start = new Vector2Int(x, y); foundStart = true; }

        if (!foundStart) return;
        grid[start.x, start.y].visited = true;
        stack.Push(start);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(current);
            if (neighbors.Count > 0)
            {
                stack.Push(current);
                Vector2Int next = neighbors[_rng.Next(0, neighbors.Count)];
                RemoveWalls(current, next);
                grid[next.x, next.y].visited = true;
                stack.Push(next);
            }
        }
    }

    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] dirs = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };
        foreach (var dir in dirs)
        {
            Vector2Int n = cell + dir;
            if (n.x >= 0 && n.x < width && n.y >= 0 && n.y < height && grid[n.x, n.y] != null && !grid[n.x, n.y].visited)
                neighbors.Add(n);
        }
        return neighbors;
    }

    void RemoveWalls(Vector2Int current, Vector2Int next)
    {
        if (grid[current.x, current.y] == null || grid[next.x, next.y] == null) return;
        int dx = current.x - next.x;
        int dy = current.y - next.y;
        if (dx == 1) grid[next.x, next.y].RemoveWallRight();
        else if (dx == -1) grid[current.x, current.y].RemoveWallRight();
        else if (dy == 1) grid[next.x, next.y].RemoveWallTop();
        else if (dy == -1) grid[current.x, current.y].RemoveWallTop();
    }

    void CleanUpLonelyPillars()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                MazeCell cell = grid[x, y];
                if (cell == null) continue;

                bool trCornerClear = IsCellInRoom(x, y) || IsCellInRoom(x + 1, y) ||
                                    IsCellInRoom(x, y + 1) || IsCellInRoom(x + 1, y + 1);

                bool tlCornerClear = IsCellInRoom(x, y) || IsCellInRoom(x - 1, y) ||
                                    IsCellInRoom(x, y + 1) || IsCellInRoom(x - 1, y + 1);

                bool blCornerClear = IsCellInRoom(x, y) || IsCellInRoom(x - 1, y) ||
                                    IsCellInRoom(x, y - 1) || IsCellInRoom(x - 1, y - 1);

                bool brCornerClear = IsCellInRoom(x, y) || IsCellInRoom(x + 1, y) ||
                                    IsCellInRoom(x, y - 1) || IsCellInRoom(x + 1, y - 1);

                bool wT = cell.wallTop.activeSelf;
                bool wR = cell.wallRight.activeSelf;
                bool aboveR = (y + 1 < height && grid[x, y + 1] != null) && grid[x, y + 1].wallRight.activeSelf;
                bool rightT = (x + 1 < width && grid[x + 1, y] != null) && grid[x + 1, y].wallTop.activeSelf;
                cell.pillarTR?.SetActive(!trCornerClear && (wT || wR || aboveR || rightT));

                bool wL = cell.wallLeft != null && cell.wallLeft.activeSelf;
                bool leftT = (x > 0 && grid[x - 1, y] != null) ? grid[x - 1, y].wallTop.activeSelf : false;
                cell.pillarTL?.SetActive(!tlCornerClear && (wL || wT || leftT));

                bool wB = cell.wallBottom != null && cell.wallBottom.activeSelf;
                bool leftB = (x > 0 && grid[x - 1, y] != null) ? grid[x - 1, y].wallBottom.activeSelf : false;
                bool belowL = (y > 0 && grid[x, y - 1] != null) ? grid[x, y - 1].wallLeft.activeSelf : false;
                cell.pillarBL?.SetActive(!blCornerClear && (wL || wB || leftB || belowL));

                bool rightB = (x + 1 < width && grid[x + 1, y] != null) ? grid[x + 1, y].wallBottom.activeSelf : false;
                cell.pillarBR?.SetActive(!brCornerClear && (wR || wB || rightB));
            }
        }
    }

    public MazeCell GetCell(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return null;
        return grid[x, y];
    }
}