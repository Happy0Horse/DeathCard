using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomData
{
    public RectInt bounds;
    public bool isBig;
    public List<Vector2Int> entrances = new List<Vector2Int>();
    public List<Vector2Int> finalDoorPositions = new List<Vector2Int>();

    public RoomData(int x, int y, int w, int h, bool big)
    {
        bounds = new RectInt(x, y, w, h);
        isBig = big;
    }

    public void AddEntrance(int x, int y)
    {
        entrances.Add(new Vector2Int(x, y));
    }
}