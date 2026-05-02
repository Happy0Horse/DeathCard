using UnityEngine;

public class BoosterPack : Interactable
{
    private Animator animator;

    public ItemData[] items;

    public ItemData item;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void Interact(PlayerInteract player = null, bool unlockDoor = false, bool breakFences = false)
    {
        if (player.playerType == PlayerInteract.PlayerType.MazePlayer)
        {
            bool successfulAdd = player.inventory.AddItem(item);
            if (successfulAdd)
            {
                animator.SetTrigger("PickUp");
                Destroy(gameObject, 1f);
            }
        }
        else
        {
            if (items.Length >= 5)
            {
                System.Random random = new System.Random();

                for (int i = 0; i < 5; ++i)
                {
                    player.inventory.AddItem(items[random.Next(0, items.Length)]);
                }

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
}
