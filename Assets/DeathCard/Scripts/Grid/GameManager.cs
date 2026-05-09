using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Maze, Game, GameOver }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "Main_Menu";
    [SerializeField] private string mazeScene = "Maze_Scene";
    [SerializeField] private string gameScene = "Game_Scene";

    [Header("Global Timing")]
    [SerializeField] private float deadlineDuration = 300f;
    [SerializeField] private float cardDistributionInterval = 10f;
    [SerializeField] private float preGameWaitDuration = 10f;
    [SerializeField] private float mazeTimerDuration = 60f;
    [SerializeField] private int cardsPerInterval = 3;

    [Header("Overtime Settings")]
    [SerializeField] private float overtimeDamagePerSecond = 5f;

    [Header("Round Transition")]
    [SerializeField] private float roundTransitionDelay = 5f;

    public int CurrentRound { get; private set; } = 0;

    private float _timeRemaining;
    private float _cardTimer;
    private float _startWaitTimer;
    private float _overtimeTickTimer;
    private float _roundTransitionTimer;
    private bool _gameActive;
    private bool _gameStarted;
    private bool _firstDistributionDone;
    private bool _timersFrozen;
    private bool _isOvertime;
    private bool _isRoundTransition;

    public static event Action<float> OnTimerUpdated;
    public static event Action<float> OnStartWaitUpdated;
    public static event Action OnDeadlineReached;
    public static event Action<int> OnDistributeCards;
    public static event Action OnGameStarted;
    public static event Action<int> OnRoundOver;
    public static event Action<float> OnOvertimeTick;
    public static event Action<float> OnRoundTransitionTick;
    public static event Action OnEndgameStarted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable() => GlobalEvents.OnDomeBroken += HandleDomeBroken;
    private void OnDisable() => GlobalEvents.OnDomeBroken -= HandleDomeBroken;

    private void Update()
    {
        if (CurrentState != GameState.Game) return;

        if (_isRoundTransition)
        {
            _roundTransitionTimer -= Time.deltaTime;
            OnRoundTransitionTick?.Invoke(_roundTransitionTimer);
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
            OnStartWaitUpdated?.Invoke(_startWaitTimer);
            if (_startWaitTimer <= 0) StartGame();
            return;
        }

        if (_timersFrozen) return;

        HandleCardDistribution();

        if (_isOvertime)
        {
            HandleOvertime();
            OnTimerUpdated?.Invoke(_timeRemaining);
        }
        else
        {
            RunStandardTimers();
        }
    }

    public void EnterMaze()
    {
        CurrentState = GameState.Maze;
        SceneManager.LoadScene(mazeScene);
    }

    public void EnterGame()
    {
        CurrentState = GameState.Game;
        ResetRoundState();
        SceneManager.LoadScene(gameScene);
    }

    public void EnterGameOver()
    {
        CurrentState = GameState.GameOver;
    }

    private void ResetRoundState()
    {
        _timeRemaining = deadlineDuration;
        _cardTimer = cardDistributionInterval;
        _startWaitTimer = preGameWaitDuration;
        _overtimeTickTimer = 0f;
        _roundTransitionTimer = 0f;
        _gameActive = true;
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
            OnDistributeCards?.Invoke(cardsPerInterval);
        }

        _cardTimer -= Time.deltaTime;
        if (_cardTimer <= 0)
        {
            _cardTimer = cardDistributionInterval;
            OnDistributeCards?.Invoke(cardsPerInterval);
        }
    }

    private void RunStandardTimers()
    {
        _timeRemaining -= Time.deltaTime;
        if (_timeRemaining <= 0)
        {
            _timeRemaining = 0;
            _isOvertime = true;
            OnDeadlineReached?.Invoke();
        }
        OnTimerUpdated?.Invoke(_timeRemaining);
    }

    private void HandleOvertime()
    {
        _overtimeTickTimer += Time.deltaTime;
        if (_overtimeTickTimer >= 1f)
        {
            _overtimeTickTimer = 0f;
            OnOvertimeTick?.Invoke(overtimeDamagePerSecond);
        }
    }

    private void HandleDomeBroken()
    {
        CurrentRound++;
        _isRoundTransition = true;
        _isOvertime = false;
        _roundTransitionTimer = roundTransitionDelay;

        if (CurrentRound >= 3)
        {
            OnRoundOver?.Invoke(CurrentRound);
            OnEndgameStarted?.Invoke();
            EnterGameOver();
            return;
        }

        OnRoundOver?.Invoke(CurrentRound);
    }

    public void StartGame()
    {
        if (_gameStarted) return;
        _gameStarted = true;
        OnGameStarted?.Invoke();
    }

    public void SetTimerFreeze(bool freeze) => _timersFrozen = freeze;
    public float GetTimeUntilNextDistribution() => _cardTimer;
    public int GetCardsPerInterval() => cardsPerInterval;
    public int GetCurrentRound() => CurrentRound;
    public float GetMazeTimerDuration() => mazeTimerDuration;
}