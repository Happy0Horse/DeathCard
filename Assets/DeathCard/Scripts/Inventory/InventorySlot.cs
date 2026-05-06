using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public ItemData currentItem;
    public ContextMenuUI contextMenu;

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
        Debug.Log("Clicked slot: " + eventData.button);

        if (currentItem == null)
        {
            Debug.Log("Slot empty");
            return;
        }

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