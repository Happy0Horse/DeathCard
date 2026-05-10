using UnityEngine;

public class Door : Interactable
{
    public bool canOpen = true;
    private Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void Interact(PlayerInteract player = null, bool unlockDoor = false, bool breakFences = false)
    {
        if(canOpen)
        {
            bool state = animator.GetBool("Interact");
            animator.SetBool("Interact", !state);
        }
    }
}