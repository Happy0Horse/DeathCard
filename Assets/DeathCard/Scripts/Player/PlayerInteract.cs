using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using NUnit.Framework;

public class PlayerInteract : NetworkBehaviour
{
    public bool unlockDoor = false;
    public bool breakFences = false;

    public Transform cameraTransform;
    public float interactDistance = 3f;
    public DebuffSystem debuffSystem;
    public Inventory inventory;
    public enum PlayerType
    {
        MazePlayer,
        MainPlayer
    }
    public PlayerType playerType;

    void Update()
    {
        //if (!isLocalPlayer) return;

        Interactable obj = GetInteractable();

        //if (obj != null && obj.tag == "Card")
        //    InteractionUI.instance.Show();
        //else
        //    InteractionUI.instance.Hide();
    }

    Interactable GetInteractable()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        int layerMask = LayerMask.GetMask("Interactable");

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, layerMask))
        {
            return hit.collider.GetComponent<Interactable>();
        }

        return null;
    }

    public void OnInteract()
    {
        //if (!isLocalPlayer) return;

        Interactable obj = GetInteractable();
        Debug.Log("Interacting with: " + (obj != null ? obj.name : "nothing"));

        if (obj != null)
        {
            obj.Interact(this);
        }
    }
}