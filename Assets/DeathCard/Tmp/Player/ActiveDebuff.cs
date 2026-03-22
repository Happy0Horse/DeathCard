using UnityEngine;
using UnityEngine.UI;
using static DebuffSystem;

class ActiveDebuff
{
    public DebuffType type;
    public float timeLeft;

    public GameObject uiObject;  
    public Image icon;           
    public TMPro.TMP_Text timerText; 
}