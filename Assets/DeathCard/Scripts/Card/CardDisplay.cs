using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static event Action<CardDisplay, string, bool> OnRequestDescription;
    public static event Action<CardDisplay> OnHideDescription;

    [Header("References")]
    [SerializeField] private CardAction actionHandler;
    [SerializeField] private GameObject stunOverlay;
    [SerializeField] private CardBaker cardBaker;

    [Header("Card UI Elements")]
    public Image cardArt;
    public Image leftStatIcon;
    public Image rightStatIcon;
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI leftStatText;
    public TextMeshProUGUI rightStatText;
    public TextMeshProUGUI levelText;

    [Header("Icon Library")]
    public Sprite damageIcon;
    public Sprite rangeIcon;
    public Sprite effectiveRangeIcon;
    public Sprite durationIcon;
    public Sprite moveIcon;

    public bool IsDissolving => _isDissolving;

    private bool _isDissolving = false;
    private DebuffSystem _debuffs;

    private void Start()
    {
        _debuffs = GetComponentInParent<DebuffSystem>();
        ApplyCardData();
    }

    private void Update()
    {
        if (_debuffs != null)
        {
            if (stunOverlay.activeSelf != _debuffs.IsStunned)
                stunOverlay.SetActive(_debuffs.IsStunned);
        }
    }

    private void OnEnable()
    {
        if (actionHandler != null)
            actionHandler.OnDataChanged += ApplyCardData;
        ApplyCardData();
    }

    private void OnDisable()
    {
        if (actionHandler != null)
            actionHandler.OnDataChanged -= ApplyCardData;
    }

    public string GetDescriptionText() => actionHandler.data != null ? actionHandler.data.GetFullDescription() : "";

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnRequestDescription?.Invoke(this, GetDescriptionText(), false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnHideDescription?.Invoke(this);
    }

    public void ApplyCardData()
    {
        bool hasData = actionHandler != null && actionHandler.data != null;

        if (hasData)
        {
            cardBaker.SetBloodyIntensity(0f);
            cardBaker.SetHolographicIntensity(0f);

            CardData data = actionHandler.data;
            cardName.text = data.itemName;
            categoryText.text = data.category.ToString();
            cardArt.sprite = data.artSprite;
            levelText.text = $"{data.level}/{data.maxLevel}";
            UpdateStatSlot(data.leftStatType, leftStatText, leftStatIcon, data);
            UpdateStatSlot(data.rightStatType, rightStatText, rightStatIcon, data);

            cardBaker.Bake(() => {
                var mat = cardBaker.GetDissolveMaterial();
                if (mat != null) mat.SetFloat("_DissolveAmount", 0f);
            });
        }
        else
        {
            if (_isDissolving) return;

            var mat = cardBaker.GetDissolveMaterial();
            if (mat != null) mat.SetFloat("_DissolveAmount", 1f);
        }
    }

    [ContextMenu("Appear Card")]
    public void AppearCard()
    {
        if (actionHandler.data == null) return;
        var mat = cardBaker.GetDissolveMaterial();
        if (mat != null) mat.SetFloat("_DissolveAmount", 0f);
    }

    [ContextMenu("Remove Card")]
    public void RemoveCard()
    {
        if (_isDissolving) return;
        _isDissolving = true;
        cardBaker.AnimateDissolve(0f, OnDissolveComplete);
    }

    public void AddBloody(float intensity)
    {
        cardBaker.SetBloodyIntensity(intensity);
    }

    public void AddHolographic(float intensity)
    {
        cardBaker.SetHolographicIntensity(intensity);
    }

    private void OnDissolveComplete()
    {
        _isDissolving = false;
        actionHandler.data = null;
    }

    private void UpdateStatSlot(CardData.StatType type, TextMeshProUGUI text, Image icon, CardData data)
    {
        bool isVisible = type != CardData.StatType.None;
        text.gameObject.SetActive(isVisible);
        icon.gameObject.SetActive(isVisible);
        if (isVisible)
        {
            text.text = data.GetStatValue(type);
            icon.sprite = GetSpriteForStat(type, data.category);
        }
    }

    private Sprite GetSpriteForStat(CardData.StatType type, CardData.CardCategory category)
    {
        return type switch
        {
            CardData.StatType.Damage => damageIcon,
            CardData.StatType.EffectiveRange => effectiveRangeIcon,
            CardData.StatType.Duration => durationIcon,
            CardData.StatType.Range => (category == CardData.CardCategory.Move) ? moveIcon : rangeIcon,
            _ => null
        };
    }
}