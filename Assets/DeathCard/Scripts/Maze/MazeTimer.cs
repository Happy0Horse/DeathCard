using UnityEngine;
using TMPro;

public class MazeTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    private float _timeRemaining;

    private void Start()
    {
        _timeRemaining = GameManager.Instance.GetMazeTimerDuration();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Maze) return;

        _timeRemaining -= Time.deltaTime;
        if (timerText != null)
            timerText.text = $"Time left: {Mathf.CeilToInt(Mathf.Max(0, _timeRemaining))}";

        if (_timeRemaining <= 0)
        {
            _timeRemaining = 0;
            enabled = false;
            GameManager.Instance.EnterGame();
        }
    }
}