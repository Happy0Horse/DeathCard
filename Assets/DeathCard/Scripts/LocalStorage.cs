using System.Collections.Generic;
using UnityEngine;

public class LocalStorage : MonoBehaviour
{
    public static LocalStorage Instance;

    public List<ItemData> items = new List<ItemData>();

    public int boosterPackCount = 0;

    public bool unlockDoor = false;
    public bool breakFences = false;
    public bool canCrouch = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
