using UnityEngine;
using UnityEngine.UI;

public class MazeSlot : MonoBehaviour
{
    public enum SkillType { BreakFences, Crouch, UnlockDoor }

    public Image icon;
    public SkillType skillType;

    void Update()
    {
        if(PlayerPrefs.GetInt("BreakFences") == 1 && skillType == SkillType.BreakFences)
        {
            icon.color = Color.limeGreen;
        }
        else if (PlayerPrefs.GetInt("UnlockDoor") == 1 && skillType == SkillType.UnlockDoor)
        {
            icon.color = Color.limeGreen;
        }
        else if (PlayerPrefs.GetInt("Crouch") == 1 && skillType == SkillType.Crouch)
        {
            icon.color = Color.limeGreen;
        }
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetInt("BreakFences", 0);
        PlayerPrefs.SetInt("UnlockDoor", 0);
        PlayerPrefs.SetInt("Crouch", 0);
    }
}
