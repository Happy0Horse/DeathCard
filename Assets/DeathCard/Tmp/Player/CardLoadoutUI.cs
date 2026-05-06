using UnityEngine;

public class CardLoadoutUI : MonoBehaviour
{
    public CardLoadoutSlot[] slots;

    public bool AddCard(ItemData item)
    {
        CardData card = item as CardData;

        if (card == null)
        {
            return false;
        }

        foreach (CardLoadoutSlot slot in slots)
        {
            if (!slot.IsEmpty)
                continue;

            if (!slot.CanPlace(card))
                continue;

            slot.SetItem(item);
            return true;
        }

        return false;
    }
}