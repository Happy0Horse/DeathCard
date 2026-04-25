using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/CardData")]
public class CardData : ItemData
{
    public enum CardCategory { Move, Attack, Trap, Utility }
    public CardCategory category;
    public PlayerAnimation.AttackMode attackMode;

    public int range;
    public int damage;
}
