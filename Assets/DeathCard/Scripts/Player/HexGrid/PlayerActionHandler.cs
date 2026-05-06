using System.Collections;
using UnityEngine;

public class PlayerActionHandler : MonoBehaviour
{
    private HexGridNavigator _navigator;
    private Animator _animator;
    private string _currentMode = "";
    private CardUtillityAction _utilityAction;
    private CardTrapAction _trapAction;
    private System.Action _onFinishCurrentAction;

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
        GameEvents.OnRequestMoveSelection += (range, callback) => {
            _onFinishCurrentAction = callback;
            ToggleSelection(range, "Move", ExecuteMove);
        };
        GameEvents.OnRequestTrapSelection += (data, callback) => {
            _onFinishCurrentAction = callback;
            RequestTrap(data);
        };
        GameEvents.OnRequestUtilityAction += (data, callback) => {
            ExecuteUtility(data);
            callback?.Invoke();
        };
        GameEvents.OnCancelCurrentAction += ResetMode;
    }

    private void OnDisable()
    {
        GameEvents.OnRequestMoveSelection -= (range, callback) => { };
        GameEvents.OnRequestTrapSelection -= (data, callback) => { };
        GameEvents.OnRequestUtilityAction -= (data, callback) => { };
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

    private void ResetMode()
    {
        _currentMode = "";
        _onFinishCurrentAction = null;
    }

    private void ExecuteMove(HexCell target)
    {
        _navigator.MoveTo(target);
        _onFinishCurrentAction?.Invoke();
        ResetMode();
    }

    private void ExecuteTrap(HexCell target)
    {
        _navigator.ClearSelectionState();
        if (_trapAction != null) _trapAction.Execute(target);
        _onFinishCurrentAction?.Invoke();
        ResetMode();
    }

    private void ExecuteUtility(CardData data)
    {
        if (_animator != null) _animator.SetTrigger("UtilityTrigger");

        if (_utilityAction != null)
        {
            _navigator.ClearSelectionState();
            _utilityAction.Execute(data);
        }
    }
}