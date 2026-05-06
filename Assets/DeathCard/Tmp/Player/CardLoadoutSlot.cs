using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardLoadoutSlot : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public ItemData currentItem;

    public Inventory inventory;

    public CardManager cardManager;

    [Header("Restrictions")]
    public bool restricted = false;
    public CardData.CardCategory allowedCategory;

    public bool IsEmpty => currentItem == null;



    public bool CanPlace(CardData card)
    {
        if (!restricted) return true;
        return card.category == allowedCategory;
    }

    public void SetItem(ItemData item)
    {
        currentItem = item;
        icon.sprite = item.icon;
        icon.color = Color.white;
    }

    public void Clear()
    {
        currentItem = null;
        icon.sprite = null;
        icon.color = new Color32(31, 25, 25, 255);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (currentItem == null) return;

        inventory.AddItem(currentItem);

        CardData card = currentItem as CardData;
        if (card != null)
        {
            cardManager.RemoveSelectedCard(card);
        }

        Clear();
    }
}