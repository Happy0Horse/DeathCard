using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class DebuffSystem : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public Transform playerModel;

    public float debuffDuration = 60f;

    public GameObject debuffSlotPrefab;
    public Transform debuffContainer;

    public Sprite invertIcon;
    public Sprite smallIcon;
    public Sprite slowIcon;

    float baseSpeed;
    float baseSprintSpeed;

    List<ActiveDebuff> activeDebuffs = new List<ActiveDebuff>();

    public enum DebuffType
    {
        InvertMovement,
        SmallPlayer,
        SlowMovement
    }

    class ActiveDebuff
    {
        public DebuffType type;
        public float timeLeft;

        public GameObject uiObject;
        public Image icon;
        public Text timerText;
    }

    void Start()
    {
        baseSpeed = playerMovement.speed;
        baseSprintSpeed = playerMovement.sprintSpeed;
    }

    void Update()
    {
        for (int i = activeDebuffs.Count - 1; i >= 0; i--)
        {
            var debuff = activeDebuffs[i];

            debuff.timeLeft -= Time.deltaTime;

            int minutes = Mathf.FloorToInt(debuff.timeLeft / 60f);
            int seconds = Mathf.FloorToInt(debuff.timeLeft % 60f);
            debuff.timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            if (debuff.timeLeft <= 0)
            {
                RemoveDebuff(debuff);
                activeDebuffs.RemoveAt(i);
            }
        }

        RecalculateEffects();
    }

    public void ApplyRandomDebuff()
    {
        DebuffType debuff = (DebuffType)Random.Range(0, System.Enum.GetValues(typeof(DebuffType)).Length);
        AddDebuff(debuff);
    }

    void AddDebuff(DebuffType type)
    {
        ActiveDebuff debuff = new ActiveDebuff();
        debuff.type = type;
        debuff.timeLeft = GetTimeForDebuff(type);

        GameObject obj = Instantiate(debuffSlotPrefab, debuffContainer);
        obj.SetActive(true);
        debuff.uiObject = obj;
        debuff.icon = obj.transform.Find("Icon").GetComponent<Image>();
        debuff.timerText = obj.transform.Find("TimerText").GetComponent<Text>();

        switch (type)
        {
            case DebuffType.InvertMovement:
                debuff.icon.sprite = invertIcon;
                break;
            case DebuffType.SmallPlayer:
                debuff.icon.sprite = smallIcon;
                break;
            case DebuffType.SlowMovement:
                debuff.icon.sprite = slowIcon;
                break;
        }

        activeDebuffs.Add(debuff);
    }

    void RemoveDebuff(ActiveDebuff debuff)
    {
        Destroy(debuff.uiObject);
    }

    void RecalculateEffects()
    {
        playerMovement.invertMovement = false;
        playerModel.localScale = Vector3.one;

        float speedMultiplier = 1f;
        float sprintMultiplier = 1f;

        foreach (var debuff in activeDebuffs)
        {
            switch (debuff.type)
            {
                case DebuffType.InvertMovement:
                    playerMovement.invertMovement = true;
                    break;

                case DebuffType.SmallPlayer:
                    playerModel.localScale = Vector3.one * 0.5f;
                    break;

                case DebuffType.SlowMovement:
                    speedMultiplier *= 0.5f;
                    sprintMultiplier *= 0.5f;
                    break;
            }
        }

        playerMovement.speed = baseSpeed * speedMultiplier;
        playerMovement.sprintSpeed = baseSprintSpeed * sprintMultiplier;
    }

    float GetTimeForDebuff(DebuffType type)
    {
        switch (type)
        {
            case DebuffType.InvertMovement:
                return 10f;

            case DebuffType.SmallPlayer:
                return 20f;

            case DebuffType.SlowMovement:
                return 30f;
            default:
                return 40f;
        }
    }
}