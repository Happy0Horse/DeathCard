using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ContextMenuUI : MonoBehaviour
{
    public GameObject panel;
    public RectTransform menuRect;

    public GameObject spawnPrefab;
    public HexGrid grid;
    public float heightOffset = 1f;

    public PlayerInteract currentPlayer;
    public CardManager cardManager;
    public CardLoadoutUI loadoutUI;

    public Inventory inventory;

    public bool IsMergeMode => isMergeMode;

    private bool isMergeMode = false;
    private ItemData currentItem;
    private CardData firstMergeCard;



    public void OnEquip()
    {
        if (cardManager == null && currentPlayer != null)
            cardManager = currentPlayer.GetComponent<CardManager>();

        if (cardManager == null)
        {
            Debug.LogError("cardManager is null");
            return;
        }

        CardData card = currentItem as CardData;
        if (card == null) return;

        bool added = loadoutUI.AddCard(currentItem);

        if (added)
        {
            cardManager.AddSelectedCard(card);
            inventory.RemoveItem(currentItem);
        }

        Hide();
    }


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
        else if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame)
        {
            Hide();
        }
    }

    public void OnInteract()
    {

        if (currentItem == null || currentPlayer == null || currentItem.itemName != "BoosterPack") return;
        inventory.RemoveItem(currentItem);

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

        Hide();
    }

    public void OnMerge()
    {
        CardData card = currentItem as CardData;

        if (card == null)
        {
            Debug.Log("This item is not a card");
            Hide();
            return;
        }

        if (card.level >= card.maxLevel)
        {
            Debug.Log("Card already has max level");
            Hide();
            return;
        }

        firstMergeCard = card;
        isMergeMode = true;

        Debug.Log("Select second same card to merge");

        Hide();
    }

    public void TryMergeWith(ItemData secondItem)
    {
        if (!isMergeMode) return;

        CardData secondCard = secondItem as CardData;
        if (secondCard == firstMergeCard)
            return;
        else if (secondCard.level != firstMergeCard.level)
            return;
        else if (secondCard.name != firstMergeCard.name)
            return;
        else if (secondCard == null)
            return;
        else if (secondCard.level >= secondCard.maxLevel)
            return;
        

        firstMergeCard.level++;

        inventory.RemoveItem(secondItem);

        isMergeMode = false;
        firstMergeCard = null;

        inventory.inventoryUI.UpdateUI();

        Debug.Log("Cards merged");
    }
}