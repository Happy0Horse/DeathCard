using System.Collections.Generic;
using UnityEngine;

public class RoomCell : MonoBehaviour
{
    [System.Serializable]
    public struct DoorWall
    {
        public Side side;
        public GameObject wallObject;
    }

    [Header("Cell Coordinates")]
    public int localX;
    public int localY;

    [Header("Valid Door Candidates")]
    public List<DoorWall> doorWalls = new();
}

public enum Side
{
    Top = 0,
    Right = 1,
    Bottom = 2,
    Left = 3
}