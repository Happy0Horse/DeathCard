using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class CardLoadoutSlot : MonoBehaviour, IPointerClickHandler
{
    public ItemData currentItem;
    public Inventory inventory;
    public CardManager cardManager;
    [Header("Restrictions")]
    public bool restricted = false;
    public CardData.CardCategory allowedCategory;
    private InventorySlot _inventorySlot;
    private void Awake()
    {
        _inventorySlot = GetComponent<InventorySlot>();
    }
    public bool IsEmpty => currentItem == null;
    public bool CanPlace(CardData card)
    {
        if (!restricted) return true;
        return card.category == allowedCategory;
    }
    public void SetItem(ItemData item)
    {
        currentItem = item;
        _inventorySlot.SetItem(item);
    }
    public void Clear()
    {
        currentItem = null;
        _inventorySlot.Clear();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            eventData.Use();
            return;
        }

        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (currentItem == null) return;
        inventory.AddItem(currentItem);
        CardData card = currentItem as CardData;
        if (card != null)
            cardManager.RemoveSelectedCard(card);
        Clear();
    }
}