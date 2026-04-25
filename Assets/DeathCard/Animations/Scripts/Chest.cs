using UnityEngine;

public class Chest : Interactable
{
    private Animator animator;

    public Inventory inventory;
    public ItemData item;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }


    public override void Interact(bool unlockDoor = false, bool breakFences = false)
    {
        bool successful_add = inventory.AddItem(item);
        if (successful_add)
        {
            System.Random random = new System.Random();
            int rand = random.Next(0, 3);
            if (rand == 0)
            {
                animator.SetTrigger("f_anim");
            }
            else if (rand == 1)
            {
                animator.SetTrigger("s_anim");
            }
            else
            {
                animator.SetTrigger("t_anim");
            }
            Destroy(gameObject, 0.8f);
        }
    }
}
