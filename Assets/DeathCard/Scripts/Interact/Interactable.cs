using UnityEngine;

public abstract class Interactable : MonoBehaviour {
    public abstract void Interact(bool unlockDoor = false, bool breakFences = false);
}