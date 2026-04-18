using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    public Vector2Int coordinates;
    public List<HexCell> neighbors = new List<HexCell>();

    [Header("Movement Settings")]
    public bool canWalkOn = true;

    public void Initialize(Vector2Int coords)
    {
        this.coordinates = coords;
        this.neighbors.Clear();
    }
}