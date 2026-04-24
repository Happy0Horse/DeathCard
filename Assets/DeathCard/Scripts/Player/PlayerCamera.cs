using UnityEngine;
using Mirror;

public class PlayerCamera : NetworkBehaviour
{
    public Camera playerCamera;

    public override void OnStartLocalPlayer()
    {
        playerCamera.gameObject.SetActive(true);
    }

    void Awake()
    {
        playerCamera.gameObject.SetActive(false);
    }
}