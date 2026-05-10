using System.Collections;
using UnityEngine;
using Mirror;

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

    private void OnEnable() => NetworkClient.RegisterHandler<EndgameStartedMessage>(OnEndgameStarted);
    private void OnDisable() => NetworkClient.UnregisterHandler<EndgameStartedMessage>();

    private void OnEndgameStarted(EndgameStartedMessage msg)
    {
        _endgameActive = true;
        StartCoroutine(EndgameMovementLoop());
    }

    private void TriggerWin()
    {
        _hasWon = true;
        _endgameActive = false;
        _navigator.ClearSelectionState();

        if (winCanvas != null)
            winCanvas.SetActive(true);

        // Сообщаем серверу что игра окончена
        NetworkClient.Send(new GameOverMessage());
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
}