using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public Transform cameraTransform;
    public float interactDistance = 3f;

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

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            return hit.collider.GetComponent<Interactable>();
        }

        return null;
    }

    public void OnInteract()
    {
        Interactable obj = GetInteractable();

        if (obj != null)
            obj.Interact();
    }
}