using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
   public bool unlockDoor = false;
   public bool breakFences = false;

    public Transform cameraTransform;
    public float interactDistance = 3f;
    public DebuffSystem debuffSystem;

    void Update()
    {
        Interactable obj = GetInteractable();

        if (obj != null && obj.tag == "Card")
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
        Debug.Log(obj);

        if (obj != null)
        {
            //Debug.Log("2");
            //if (obj.CompareTag("Infected"))
            //{
            //    debuffSystem.ApplyRandomDebuff();
            //}


            //else if(obj.CompareTag("LockDoor") && !unlockDoor)
            //{
            //    return;
            //}
            //else if (obj.CompareTag("Fence") && !breakFences)
            //{
            //    return;
            //}
            obj.Interact(unlockDoor, breakFences);
        }
    }
}