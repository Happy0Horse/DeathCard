using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;
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

    private HexViewManager _viewManager;
    private bool _isOvertime;

    private void Awake()
    {
        _viewManager = FindFirstObjectByType<HexViewManager>();
    }

    private void OnEnable()
    {
        NetworkClient.RegisterHandler<TimerUpdateMessage>(OnTimerUpdate);
        NetworkClient.RegisterHandler<StartWaitMessage>(OnStartWait);
        NetworkClient.RegisterHandler<GameStartedMessage>(OnGameStarted);
        NetworkClient.RegisterHandler<RoundOverMessage>(OnRoundOver);
        NetworkClient.RegisterHandler<DeadlineReachedMessage>(OnDeadlineReached);

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (fpViewButton != null)
            fpViewButton.onClick.AddListener(OnFPButtonClicked);
    }

    private void OnDisable()
    {
        NetworkClient.UnregisterHandler<TimerUpdateMessage>();
        NetworkClient.UnregisterHandler<StartWaitMessage>();
        NetworkClient.UnregisterHandler<GameStartedMessage>();
        NetworkClient.UnregisterHandler<RoundOverMessage>();
        NetworkClient.UnregisterHandler<DeadlineReachedMessage>();
    }

    private void Start()
    {
        waitingPanel.SetActive(true);
        activeTimerPanel.SetActive(false);
        roundEndPanel.SetActive(false);
    }

    private void OnStartButtonClicked()
    {
        if (_viewManager != null) _viewManager.ExitFirstPerson();
        NetworkClient.Send(new ManualStartMessage());
    }

    private void OnFPButtonClicked()
    {
        if (_viewManager != null) _viewManager.EnterFirstPerson();
    }

    private void OnGameStarted(GameStartedMessage msg)
    {
        waitingPanel.SetActive(false);
        roundEndPanel.SetActive(false);
        activeTimerPanel.SetActive(true);
        _isOvertime = false;
        if (_viewManager != null) _viewManager.ExitFirstPerson();
    }

    private void OnDeadlineReached(DeadlineReachedMessage msg)
    {
        _isOvertime = true;
        if (timerText != null)
            timerText.text = "Overtime. Everyone takes damage overtime";
    }

    private void OnRoundOver(RoundOverMessage msg)
    {
        activeTimerPanel.SetActive(false);
        waitingPanel.SetActive(false);
        roundEndPanel.SetActive(true);
        _isOvertime = false;

        if (roundStatusText != null)
            roundStatusText.text = $"DOME {msg.round} HAS FALLEN";

        if (roundCountdownText != null)
            roundCountdownText.text = $"Next round in: {Mathf.CeilToInt(msg.transitionTime)}s";
    }

    private void OnStartWait(StartWaitMessage msg)
    {
        if (autoStartText != null)
            autoStartText.text = $"Auto-starting in: {Mathf.CeilToInt(msg.timeUntilStart)}s";
    }

    private void OnTimerUpdate(TimerUpdateMessage msg)
    {
        if (!_isOvertime && timerText != null)
        {
            TimeSpan time = TimeSpan.FromSeconds(msg.timeRemaining);
            timerText.text = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
        }

        if (nextDistributionText != null)
            nextDistributionText.text = $"Until next card distribution: {Mathf.CeilToInt(msg.nextDistribution)}";

        if (cardAmountText != null)
            cardAmountText.text = $"Card amount per distribution: {msg.cardsPerInterval}";
    }
}