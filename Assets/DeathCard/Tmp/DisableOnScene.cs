using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class DisableOnScene : NetworkBehaviour
{
    public string sceneName;

    public override void OnStartLocalPlayer()
    {
        if (SceneManager.GetActiveScene().name == sceneName)
            gameObject.SetActive(false);
    }
}