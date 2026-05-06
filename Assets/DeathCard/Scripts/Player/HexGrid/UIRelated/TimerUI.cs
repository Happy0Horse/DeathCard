using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TimerUI : MonoBehaviour
{
    [Header("State Panels")]
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private GameObject activeTimerPanel;
    [SerializeField] private GameObject roundEndPanel;

    [Header("Waiting Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button fpViewButton;
    [SerializeField] private TextMeshProUGUI autoStartText;

    [Header("Active Elements")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI nextDistributionText;
    [SerializeField] private TextMeshProUGUI cardAmountText;

    [Header("Round End Elements")]
    [SerializeField] private TextMeshProUGUI roundStatusText;
    [SerializeField] private TextMeshProUGUI roundCountdownText;
    [SerializeField] private float transitionDelay = 5f;

    private HexViewManager _viewManager;
    private float _transitionTimer;
    private bool _isTransitioning;
    private bool _isOvertime;

    private void Awake()
    {
        _viewManager = FindFirstObjectByType<HexViewManager>();
    }

    private void OnEnable()
    {
        GameManager.OnTimerUpdated += UpdateActiveDisplay;
        GameManager.OnStartWaitUpdated += UpdateWaitingDisplay;
        GameManager.OnGameStarted += ShowActiveTimer;
        GameManager.OnRoundOver += ShowRoundEnd;
        GameManager.OnDeadlineReached += EnableOvertimeDisplay;

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (fpViewButton != null)
            fpViewButton.onClick.AddListener(OnFPButtonClicked);
    }

    private void OnDisable()
    {
        GameManager.OnTimerUpdated -= UpdateActiveDisplay;
        GameManager.OnStartWaitUpdated -= UpdateWaitingDisplay;
        GameManager.OnGameStarted -= ShowActiveTimer;
        GameManager.OnRoundOver -= ShowRoundEnd;
        GameManager.OnDeadlineReached -= EnableOvertimeDisplay;
    }

    private void Start()
    {
        waitingPanel.SetActive(true);
        activeTimerPanel.SetActive(false);
        roundEndPanel.SetActive(false);
    }

    private void Update()
    {
        if (!_isTransitioning) return;

        _transitionTimer -= Time.deltaTime;
        if (roundCountdownText != null)
        {
            roundCountdownText.text = $"Next round in: {Mathf.CeilToInt(_transitionTimer)}s";
        }

        if (_transitionTimer <= 0)
        {
            _isTransitioning = false;
            Debug.Log("Initiating Scene Transition...");
        }
    }

    private void OnStartButtonClicked()
    {
        if (_viewManager != null) _viewManager.ExitFirstPerson();
        GameManager.Instance.StartGame();
    }

    private void OnFPButtonClicked()
    {
        if (_viewManager != null) _viewManager.EnterFirstPerson();
    }

    private void ShowActiveTimer()
    {
        waitingPanel.SetActive(false);
        roundEndPanel.SetActive(false);
        activeTimerPanel.SetActive(true);
        _isOvertime = false;
        if (_viewManager != null) _viewManager.ExitFirstPerson();
    }

    private void EnableOvertimeDisplay()
    {
        _isOvertime = true;
        if (timerText != null)
        {
            timerText.text = "Overtime. Everyone takes damage overtime";
        }
    }

    private void ShowRoundEnd(int domeIndex)
    {
        activeTimerPanel.SetActive(false);
        waitingPanel.SetActive(false);
        roundEndPanel.SetActive(true);
        _isOvertime = false;

        if (roundStatusText != null)
        {
            roundStatusText.text = $"DOME {domeIndex} HAS FALLEN";
        }

        _transitionTimer = transitionDelay;
        _isTransitioning = true;
    }

    private void UpdateWaitingDisplay(float timeUntilStart)
    {
        if (autoStartText != null)
        {
            autoStartText.text = $"Auto-starting in: {Mathf.CeilToInt(timeUntilStart)}s";
        }
    }

    private void UpdateActiveDisplay(float timeRemaining)
    {
        if (GameManager.Instance == null) return;

        if (!_isOvertime && timerText != null)
        {
            TimeSpan time = TimeSpan.FromSeconds(timeRemaining);
            timerText.text = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
        }

        if (nextDistributionText != null)
        {
            float nextDist = GameManager.Instance.GetTimeUntilNextDistribution();
            nextDistributionText.text = $"Until next card distribution: {Mathf.CeilToInt(nextDist)}";
        }

        if (cardAmountText != null)
        {
            cardAmountText.text = $"Card amount per distribution: {GameManager.Instance.GetCardsPerInterval()}";
        }
    }
}