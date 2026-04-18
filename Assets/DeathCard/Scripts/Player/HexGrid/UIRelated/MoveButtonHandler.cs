using UnityEngine;

public class MoveButtonHandler : MonoBehaviour
{
    public int range = 3;

    public void TriggerMoveSelection()
    {
        Debug.Log($"[MoveButtonHandler] Button clicked. Range: {range}");
        GameEvents.OnRequestMoveSelection?.Invoke(range);
    }
}