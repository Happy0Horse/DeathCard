using UnityEngine;

public class PlayerActionHandler : MonoBehaviour
{
    private HexGridNavigator _navigator;
    public Material placedTrapMaterial;
    private string _currentMode = "";

    private void Awake() => _navigator = GetComponent<HexGridNavigator>();

    private void OnEnable()
    {
        GameEvents.OnRequestMoveSelection += range => ToggleSelection(range, "Move", ExecuteMove);
        GameEvents.OnRequestTrapSelection += range => ToggleSelection(range, "Trap", ExecuteTrap);
        GameEvents.OnRequestUtilityAction += ExecuteUtility;
        GameEvents.OnCancelCurrentAction += ResetMode;
    }

    private void OnDisable()
    {
        GameEvents.OnRequestMoveSelection -= range => ToggleSelection(range, "Move", ExecuteMove);
        GameEvents.OnRequestTrapSelection -= range => ToggleSelection(range, "Trap", ExecuteTrap);
        GameEvents.OnRequestUtilityAction -= ExecuteUtility;
        GameEvents.OnCancelCurrentAction -= ResetMode;
    }

    private void ToggleSelection(int range, string mode, System.Action<HexCell> callback)
    {
        if (_currentMode == mode)
        {
            GameEvents.OnCancelCurrentAction?.Invoke();
        }
        else
        {
            _currentMode = mode;
            _navigator.BeginSelection(range, callback);
        }
    }

    private void ResetMode() => _currentMode = "";

    private void ExecuteMove(HexCell target)
    {
        _navigator.MoveTo(target);
        ResetMode();
    }

    private void ExecuteTrap(HexCell target)
    {
        _navigator.ClearSelectionState();

        Renderer rend = target.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material = placedTrapMaterial;
        }

        ResetMode();
    }

    private void ExecuteUtility() => Debug.Log("Utility Action Triggered");
}