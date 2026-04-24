using UnityEngine;

public class AttackButtonHandler : MonoBehaviour
{
    public void TriggerAttackMode()
    {
        GameEvents.OnRequestAttackMode?.Invoke();
    }
}