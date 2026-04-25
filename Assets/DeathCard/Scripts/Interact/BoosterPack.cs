using UnityEngine;

public class BoosterPack : Interactable
{
    public Inventory inventory;
    public ItemData item;
    public override void Interact(bool unlockDoor = false, bool breakFences = false)
    {
        bool successful_add = inventory.AddItem(item);
        if (successful_add)
            Destroy(gameObject);
    }
}
