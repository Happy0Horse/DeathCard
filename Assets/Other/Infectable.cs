using UnityEngine;

public class Infectable : MonoBehaviour
{
    Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    public void Infect()
    {
        rend.material.color = new Color(0.6f, 0f, 0.8f); // фіолетовий
    }
}