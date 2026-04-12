using UnityEngine;

public class Infectable : MonoBehaviour
{
    public void Infect()
    {
       this.gameObject.tag = "Infected";
    }
}