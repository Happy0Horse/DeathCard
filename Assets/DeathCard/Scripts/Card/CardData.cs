using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/CardData")]
public class CardData : ItemData
{
    public enum CardCategory { Move, Attack, Trap, Utility }
    public CardCategory category;
    public PlayerAnimation.AttackMode attackMode;
    public PlayerAnimation.UtilityType utilityType;
    public PlayerAnimation.TrapType trapType;

    public int range;
    public int damage; 
    public int effectiveRange;
    public float effectDuration;
    public bool isMultiHit;

    [Header("UI Display")]
    public Sprite categoryIcon;
    public Sprite artSprite;
}
