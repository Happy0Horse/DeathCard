using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Убираем static Instance
    public enum GameState { Maze, Game, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Maze;

    [Header("Scene Names")]
    public string mazeScene = "Maze_Scene";
    public string gameScene = "Game_Scene";

    [Header("Global Timing")]
    public float deadlineDuration = 121f;
    public float cardDistributionInterval = 10f;
    public float preGameWaitDuration = 60f;
    public float mazeTimerDuration = 60f;
    public int cardsPerInterval = 3;

    [Header("Overtime Settings")]
    public float overtimeDamagePerSecond = 5f;

    [Header("Round Transition")]
    public float roundTransitionDelay = 5f;
    public int maxRounds = 3;
    public int CurrentRound { get; private set; } = 0;

    private float _timeRemaining;
    private float _cardTimer;
    private float _startWaitTimer;
    private float _overtimeTickTimer;
    private float _roundTransitionTimer;
    private bool _gameStarted;
    private bool _firstDistributionDone;
    private bool _timersFrozen;
    private bool _isOvertime;
    private bool _isRoundTransition;
    private float _mazeTimer;
    private bool _mazeActive = false;
    private float _mazeTimerSendInterval = 0f;
    private float _gameStartDelay = 5f;
    private bool _clientsReady = false;
    private string _roomId;

    // Убираем все static events — они не нужны на сервере
    public static event Action<int> OnDistributeCards;
    public static event Action OnEndgameStarted;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable() => GlobalEvents.OnDomeBroken += HandleDomeBroken;
    private void OnDisable() => GlobalEvents.OnDomeBroken -= HandleDomeBroken;

    public void Initialize(string roomId)
    {
        Debug.Log($"[GameManager] Initialize roomId={roomId}");
        _roomId = roomId;
        CurrentRound = 0;
        EnterMaze();
    }

    private void Update()
    {
        if (CurrentState == GameState.Maze && _mazeActive)
        {
            _mazeTimer -= Time.deltaTime;

            _mazeTimerSendInterval -= Time.deltaTime;
            if (_mazeTimerSendInterval <= 0)
            {
                _mazeTimerSendInterval = 1f;
                RoomManager.instance.SendToRoom(_roomId, new MazeTimerMessage { timeRemaining = _mazeTimer });
            }

            if (_mazeTimer <= 0)
            {
                _mazeActive = false;
                EnterGame();
            }
            return;
        }

        if (CurrentState != GameState.Game) return;
        if (!_clientsReady) return;

        if (_isRoundTransition)
        {
            _roundTransitionTimer -= Time.deltaTime;
            RoomManager.instance.SendToRoom(_roomId, new RoundOverMessage
            {
                round = CurrentRound,
                transitionTime = _roundTransitionTimer
            });
            if (_roundTransitionTimer <= 0)
            {
                _isRoundTransition = false;
                EnterMaze();
            }
            return;
        }

        if (!_gameStarted)
        {
            _startWaitTimer -= Time.deltaTime;
            RoomManager.instance.SendToRoom(_roomId, new StartWaitMessage { timeUntilStart = _startWaitTimer });
            if (_startWaitTimer <= 0) StartGame();
            return;
        }

        if (_timersFrozen) return;

        HandleCardDistribution();

        if (_isOvertime)
            HandleOvertime();
        else
            RunStandardTimers();
    }

    public void EnterMaze()
    {
        Debug.Log($"[GameManager] EnterMaze, mazeTimerDuration={mazeTimerDuration}");
        CurrentState = GameState.Maze;
        _mazeTimer = mazeTimerDuration;
        _mazeActive = true;
        RoomManager.instance.ChangeRoomScene(_roomId, mazeScene);
        NotifyClients(0);
    }

    public void EnterGame()
    {
        CurrentState = GameState.Game;
        ResetRoundState();
        _clientsReady = false;
        RoomManager.instance.ChangeRoomScene(_roomId, gameScene);
        NotifyClients(1);
        StartCoroutine(WaitForClientsReady());
    }

    public void EnterGameOver()
    {
        CurrentState = GameState.GameOver;
        Debug.Log($"[GameManager] Игра окончена! Комната {_roomId}");
        NotifyClients(2);
    }

    IEnumerator WaitForClientsReady()
    {
        yield return new WaitForSeconds(_gameStartDelay);
        _clientsReady = true;
    }

    private void NotifyClients(int state)
    {
        RoomManager.instance.SendToRoom(_roomId, new GameStateMessage
        {
            state = state,
            round = CurrentRound,
            sceneName = state == 0 ? mazeScene : state == 1 ? gameScene : ""
        });
    }

    private void ResetRoundState()
    {
        _timeRemaining = deadlineDuration;
        _cardTimer = cardDistributionInterval;
        _startWaitTimer = preGameWaitDuration;
        _overtimeTickTimer = 0f;
        _roundTransitionTimer = 0f;
        _gameStarted = false;
        _firstDistributionDone = false;
        _timersFrozen = false;
        _isOvertime = false;
        _isRoundTransition = false;
    }

    private void HandleCardDistribution()
    {
        if (!_firstDistributionDone)
        {
            _firstDistributionDone = true;
            RoomManager.instance.SendToRoom(_roomId, new DistributeCardsMessage { count = cardsPerInterval });
        }

        _cardTimer -= Time.deltaTime;
        if (_cardTimer <= 0)
        {
            _cardTimer = cardDistributionInterval;
            RoomManager.instance.SendToRoom(_roomId, new DistributeCardsMessage { count = cardsPerInterval });
        }
    }

    private void RunStandardTimers()
    {
        _timeRemaining -= Time.deltaTime;
        if (_timeRemaining <= 0)
        {
            _timeRemaining = 0;
            _isOvertime = true;
            RoomManager.instance.SendToRoom(_roomId, new DeadlineReachedMessage());
        }

        RoomManager.instance.SendToRoom(_roomId, new TimerUpdateMessage
        {
            timeRemaining = _timeRemaining,
            nextDistribution = _cardTimer,
            cardsPerInterval = cardsPerInterval
        });
    }

    private void HandleOvertime()
    {
        _overtimeTickTimer += Time.deltaTime;
        if (_overtimeTickTimer >= 1f)
        {
            _overtimeTickTimer = 0f;
            RoomManager.instance.SendToRoom(_roomId, new OvertimeTickMessage
            {
                damage = overtimeDamagePerSecond
            });
        }
    }

    private void HandleDomeBroken()
    {
        CurrentRound++;

        if (CurrentRound >= maxRounds)
        {
            RoomManager.instance.SendToRoom(_roomId, new RoundOverMessage { round = CurrentRound, transitionTime = 0 });
            RoomManager.instance.SendToRoom(_roomId, new EndgameStartedMessage()); // вместо OnEndgameStarted?.Invoke()
            EnterGameOver();
            return;
        }

        _isRoundTransition = true;
        _isOvertime = false;
        _roundTransitionTimer = roundTransitionDelay;
        RoomManager.instance.SendToRoom(_roomId, new RoundOverMessage
        {
            round = CurrentRound,
            transitionTime = roundTransitionDelay
        });
    }

    public void TriggerDomeBroken()
    {
        HandleDomeBroken();
    }

    public void StartGame()
    {
        if (_gameStarted) return;
        _gameStarted = true;
        RoomManager.instance.SendToRoom(_roomId, new GameStartedMessage());
    }

    public void SetTimerFreeze(bool freeze) => _timersFrozen = freeze;
    public float GetTimeUntilNextDistribution() => _cardTimer;
    public int GetCardsPerInterval() => cardsPerInterval;
    public int GetCurrentRound() => CurrentRound;
    public float GetMazeTimerDuration() => mazeTimerDuration;
}