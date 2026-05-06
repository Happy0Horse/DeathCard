using Mirror.BouncyCastle.Pqc.Crypto.Lms;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int maxSlots = 28;
    public List<ItemData> items = new List<ItemData>();
    public InventoryUI inventoryUI;
    public bool AddItem(ItemData item)
    {
        if (items.Count >= maxSlots)
        {
            return false;
        }
        items.Add(item);
        Debug.Log($"Added {item.name} to inventory. Total items: {items.Count}/{maxSlots}");
        inventoryUI.UpdateUI();
        return true;
    }

    public void RemoveItem(ItemData item)
    {
        items.Remove(item);
        inventoryUI.UpdateUI();
    }

}

