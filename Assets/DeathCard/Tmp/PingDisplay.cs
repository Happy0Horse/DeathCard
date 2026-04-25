using Mirror;
using TMPro;
using UnityEngine;

public class PingDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pingText;
    [SerializeField] private float updateRate = 0.5f;
    
    private float lastUpdate;
    
    void Update()
    {
        if (Time.time - lastUpdate < updateRate) return;
        lastUpdate = Time.time;
        
        if (pingText == null) return;
        
        if (NetworkClient.isConnected && NetworkTime.rtt > 0)
        {
            // Приводим double к float
            int ping = Mathf.RoundToInt((float)NetworkTime.rtt * 1000f);
            pingText.text = $"{ping} ms";
            pingText.color = ping < 100 ? Color.green : (ping < 200 ? Color.yellow : Color.red);
        }
        else
        {
            pingText.text = "... ms";
        }
    }
}