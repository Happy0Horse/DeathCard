using System.Collections.Generic;
using UnityEngine;

public static class CardStorage
{
    private static List<ItemData> cards = new List<ItemData>();

    public static void AddCard(ItemData card)
    {
        if (card == null) return;

        cards.Add(card);
    }

    public static List<ItemData> GetRandomCards(int count)
    {
        List<ItemData> result = new List<ItemData>();

        if (cards.Count == 0)
            return result;

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, cards.Count);
            result.Add(cards[randomIndex]);
        }

        return result;
    }
}