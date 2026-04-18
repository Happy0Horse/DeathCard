using UnityEngine;

public class EnvironmentButtonHandler : MonoBehaviour
{
    public void TriggerTrapMode(int range)
    {
        GameEvents.OnRequestTrapSelection?.Invoke(range);
    }
}
