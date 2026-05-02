using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ContextMenuUI : MonoBehaviour
{
    public GameObject panel;
    public RectTransform menuRect;

    private ItemData currentItem;

    public GameObject spawnPrefab;
    public HexGrid grid;
    public float heightOffset = 1f;

    public PlayerInteract currentPlayer;

    public Inventory inventory;
    public void Show(ItemData item, Vector2 position)
    {
        currentItem = item;
        panel.SetActive(true);
        panel.transform.position = position + new Vector2(90f, -100f);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    void Update()
    {
        if (!panel.activeSelf) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            bool clickedInsideMenu = RectTransformUtility.RectangleContainsScreenPoint(
                menuRect,
                Mouse.current.position.ReadValue()
            );

            if (!clickedInsideMenu)
            {
                Hide();
            }
        }
    }

    public void OnInteract()
    {
        if (currentItem == null || currentPlayer == null) return;

        var navigator = currentPlayer.GetComponent<HexGridNavigator>();

        if (navigator == null)
        {
            Debug.LogError("Navigator not found");
            return;
        }

        Vector2Int coords = navigator.CurrentCoordinates;
        HexGrid grid = navigator.grid;
        HexCell currentCell = grid.GetCell(coords);

        if (currentCell == null) return;

        HexCell neighbor = currentCell.neighbors.Find(n => n.canWalkOn);

        if (neighbor == null)
        {
            Debug.Log("No free neighbor");
            return;
        }

        Vector3 pos = neighbor.transform.position;
        pos.y += heightOffset;

        GameObject obj = Instantiate(spawnPrefab, pos, Quaternion.Euler(0f, -90f, 0f));
        obj.transform.localScale = Vector3.one * 0.5f;

        Debug.Log("Spawned from item: " + currentItem.name);

        inventory.RemoveItem(currentItem);
        Hide();
    }
}