using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Global Timing")]
    [SerializeField] private float deadlineDuration = 300f;
    [SerializeField] private float cardDistributionInterval = 10f;
    [SerializeField] private float preGameWaitDuration = 10f;
    [SerializeField] private int cardsPerInterval = 3;

    [Header("Overtime Settings")]
    [SerializeField] private float overtimeDamagePerSecond = 5f;
    private float _overtimeTickTimer;
    private bool _isOvertime = false;

    [Header("Dome Tracking")]
    private int _currentDomeIndex = 0;
    private bool _isRoundTransition = false;

    private float _timeRemaining;
    private float _cardTimer;
    private float _startWaitTimer;
    private bool _gameActive;
    private bool _gameStarted;
    private bool _firstDistributionDone;
    private bool _timersFrozen;

    public static event Action<float> OnTimerUpdated;
    public static event Action<float> OnStartWaitUpdated;
    public static event Action OnDeadlineReached;
    public static event Action<int> OnDistributeCards;
    public static event Action OnGameStarted;
    public static event Action<int> OnRoundOver;

    public static event Action<float> OnOvertimeTick;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable() => GlobalEvents.OnDomeBroken += HandleDomeBroken;
    private void OnDisable() => GlobalEvents.OnDomeBroken -= HandleDomeBroken;

    private void Start()
    {
        _timeRemaining = deadlineDuration;
        _cardTimer = cardDistributionInterval;
        _startWaitTimer = preGameWaitDuration;
        _gameActive = true;
    }

    private void Update()
    {
        if (!_gameActive || _isRoundTransition) return;

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
        _currentDomeIndex++;
        _isRoundTransition = true;
        _isOvertime = false;
        OnRoundOver?.Invoke(_currentDomeIndex);
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
    public int GetCurrentDomeIndex() => _currentDomeIndex;
}