using UnityEngine;

public class CardAction : MonoBehaviour
{
    public CardData data;

    public void UseCard()
    {
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
                GameEvents.OnRequestTrapSelection?.Invoke(data.range);
                break;

            case CardData.CardCategory.Utility:
                GameEvents.OnRequestUtilityAction?.Invoke();
                break;
        }
    }
}