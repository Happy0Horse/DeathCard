using System;
using System.Collections;
using System.Collections.Generic;
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

    private bool _canCheckOutsideClick = false;
    private Canvas _canvas;
    private Coroutine _outsideClickCoroutine;

    public void Show(ItemData item, Vector2 screenPosition)
    {
        currentItem = item;

        panel.SetActive(true);

        _canvas = panel.GetComponentInParent<Canvas>();

        RectTransform canvasRect = _canvas.GetComponent<RectTransform>();
        RectTransform panelRect = panel.GetComponent<RectTransform>();

        Camera uiCamera = GetUICamera();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            uiCamera,
            out Vector2 localPoint
        );

        panelRect.anchoredPosition = localPoint + new Vector2(90f, 0f);

        _canCheckOutsideClick = false;

        if (_outsideClickCoroutine != null)
            StopCoroutine(_outsideClickCoroutine);

        _outsideClickCoroutine = StartCoroutine(EnableOutsideClickAfterOpeningClick());
    }

    private IEnumerator EnableOutsideClickAfterOpeningClick()
    {
        yield return null;

        while (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            yield return null;
        }

        yield return null;

        _canCheckOutsideClick = true;
    }

    void Update()
    {
        if (!panel.activeSelf) return;

        if (Keyboard.current != null &&
            (Keyboard.current.escapeKey.wasPressedThisFrame ||
             Keyboard.current.tabKey.wasPressedThisFrame))
        {
            Hide();
            return;
        }

        if (!_canCheckOutsideClick) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (IsPointerOverMenu())
                return;

            Hide();
        }
    }

    private bool IsPointerOverMenu()
    {
        if (EventSystem.current == null)
            return IsPointerInsideMenuRect();

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            Transform hitTransform = result.gameObject.transform;

            if (hitTransform == menuRect || hitTransform.IsChildOf(menuRect))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPointerInsideMenuRect()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        return RectTransformUtility.RectangleContainsScreenPoint(
            menuRect,
            mousePosition,
            GetUICamera()
        );
    }

    private Camera GetUICamera()
    {
        if (_canvas == null)
            _canvas = panel.GetComponentInParent<Canvas>();

        if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return _canvas.worldCamera;
    }

    public void Hide()
    {
        panel.SetActive(false);
        _canCheckOutsideClick = false;
    }

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

    public void OnInteract()
    {
        if (currentItem == null || currentPlayer == null || currentItem.itemName != "BoosterPack")
            return;

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
        Debug.Log("It gets here first");

        CardData secondCard = secondItem as CardData;

        if (secondCard == null)
            return;

        Debug.Log("Then its not null");

        if (secondCard == firstMergeCard)
            return;

        Debug.Log("Then they arent the same");


        if (secondCard.level != firstMergeCard.level)
            return;

        Debug.Log("Then they are the same level");


        if (secondCard.name != firstMergeCard.name)
            return;


        Debug.Log("Then they are of the same name");

        if (secondCard.level >= secondCard.maxLevel)
            return;

        firstMergeCard.level++;

        inventory.RemoveItem(secondItem);

        isMergeMode = false;
        firstMergeCard = null;

        inventory.inventoryUI.UpdateUI();

        Debug.Log("Cards merged");
    }
}