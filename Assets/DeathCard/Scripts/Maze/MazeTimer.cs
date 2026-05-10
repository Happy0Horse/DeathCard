using UnityEngine;
using TMPro;

public class MazeTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    public void UpdateTimer(float timeRemaining)
    {
        if (timerText != null)
            timerText.text = $"Time left: {Mathf.CeilToInt(Mathf.Max(0, timeRemaining))}";
    }
}