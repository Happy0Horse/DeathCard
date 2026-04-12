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

    [Header("Decorations")]
    public GameObject torchPrefab;
    public Transform[] torchMountPoints;

    public Renderer floorRenderer;
    public bool visited = false;
    private int Health;

    public void SetFloorMaterial(Material newMat)
    {
        if (newMat == null) return;

        Renderer r = GetComponentInChildren<Renderer>();

        if (r != null)
        {
            r.sharedMaterial = newMat;
        }
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
    public bool IsWallActive(int direction)
    {
        switch (direction)
        {
            case 0: return wallTop != null && wallTop.activeSelf;
            case 1: return wallRight != null && wallRight.activeSelf;
            case 2: return wallBottom != null && wallBottom.activeSelf;
            case 3: return wallLeft != null && wallLeft.activeSelf;
            default: return true;
        }
    }
}