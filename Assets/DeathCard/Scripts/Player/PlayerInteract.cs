using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public Transform cameraTransform;
    public float interactDistance = 3f;
    public DebuffSystem debuffSystem;

    void Update()
    {
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
        {
            return hit.collider.GetComponent<Interactable>();
        }
        return null;
    }

    public void OnInteract()
    {
        Interactable obj = GetInteractable();

        if (obj != null)
        {
            if (obj.CompareTag("Infected"))
            {
                debuffSystem.ApplyRandomDebuff();
            }

            obj.Interact();
        }
    }
}