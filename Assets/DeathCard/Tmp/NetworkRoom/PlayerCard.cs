using UnityEngine;
using TMPro;

public class PlayerCard : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statusText;

    public void SetEmpty()
    {
        nameText.text = "Empty";
        statusText.text = "Waiting...";
        statusText.color = Color.gray;
    }

    public void Setup(string playerName, bool isReady)
    {
        nameText.text = playerName;
        statusText.text = isReady ? "Ready" : "NotReady";
        statusText.color = isReady ? Color.green : Color.red;
    }
}