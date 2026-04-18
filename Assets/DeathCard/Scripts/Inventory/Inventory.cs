using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int maxSlots = 4;
    public List<ItemData> items = new List<ItemData>();
    public InventoryUI inventoryUI;
    public bool AddItem(ItemData item)
    {
        Debug.Log(items.Count);
        if (items.Count >= maxSlots)
        {
            return false;
        }
        items.Add(item);
        inventoryUI.UpdateUI();
        return true;
    }

    //public void RemoveItem(ItemData item)
    //{
    //    items.Remove(item);
    //    inventoryUI.UpdateUI();
    //}
}