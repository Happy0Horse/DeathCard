using Unity.VisualScripting;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public virtual void Interact()
    {
        Destroy(this.gameObject);
    }
}
