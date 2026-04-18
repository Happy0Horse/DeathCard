using UnityEngine;
using TMPro;

public class PlayerCard : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statusText;

    public void SetEmpty()
    {
        nameText.text = "Пусто";
        statusText.text = "Ожидание...";
        statusText.color = Color.gray;
    }

    public void Setup(string playerName, bool isReady)
    {
        nameText.text = playerName;
        statusText.text = isReady ? "Готов" : "Не готов";
        statusText.color = isReady ? Color.green : Color.red;
    }
}