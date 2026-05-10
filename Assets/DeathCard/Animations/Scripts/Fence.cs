using UnityEngine;

public class Fence : Interactable
{
    private Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void Interact(PlayerInteract player = null, bool unlockDoor = false, bool breakFences = false)
    {
        if (breakFences)
        {
            if (animator.GetBool("IsFenced"))
            {
                animator.SetBool("IsFenced", false);
            }

            Destroy(gameObject, 1f);
        }
    }
}