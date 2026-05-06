using UnityEngine;

public class CardAction : MonoBehaviour
{
    [SerializeField]
    private CardData _data;
    public System.Action OnDataChanged;

    public CardData data
    {
        get => _data;
        set
        {
            _data = value;
            OnDataChanged?.Invoke();
        }
    }

    private DebuffSystem _debuffs;

    private void Start()
    {
        _debuffs = GetComponentInParent<DebuffSystem>();
    }

    public void UseCard()
    {
        if (_debuffs != null && _debuffs.IsStunned) return;
        if (data == null) return;

        CardDisplay display = GetComponent<CardDisplay>();
        if (display != null && display.IsDissolving) return;

        CardManager manager = GetComponentInParent<CardManager>();
        System.Action onComplete = () => manager.RemoveCard(this);

        switch (data.category)
        {
            case CardData.CardCategory.Move:
                GameEvents.OnRequestMoveSelection?.Invoke(data.range, onComplete);
                break;
            case CardData.CardCategory.Attack:
                GameEvents.OnRequestAttackMode?.Invoke(data, onComplete);
                break;
            case CardData.CardCategory.Trap:
                GameEvents.OnRequestTrapSelection?.Invoke(data, onComplete);
                break;
            case CardData.CardCategory.Utility:
                GameEvents.OnRequestUtilityAction?.Invoke(data, onComplete);
                break;
        }
    }
}