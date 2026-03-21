using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    #region Variables
    [Header("Settings")]
    public int width = 50;
    public int height = 50;
    public float cellSize = 4f;
    public MazeCell cellPrefab;

    [Header("Erosion (Optional)")]
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

    private MazeCell[,] grid;
    #endregion

    #region Runtime Auto-Regeneration

    private int _lW, _lH, _lMin, _lMax;
    private float _lRA, _lHA, _lEA, _lER;
    private bool _lUR, _lUH;

    void Update()
    {
        if (CheckChanges())
        {
            CaptureState();
            Generate();
        }
    }

    bool CheckChanges()
    {
        return width != _lW || height != _lH || useRooms != _lUR || useHollows != _lUH ||
               minSize != _lMin || maxSize != _lMax ||
               !Mathf.Approximately(roomAmountMultiplier, _lRA) ||
               !Mathf.Approximately(hollowAmountMultiplier, _lHA) ||
               !Mathf.Approximately(erosionAmount, _lEA) ||
               !Mathf.Approximately(erosionRandomness, _lER);
    }

    void CaptureState()
    {
        _lW = width; _lH = height; _lUR = useRooms; _lUH = useHollows;
        _lMin = minSize; _lMax = maxSize;
        _lRA = roomAmountMultiplier; _lHA = hollowAmountMultiplier;
        _lEA = erosionAmount; _lER = erosionRandomness;
    }
    #endregion

    #region Initialization
    void Start()
    {
        // remove if removed runtime modification
#if UNITY_EDITOR
        CaptureState();
#endif
        Generate();
    }

    public void Generate()
    {
        ClearOldMaze();
        GenerateGrid();

        if (erosionAmount > 0) ApplyCircularErosion();
        if (useHollows && hollowAmountMultiplier > 0) GenerateStandaloneHollows();
        if (useRooms && roomAmountMultiplier > 0) GenerateRooms();

        GenerateMaze();

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.SetupSpawns(grid, width, height);
        }

        CleanUpLonelyPillars();
    }

    void ClearOldMaze()
    {
        MazeCell[] existingCells = GetComponentsInChildren<MazeCell>();
        for (int i = existingCells.Length - 1; i >= 0; i--)
        {
            if (Application.isPlaying) Destroy(existingCells[i].gameObject);
            else DestroyImmediate(existingCells[i].gameObject);
        }
        grid = null;
    }
    #endregion

    #region Grid Generation
    void GenerateGrid()
    {
        grid = new MazeCell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = Instantiate(cellPrefab, new Vector3(x * cellSize, 0, y * cellSize), Quaternion.identity, transform);

                grid[x, y].pillarTR?.SetActive(true);
                grid[x, y].wallTop?.SetActive(true);
                grid[x, y].wallRight?.SetActive(true);

                grid[x, y].pillarTL?.SetActive(false);
                grid[x, y].pillarBL?.SetActive(false);
                grid[x, y].pillarBR?.SetActive(false);
                grid[x, y].wallLeft?.SetActive(false);
                grid[x, y].wallBottom?.SetActive(false);

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
            }
        }
    }
    #endregion

    #region Erosion Logic
    void ApplyCircularErosion()
    {
        Vector2 center = new Vector2(width / 2f, height / 2f);
        float maxPossibleDist = Vector2.Distance(Vector2.zero, center);
        float seed = Random.Range(0f, 100f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null) continue;

                float dist = Vector2.Distance(new Vector2(x, y), center);
                float normalizedDist = dist / maxPossibleDist;
                float noise = Mathf.PerlinNoise(x * 0.1f * erosionRandomness + seed, y * 0.1f * erosionRandomness + seed);
                float noiseInfluence = noise * 0.2f * erosionAmount;

                if (normalizedDist + noiseInfluence > (1.2f - (erosionAmount * 0.7f)))
                {
                    RemoveAndEncloseCell(x, y);
                }
            }
        }
    }

    void RemoveAndEncloseCell(int x, int y)
    {
        if (grid[x, y] == null) return;

        if (x > 0 && grid[x - 1, y] != null) { grid[x - 1, y].wallRight?.SetActive(true); }
        if (y > 0 && grid[x, y - 1] != null) { grid[x, y - 1].wallTop?.SetActive(true); }
        if (x + 1 < width && grid[x + 1, y] != null) { grid[x + 1, y].wallLeft?.SetActive(true); }
        if (y + 1 < height && grid[x, y + 1] != null) { grid[x, y + 1].wallBottom?.SetActive(true); }

        if (Application.isPlaying) Destroy(grid[x, y].gameObject);
        else DestroyImmediate(grid[x, y].gameObject);
        grid[x, y] = null;
    }
    #endregion

    #region Room & Hollow Logic
    void GenerateRooms()
    {
        int baseAttempts = Mathf.Max(3, (width * height) / 100);
        int maxRoomAttempts = Mathf.RoundToInt(baseAttempts * roomAmountMultiplier);
        int bigRoomCount = 0;
        int maxBigRooms = (width > 20) ? 2 : 1;

        for (int i = 0; i < maxRoomAttempts; i++)
        {
            bool isBig = (bigRoomCount < maxBigRooms && Random.value < 0.3f);
            int rW = isBig ? Random.Range(6, 9) : Random.Range(minSize, maxSize);
            int rH = isBig ? Random.Range(6, 9) : Random.Range(minSize, maxSize);
            int startX = Random.Range(1, width - rW - 1);
            int startY = Random.Range(1, height - rH - 1);

            if (IsAreaAvailable(startX, startY, rW, rH, 1))
            {
                CarveRoom(startX, startY, rW, rH, isBig && useHollows && Random.value < 0.6f);
                if (isBig) { bigRoomCount++; ForceManyEntrances(startX, startY, rW, rH); }
                else { CreateRoomEntrance(startX, startY, rW, rH); }
            }
        }
    }

    void GenerateStandaloneHollows()
    {
        int baseAttempts = Mathf.Max(1, (width * height) / 150);
        int hollowAttempts = Mathf.RoundToInt(baseAttempts * hollowAmountMultiplier);
        for (int i = 0; i < hollowAttempts; i++)
        {
            int hW = Random.Range(minSize, maxSize);
            int hH = Random.Range(minSize, maxSize);
            int startX = Random.Range(2, width - hW - 2);
            int startY = Random.Range(2, height - hH - 2);

            if (IsAreaAvailable(startX, startY, hW, hH, 1))
            {
                for (int x = startX; x < startX + hW; x++)
                    for (int y = startY; y < startY + hH; y++)
                        RemoveAndEncloseCell(x, y);
            }
        }
    }

    void CarveRoom(int startX, int startY, int rWidth, int rHeight, bool addPlusHollow)
    {
        for (int x = startX; x < startX + rWidth; x++)
        {
            for (int y = startY; y < startY + rHeight; y++)
            {
                if (addPlusHollow && (x == startX + rWidth / 2 || y == startY + rHeight / 2)) { RemoveAndEncloseCell(x, y); continue; }
                if (grid[x, y] == null) continue;
                grid[x, y].visited = true;
                if (x > startX && grid[x - 1, y] != null) RemoveWalls(new Vector2Int(x, y), new Vector2Int(x - 1, y));
                if (y > startY && grid[x, y - 1] != null) RemoveWalls(new Vector2Int(x, y), new Vector2Int(x, y - 1));
                if (x < startX + rWidth - 1 && y < startY + rHeight - 1) grid[x, y].RemovePillarTR();
            }
        }
    }

    bool IsAreaAvailable(int startX, int startY, int rW, int rH, int buffer)
    {
        for (int x = startX - buffer; x < startX + rW + buffer; x++)
            for (int y = startY - buffer; y < startY + rH + buffer; y++)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) return false;
                if (grid[x, y] == null || grid[x, y].visited) return false;
            }
        return true;
    }
    #endregion

    #region Maze Generation
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
                Vector2Int next = neighbors[Random.Range(0, neighbors.Count)];
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
    #endregion

    #region Utility & Cleanup
    void CreateRoomEntrance(int startX, int startY, int rW, int rH)
    {
        int side = Random.Range(0, 4);
        Vector2Int roomCell = Vector2Int.zero;
        Vector2Int outsideCell = Vector2Int.zero;
        if (side == 0 && startX > 0) { roomCell = new Vector2Int(startX, startY + Random.Range(0, rH)); outsideCell = roomCell + Vector2Int.left; }
        else if (side == 1 && startX + rW < width) { roomCell = new Vector2Int(startX + rW - 1, startY + Random.Range(0, rH)); outsideCell = roomCell + Vector2Int.right; }
        else if (side == 2 && startY > 0) { roomCell = new Vector2Int(startX + Random.Range(0, rW), startY); outsideCell = roomCell + Vector2Int.down; }
        else if (side == 3 && startY + rH < height) { roomCell = new Vector2Int(startX + Random.Range(0, rW), startY + rH - 1); outsideCell = roomCell + Vector2Int.up; }

        if (outsideCell.x >= 0 && outsideCell.x < width && outsideCell.y >= 0 && outsideCell.y < height && grid[outsideCell.x, outsideCell.y] != null)
            RemoveWalls(roomCell, outsideCell);
    }

    void ForceManyEntrances(int sx, int sy, int w, int h)
    {
        for (int x = sx; x < sx + w; x += 3)
        {
            if (sy > 0 && grid[x, sy - 1] != null) RemoveWalls(new Vector2Int(x, sy), new Vector2Int(x, sy - 1));
            if (sy + h < height && grid[x, sy + h] != null) RemoveWalls(new Vector2Int(x, sy + h - 1), new Vector2Int(x, sy + h));
        }
        for (int y = sy; y < sy + h; y += 3)
        {
            if (sx > 0 && grid[sx - 1, y] != null) RemoveWalls(new Vector2Int(sx, y), new Vector2Int(sx - 1, y));
            if (sx + w < width && grid[sx + w, y] != null) RemoveWalls(new Vector2Int(sx + w - 1, y), new Vector2Int(sx + w, y));
        }
    }

    void CleanUpLonelyPillars()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null) continue;

                grid[x, y].pillarTR?.SetActive(false);
                grid[x, y].pillarTL?.SetActive(false);
                grid[x, y].pillarBL?.SetActive(false);
                grid[x, y].pillarBR?.SetActive(false);

                bool wT = grid[x, y].wallTop.activeSelf;
                bool wR = grid[x, y].wallRight.activeSelf;
                bool aboveR = (y + 1 < height && grid[x, y + 1] != null) && grid[x, y + 1].wallRight.activeSelf;
                bool rightT = (x + 1 < width && grid[x + 1, y] != null) && grid[x + 1, y].wallTop.activeSelf;

                if (wT || wR || aboveR || rightT)
                    grid[x, y].pillarTR?.SetActive(true);

                if (x == 0 || grid[x - 1, y] == null)
                {
                    bool wL = grid[x, y].wallLeft.activeSelf;
                    bool lT = (x > 0 && grid[x - 1, y] != null) ? grid[x - 1, y].wallTop.activeSelf : false;
                    if (wL || wT || lT)
                        grid[x, y].pillarTL?.SetActive(true);
                }

                if ((x == 0 && y == 0) || (x > 0 && y > 0 && grid[x - 1, y] == null && grid[x, y - 1] == null))
                {
                    if (grid[x, y].wallLeft.activeSelf || grid[x, y].wallBottom.activeSelf)
                        grid[x, y].pillarBL?.SetActive(true);
                }

                if (y == 0 || grid[x, y - 1] == null)
                {
                    bool wB = grid[x, y].wallBottom.activeSelf;
                    if (wB || wR)
                        grid[x, y].pillarBR?.SetActive(true);
                }
            }
        }
    }
    #endregion
}