using UnityEngine;

public class UtilityButtonHandler : MonoBehaviour
{
    public void TriggerUtility()
    {
        GameEvents.OnRequestUtilityAction?.Invoke();
    }
}
