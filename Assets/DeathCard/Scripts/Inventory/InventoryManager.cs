using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : NetworkBehaviour
{
    public GameObject panel;

    public HexViewManager manager;

    public Inventory inventory;

    public ItemData boosterPackItem;

    private bool isOpen = false;

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
        int boosterPacks = PlayerPrefs.GetInt("BoosterPacks", 3);

        Debug.Log($"Loading {boosterPacks} booster packs into inventory.");
        for (int i = 0; i < 3; ++i)
        {
            Debug.Log($"Loading {i} booster packs into inventory.");
            inventory.AddItem(boosterPackItem);
        }

        PlayerPrefs.SetInt("BoosterPacks", 0);
        PlayerPrefs.Save();
    }

    private void Start()
    {

        LoadBoosterPacksToInventory();

    }

    public void OnInventory(InputValue value)
    {
        //if (!isLocalPlayer) return;

        int boosterPacks = PlayerPrefs.GetInt("BoosterPacks", 3);
        if (boosterPacks != 0)
        {

        }

        isOpen = !isOpen;
        panel.SetActive(isOpen);

        if (isOpen)
        {
            manager.enabled = false;
            checkPosition();
        }
        else
        {
            manager.enabled = true;
            checkPosition();
        }
    }
}
