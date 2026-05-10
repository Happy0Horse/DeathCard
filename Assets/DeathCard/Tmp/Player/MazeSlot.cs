using UnityEngine;
using UnityEngine.UI;

public class MazeSlot : MonoBehaviour
{
    public enum SkillType { BreakFences, Crouch, UnlockDoor }

    public Image icon;
    public SkillType skillType;

    void Update()
    {
        if( LocalStorage.Instance.breakFences && skillType == SkillType.BreakFences)
        {
            icon.color = Color.limeGreen;
        }
        else if (LocalStorage.Instance.unlockDoor && skillType == SkillType.UnlockDoor)
        {
            icon.color = Color.limeGreen;
        }
        else if (LocalStorage.Instance.canCrouch && skillType == SkillType.Crouch)
        {
            icon.color = Color.limeGreen;
        }
    }
}
