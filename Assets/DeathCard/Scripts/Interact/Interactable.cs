using UnityEngine;

public abstract class Interactable : MonoBehaviour {
    public abstract void Interact(PlayerInteract player = null, bool unlockDoor = false, bool breakFences = false);
}