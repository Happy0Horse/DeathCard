using UnityEngine;

public class CardAction : MonoBehaviour
{
    public CardData data;
    private DebuffSystem _debuffs;

    private void Start()
    {
        _debuffs = GetComponentInParent<DebuffSystem>();
    }

    public void UseCard()
    {
        if (_debuffs != null && _debuffs.IsStunned)
        {
            Debug.Log("Blocked: Player is stunned.");
            return;
        }

        Debug.Log("It got there");
        if (data == null) return;

        switch (data.category)
        {
            case CardData.CardCategory.Move:
                GameEvents.OnRequestMoveSelection?.Invoke(data.range);
                break;

            case CardData.CardCategory.Attack:
                GameEvents.OnRequestAttackMode?.Invoke(data);
                break;

            case CardData.CardCategory.Trap:
                GameEvents.OnRequestTrapSelection?.Invoke(data);
                break;

            case CardData.CardCategory.Utility:
                GameEvents.OnRequestUtilityAction?.Invoke(data);
                break;
        }
    }
}