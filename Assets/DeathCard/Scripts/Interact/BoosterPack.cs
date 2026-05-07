using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

    void GetCards(PlayerInteract player, bool unlockDoor, bool breakFences)
    {
        List<ItemData> itemList = new List<ItemData>();
        System.Random random = new System.Random();

        if (PlayerPrefs.GetInt("BreakFences", 0) == 0)
        {
            if (random.Next(0, 10) < 1)
                PlayerPrefs.SetInt("BreakFences", 1);
              
        }
        if (PlayerPrefs.GetInt("UnlockDoor", 0) == 0)
        {
            if (random.Next(0, 10) < 2)
                PlayerPrefs.SetInt("UnlockDoor", 1);

        }
        if (PlayerPrefs.GetInt("CanCrouch", 0) == 0)
        {
            if (random.Next(0, 10) < 3)
                PlayerPrefs.SetInt("CanCrouch", 1); 
        }
        for (int i = itemList.Count; i < 5; ++i)
        {
            int rand = random.Next(0, 10);
            if (rand < 3)
            {
                List<CardData> cardPool = new List<CardData>();
                for (int j = 0; j < items.Length; ++j)
                {
                    if(items[j] is CardData && (items[j] as CardData).category == CardData.CardCategory.Move)
                        cardPool.Add(items[j] as CardData);
                }
                itemList.Add(cardPool[random.Next(0, cardPool.Count)]);
            }
            else if (rand >= 3 && rand <= 6)
            {
                List<CardData> cardPool = new List<CardData>();
                for (int j = 0; j < items.Length; ++j)
                {
                    if (items[j] is CardData && (items[j] as CardData).category == CardData.CardCategory.Attack)
                        cardPool.Add(items[j] as CardData);
                }
                itemList.Add(cardPool[random.Next(0, cardPool.Count)]);
            }
            else if (rand > 6 && rand <= 9)
            {
                List<CardData> cardPool = new List<CardData>();
                for (int j = 0; j < items.Length; ++j)
                {
                    if (items[j] is CardData && (items[j] as CardData).category == CardData.CardCategory.Trap)
                        cardPool.Add(items[j] as CardData);
                }
                itemList.Add(cardPool[random.Next(0, cardPool.Count)]);
            }
            else if (rand == 10)
            {
                List<CardData> cardPool = new List<CardData>();
                for (int j = 0; j < items.Length; ++j)
                {
                    if (items[j] is CardData && (items[j] as CardData).category == CardData.CardCategory.Utility)
                        cardPool.Add(items[j] as CardData);
                }
                itemList.Add(cardPool[random.Next(0, cardPool.Count)]);
            }
        }
        for (int q = 0; q < itemList.Count; ++q)
        {
            player.inventory.AddItem(itemList[q]);
        }
    }

    public override void Interact(PlayerInteract player = null, bool unlockDoor = false, bool breakFences = false)
    {
        if (player.playerType == PlayerInteract.PlayerType.MazePlayer)
        {
            bool successfulAdd = player.inventory.AddItem(item);
            if (successfulAdd)
            {
                PlayerPrefs.SetInt("BoosterPacks", PlayerPrefs.GetInt("BoosterPacks", 0) + 1);
                animator.SetTrigger("PickUp");
                Destroy(gameObject, 1f);
            }
        }
        else
        {
            if (items.Length >= 5)
            {
                gameObject.layer = LayerMask.NameToLayer("Default");


                GetCards(player, unlockDoor, breakFences);

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
}
