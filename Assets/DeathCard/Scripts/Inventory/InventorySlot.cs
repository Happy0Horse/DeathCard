using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public ItemData currentItem;
    public ContextMenuUI contextMenu;
    [SerializeField] private CardInventorySlot cardSlot;
    private CardLoadoutSlot _loadoutSlot;

    private void Awake()
    {
        _loadoutSlot = GetComponent<CardLoadoutSlot>();
    }


    private void OnEnable()
    {
        if (currentItem is CardData cardData && cardSlot != null)
        {
            cardSlot.gameObject.SetActive(true);
            cardSlot.SetCard(cardData);
        }
        else if (cardSlot != null)
        {
            cardSlot.gameObject.SetActive(false);
        }
    }

    public void SetItem(ItemData item)
    {
        currentItem = item;
        if (cardSlot != null && item is CardData cardData)
        {
            cardSlot.gameObject.SetActive(true);
            cardSlot.SetCard(cardData);
        }
        else
        {
            if (cardSlot != null) cardSlot.gameObject.SetActive(false);
            if (icon != null)
            {
                icon.sprite = item.icon;
                icon.color = Color.white;
            }
        }
    }

    public void Clear()
    {
        currentItem = null;
        if (cardSlot != null) cardSlot.gameObject.SetActive(false);
        if (icon != null)
        {
            icon.sprite = null;
            icon.color = new Color32(31, 25, 25, 255);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked slot: " + eventData.button);
        if (currentItem == null)
        {
            Debug.Log("Slot empty");
            return;
        }
        if (_loadoutSlot != null) return;
        if (contextMenu.IsMergeMode && eventData.button == PointerEventData.InputButton.Left)
        {
            contextMenu.TryMergeWith(currentItem);
            return;
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            contextMenu.Show(currentItem, eventData.position);
        }
    }
}