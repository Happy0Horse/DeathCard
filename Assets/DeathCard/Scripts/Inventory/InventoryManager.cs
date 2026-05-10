using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : NetworkBehaviour
{    
    public PlayerAnimation playerAnimation;

    public GameObject panel;

    public HexViewManager manager;

    public Inventory inventory;

    public ItemData boosterPackItem;

    private bool isOpen = false;
    private bool canOpen = true;

    private void OnEnable()
    {
        NetworkClient.RegisterHandler<GameStartedMessage>(OnGameStarted);
        NetworkClient.RegisterHandler<RoundOverMessage>(OnRoundOver);
    }

    private void OnDisable()
    {
        NetworkClient.UnregisterHandler<GameStartedMessage>();
        NetworkClient.UnregisterHandler<RoundOverMessage>();
    }

    private void OnGameStarted(GameStartedMessage msg) => DisableInventory();
    private void OnRoundOver(RoundOverMessage msg) => EnableInventory(msg.round);

    private void DisableInventory()
    {
        canOpen = false;
        if (isOpen)
        {
            isOpen = false;
            panel.SetActive(false);
            playerAnimation.enabled = true;
            manager.enabled = true;
            checkPosition();
        }
    }

    private void EnableInventory(int domeIndex)
    {
        canOpen = true;
    }

    void checkPosition()
    {

        if (manager.CurrentView == HexViewManager.ViewMode.Orbit || manager.CurrentView == HexViewManager.ViewMode.FirstPerson)
        {
            if (isOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }       
    }

    void LoadBoosterPacksToInventory()
    {
        //int boosterPacks = PlayerPrefs.GetInt("BoosterPacks", 1);
        Debug.LogAssertion("Loading " + LocalStorage.Instance.boosterPackCount + " booster packs to inventory.");

        for (int i = 0; i < LocalStorage.Instance.boosterPackCount - 1; ++i)
        {
            inventory.AddItem(boosterPackItem);
        }

        //PlayerPrefs.SetInt("BoosterPacks", 0);
        LocalStorage.Instance.boosterPackCount = 0;
        //PlayerPrefs.Save();
    }

    void LoadItemsToInventory()
    {
        List<ItemData> items = LocalStorage.Instance.items;
        foreach (ItemData item in items)
        {
            inventory.AddItem(item);
            Debug.Log("Loaded " + item + " to inventory from LocalStorage");
        }
    }

    private void Awake()
    {
        if(LocalStorage.Instance.items.Count != 0)
            LoadItemsToInventory();
        if (LocalStorage.Instance.boosterPackCount > 0)
            LoadBoosterPacksToInventory();
    }

    public void OnInventory(InputValue value)
    {
        if (!canOpen) return;

        isOpen = !isOpen;
        panel.SetActive(isOpen);

        if (isOpen)
        {
            playerAnimation.enabled = false;
            manager.enabled = false;
            checkPosition();
        }
        else
        {
            playerAnimation.enabled = true;
            manager.enabled = true;
            checkPosition();
        }
    }
}
