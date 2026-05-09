using System.Collections;
using UnityEngine;

public class EndgameHandler : MonoBehaviour
{
    [SerializeField] private int moveRange = 3;
    [SerializeField] private GameObject winCanvas;

    private HexGridNavigator _navigator;
    private bool _endgameActive = false;
    private bool _hasWon = false;
    private bool _internalMoving = false;

    private void Awake()
    {
        _navigator = GetComponent<HexGridNavigator>();
    }

    private void OnEnable() => GameManager.OnEndgameStarted += StartEndgame;
    private void OnDisable() => GameManager.OnEndgameStarted -= StartEndgame;

    private void StartEndgame()
    {
        _endgameActive = true;
        StartCoroutine(EndgameMovementLoop());
    }

    private IEnumerator EndgameMovementLoop()
    {
        while (_endgameActive && !_hasWon)
        {
            if (_internalMoving || _navigator.IsMoving)
            {
                yield return null;
                continue;
            }

            bool selectionResolved = false;

            _navigator.BeginSelection(moveRange, (targetCell) =>
            {
                if (targetCell != null)
                    StartCoroutine(HandleMove(targetCell));
                selectionResolved = true;
            });

            while (!selectionResolved)
                yield return null;
        }
    }

    private IEnumerator HandleMove(HexCell target)
    {
        _internalMoving = true;
        _navigator.MoveTo(target);

        yield return new WaitUntil(() => !_navigator.IsMoving);

        _internalMoving = false;

        if (_navigator.CurrentCoordinates == Vector2Int.zero)
            TriggerWin();
    }

    private void TriggerWin()
    {
        _hasWon = true;
        _endgameActive = false;
        _navigator.ClearSelectionState();

        if (winCanvas != null)
            winCanvas.SetActive(true);

        GameManager.Instance.EnterGameOver();
    }
}