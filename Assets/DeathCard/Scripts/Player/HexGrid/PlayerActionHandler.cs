using System.Collections;
using UnityEngine;

public class PlayerActionHandler : MonoBehaviour
{
    private HexGridNavigator _navigator;
    private Animator _animator;
    private string _currentMode = "";
    private CardUtillityAction _utilityAction;
    private CardTrapAction _trapAction;

    public Material placedTrapMaterial;


    private void Awake()
    {
        _navigator = GetComponent<HexGridNavigator>();
        _animator = GetComponent<Animator>();
        _utilityAction = GetComponent<CardUtillityAction>();
        _trapAction = GetComponent<CardTrapAction>();
    }

    private void OnEnable()
    {
        GameEvents.OnRequestMoveSelection += range => ToggleSelection(range, "Move", ExecuteMove);
        GameEvents.OnRequestTrapSelection += RequestTrap;
        GameEvents.OnRequestUtilityAction += ExecuteUtility;
        GameEvents.OnCancelCurrentAction += ResetMode;
    }

    private void OnDisable()
    {
        GameEvents.OnRequestMoveSelection -= range => ToggleSelection(range, "Move", ExecuteMove);
        GameEvents.OnRequestTrapSelection -= RequestTrap;
        GameEvents.OnRequestUtilityAction -= ExecuteUtility;
        GameEvents.OnCancelCurrentAction -= ResetMode;
    }

    private void RequestTrap(CardData data)
    {
        if (_trapAction != null)
        {
            _trapAction.RequestTrap(data);
            ToggleSelection(data.range, "Trap", ExecuteTrap);
        }
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
        // Modify logic here as needed
        _navigator.ClearSelectionState();
        if (_trapAction != null) _trapAction.Execute(target);
        ResetMode();
    }

    private void ExecuteUtility(CardData data)
    {
        // Modify logic here as needed
        if (_animator != null) _animator.SetTrigger("UtilityTrigger");

        if (_utilityAction != null)
        {
            _utilityAction.Execute(data);
        }
    }
}