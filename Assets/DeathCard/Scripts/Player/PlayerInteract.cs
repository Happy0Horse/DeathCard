using UnityEngine;
using Mirror;

public class PlayerInteract : NetworkBehaviour
{
    public Transform cameraTransform;
    public float interactDistance = 3f;
    public DebuffSystem debuffSystem;

    void Update()
    {
        if (!isLocalPlayer) return;

        Interactable obj = GetInteractable();

        if (obj != null)
            InteractionUI.instance.Show();
        else
            InteractionUI.instance.Hide();
    }

    Interactable GetInteractable()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Interactable");

        if (Physics.Raycast(ray, out hit, interactDistance, layerMask))
            return hit.collider.GetComponent<Interactable>();

        return null;
    }

    public void OnInteract()
    {
        if (!isLocalPlayer) return;

        Interactable obj = GetInteractable();

        if (obj != null)
        {
            if (obj.CompareTag("Infected"))
                debuffSystem.ApplyRandomDebuff();

            CmdInteract(obj.gameObject);
        }
    }

    [Command]
    void CmdInteract(GameObject target)
    {
        Interactable obj = target.GetComponent<Interactable>();
        if (obj != null)
            obj.Interact();
    }
}