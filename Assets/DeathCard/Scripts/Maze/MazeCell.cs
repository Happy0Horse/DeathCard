using UnityEngine;

public class MazeCell : MonoBehaviour
{
    public GameObject wallLeft;
    public GameObject wallRight;
    public GameObject wallTop;
    public GameObject wallBottom;

    public bool visited = false;

    public void RemoveWallLeft()
    {
        wallLeft.SetActive(false);
    }

    public void RemoveWallRight()
    {
        wallRight.SetActive(false);
    }

    public void RemoveWallTop()
    {
        wallTop.SetActive(false);
    }

    public void RemoveWallBottom()
    {
        wallBottom.SetActive(false);
    }
}