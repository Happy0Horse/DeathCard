using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class CardInventorySlot : MonoBehaviour
{
    [Header("Card UI Elements")]
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI categoryText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image cardArtImage;
    [Header("Baker")]
    [SerializeField] private CardBaker cardBaker;
    private CardData _currentData;

    private void OnEnable()
    {
        if (_currentData != null)
        {
            ApplyText(_currentData);
            ApplyEffects(_currentData);
            cardBaker.Bake();
        }
    }

    public void SetCard(CardData data)
    {
        _currentData = data;
        ApplyText(data);
        ApplyEffects(data);
        if (gameObject.activeInHierarchy)
            cardBaker.Bake();
    }

    public void Clear()
    {
        _currentData = null;
        cardBaker.SetBloodyIntensity(0f);
        cardBaker.SetHolographicIntensity(0f);
    }

    public void AddBloody(float intensity)
    {
        cardBaker.SetBloodyIntensity(intensity);
    }

    public void AddHolographic(float intensity)
    {
        cardBaker.SetHolographicIntensity(intensity);
    }

    private void ApplyText(CardData data)
    {
        cardNameText.text = data.itemName;
        categoryText.text = data.category.ToString();
        levelText.text = $"{data.level}/{data.maxLevel}";
        cardArtImage.sprite = data.artSprite;
    }

    private void ApplyEffects(CardData data)
    {
        cardBaker.SetBloodyIntensity(0f);
        cardBaker.SetHolographicIntensity(0f);

        if (data.level > 0)
        {
            float intensity = (float)data.level / data.maxLevel * 3f;
            cardBaker.SetBloodyIntensity(intensity);
        }

        if (data.category == CardData.CardCategory.Utility &&
            data.utilityType == PlayerAnimation.UtilityType.GalaxyVoid)
        {
            cardBaker.SetHolographicIntensity(2f);
        }
    }
}