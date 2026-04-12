using UnityEngine;

public class MazeCell : MonoBehaviour
{
    [Header("Walls")]
    public GameObject wallLeft;
    public GameObject wallRight;
    public GameObject wallTop;
    public GameObject wallBottom;

    [Header("Corners")]
    public GameObject pillarTR;
    public GameObject pillarTL;
    public GameObject pillarBR;
    public GameObject pillarBL;


    public Renderer floorRenderer;
    public bool visited = false;

    public void SetFloorColor(Color color)
    {
        if (floorRenderer != null) floorRenderer.material.color = color;
    }

    public void SetupPillars(int x, int y, int maxWidth, int maxHeight)
    {
        if (pillarTL) pillarTL.SetActive(x == 0);
        if (pillarBL) pillarBL.SetActive(x == 0 && y == 0);
        if (pillarBR) pillarBR.SetActive(y == 0);
    }
    public void RemovePillarTR() { if (pillarTR) pillarTR.SetActive(false); }
    public void RemoveWallRight() { if (wallRight) wallRight.SetActive(false); }
    public void RemoveWallTop() { if (wallTop) wallTop.SetActive(false); }
}