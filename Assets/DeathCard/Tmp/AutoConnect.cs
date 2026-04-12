using Mirror;
using UnityEngine;

public class AutoConnect : MonoBehaviour
{
    void Start()
    {
#if UNITY_SERVER
        NetworkManager.singleton.StartServer();
#else
        NetworkManager.singleton.StartClient();
#endif
    }
}