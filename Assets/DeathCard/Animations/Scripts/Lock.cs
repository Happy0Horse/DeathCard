
using System.Collections;
using UnityEngine;

public class Lock : Interactable
{

    [SerializeField] private Door doorMiddle;
    [SerializeField] private GameObject lock2;

    private Animator animator1;
    private Animator animator2;

    void Awake()
    {
        animator1 = GetComponent<Animator>();
        animator2 = lock2.GetComponent<Animator>();
        doorMiddle.canOpen = false;
    }

    public override void Interact(bool unlockDoor = false, bool breakFences = false)
    {
        if (unlockDoor)
        {
            animator1.SetBool("IsLock", false);
            animator2.SetBool("IsLock", false);

            doorMiddle.canOpen = true;

            Destroy(gameObject, 1f);
            Destroy(lock2, 1f);
        }
        else
        {
            animator1.SetTrigger("Lock");
            animator2.SetTrigger("Lock");
        }
    }
}