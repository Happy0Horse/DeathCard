using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;
    public InventorySlot[] slots;

    public void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                Debug.Log($"Slot {inventory.items[i].itemName}");
                slots[i].SetItem(inventory.items[i]);
            }
            else
                slots[i].Clear();
            //break;
            //slots[i].Clear();
        }
    }
}