using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public static event Action<CardDisplay, string, bool> OnRequestDescription;
    public static event Action<CardDisplay> OnHideDescription;

    [Header("References")]
    [SerializeField] private CardAction actionHandler;
    [SerializeField] private GameObject stunOverlay;
    [SerializeField] private GameObject noCardOverlay;

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

    private bool _isSticky = false;
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
            {
                stunOverlay.SetActive(_debuffs.IsStunned);
            }
        }
    }

    private void OnEnable()
    {
        if (actionHandler != null)
        {
            actionHandler.OnDataChanged += ApplyCardData;
        }
        ApplyCardData();
    }

    private void OnDisable()
    {
        if (actionHandler != null)
        {
            actionHandler.OnDataChanged -= ApplyCardData;
        }
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            _isSticky = !_isSticky;
            OnRequestDescription?.Invoke(this, GetDescriptionText(), true);
        }
    }

    public void ApplyCardData()
    {
        bool hasData = actionHandler != null && actionHandler.data != null;
        if (noCardOverlay != null) noCardOverlay.SetActive(!hasData);

        if (!hasData) return;

        CardData data = actionHandler.data;
        cardName.text = data.itemName;
        categoryText.text = data.category.ToString();
        cardArt.sprite = data.artSprite;
        levelText.text = $"{data.level}/{data.maxLevel}";

        UpdateStatSlot(data.leftStatType, leftStatText, leftStatIcon, data);
        UpdateStatSlot(data.rightStatType, rightStatText, rightStatIcon, data);
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