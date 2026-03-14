using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;

    public MazeCell cellPrefab;
    public float cellSize = 4f;

    private MazeCell[,] grid;

    public GameObject player;
    public float playerHeight = 1f;

    void Start()
    {
        GenerateGrid();
        GenerateMaze();
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        int x = Random.Range(0, width);
        int y = Random.Range(0, height);

        Vector3 spawnPos = grid[x, y].transform.position + new Vector3(0, playerHeight, 0);
        Instantiate(player, spawnPos, Quaternion.identity);
    }

    void GenerateGrid()
    {
        grid = new MazeCell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                MazeCell cell = Instantiate(
                    cellPrefab,
                    new Vector3(x * cellSize, 0, y * cellSize),
                    Quaternion.identity,
                    transform
                );

                grid[x, y] = cell;
            }
        }
    }

    void GenerateMaze()
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        Vector2Int current = new Vector2Int(0, 0);
        grid[0, 0].visited = true;
        stack.Push(current);

        while (stack.Count > 0)
        {
            current = stack.Pop();

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

        if (cell.x > 0 && !grid[cell.x - 1, cell.y].visited)
            neighbors.Add(new Vector2Int(cell.x - 1, cell.y));

        if (cell.x < width - 1 && !grid[cell.x + 1, cell.y].visited)
            neighbors.Add(new Vector2Int(cell.x + 1, cell.y));

        if (cell.y > 0 && !grid[cell.x, cell.y - 1].visited)
            neighbors.Add(new Vector2Int(cell.x, cell.y - 1));

        if (cell.y < height - 1 && !grid[cell.x, cell.y + 1].visited)
            neighbors.Add(new Vector2Int(cell.x, cell.y + 1));

        return neighbors;
    }

    void RemoveWalls(Vector2Int current, Vector2Int next)
    {
        int dx = current.x - next.x;
        int dy = current.y - next.y;

        if (dx == 1)
        {
            grid[current.x, current.y].RemoveWallLeft();
            grid[next.x, next.y].RemoveWallRight();
        }
        else if (dx == -1)
        {
            grid[current.x, current.y].RemoveWallRight();
            grid[next.x, next.y].RemoveWallLeft();
        }

        if (dy == 1)
        {
            grid[current.x, current.y].RemoveWallBottom();
            grid[next.x, next.y].RemoveWallTop();
        }
        else if (dy == -1)
        {
            grid[current.x, current.y].RemoveWallTop();
            grid[next.x, next.y].RemoveWallBottom();
        }
    }
}