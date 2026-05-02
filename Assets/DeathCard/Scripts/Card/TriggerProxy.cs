using UnityEngine;

public class TriggerProxy : MonoBehaviour
{
    public CardAttackLander parentLander;

    private void OnTriggerEnter(Collider other)
    {
        parentLander.HandleCollision(other);
    }
}