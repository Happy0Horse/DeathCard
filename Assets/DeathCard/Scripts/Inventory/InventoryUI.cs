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
                slots[i].SetItem(inventory.items[i]);
            else
                break;
            //slots[i].Clear();
        }
    }
}