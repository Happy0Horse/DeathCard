using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [SerializeField] private List<CardAction> cardSlots = new List<CardAction>(10);
    [SerializeField] private List<CardData> selectedCards = new List<CardData>();

    private void OnEnable()
    {
        GameManager.OnDistributeCards += HandleGlobalCardDistribution;
        GameManager.OnRoundOver += HandleRoundOver;
    }

    private void OnDisable()
    {
        GameManager.OnDistributeCards -= HandleGlobalCardDistribution;
        GameManager.OnRoundOver -= HandleRoundOver;
    }

    private void HandleRoundOver(int domeIndex)
    {
        foreach (CardAction slot in cardSlots)
        {
            if (slot != null && slot.data != null)
            {
                RemoveCard(slot);
            }
        }
    }

    private void HandleGlobalCardDistribution(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CardData data = GetRandomCardByWeight();
            if (data != null)
            {
                AddCard(data);
            }
        }
    }

    private CardData GetRandomCardByWeight()
    {
        if (selectedCards.Count == 0) return null;

        int totalWeight = 0;
        foreach (var card in selectedCards)
        {
            totalWeight += Mathf.Max(0, card.weight);
        }

        if (totalWeight <= 0) return selectedCards[UnityEngine.Random.Range(0, selectedCards.Count)];

        int rnd = UnityEngine.Random.Range(0, totalWeight);
        int processedWeight = 0;

        foreach (var card in selectedCards)
        {
            processedWeight += card.weight;
            if (rnd < processedWeight)
            {
                return card;
            }
        }

        return selectedCards[selectedCards.Count - 1];
    }

    public void AddCard(CardData data)
    {
        CardAction slot = GetEmptySlot();
        if (slot == null) return;

        slot.data = data;
        StartCoroutine(ApplyEffectsDelayed(slot, data));
    }

    private IEnumerator ApplyEffectsDelayed(CardAction slot, CardData data)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        ApplyEffects(slot, data);
    }

    private void ApplyEffects(CardAction slot, CardData data)
    {
        CardDisplay display = slot.GetComponent<CardDisplay>();
        if (display == null) return;

        if (data.level > 0)
        {
            float intensity = (float)data.level / data.maxLevel * 3f;
            display.AddBloody(intensity);
        }

        if (data.category == CardData.CardCategory.Utility &&
            data.utilityType == PlayerAnimation.UtilityType.GalaxyVoid)
        {
            display.AddHolographic(2f);
        }
    }

    public void RemoveCard(CardAction slot)
    {
        if (slot == null || slot.data == null) return;

        CardDisplay display = slot.GetComponent<CardDisplay>();
        if (display != null)
            display.RemoveCard();
        else
            slot.data = null;
    }

    private CardAction GetEmptySlot()
    {
        foreach (CardAction slot in cardSlots)
            if (slot != null && slot.data == null)
                return slot;
        return null;
    }

    public void ConsumeCard(CardAction slot)
    {
        if (slot == null) return;
        RemoveCard(slot);
    }
    
    public bool AddSelectedCard(CardData card)
    {
        if (card == null) return false;
        if (selectedCards.Count >= 5) return false;

        selectedCards.Add(card);
        Debug.Log($"Added card: {card.name}. Total selected cards: {selectedCards.Count}");
        return true;
    }

    public void RemoveSelectedCard(CardData card)
    {
        selectedCards.Remove(card);
    }
}