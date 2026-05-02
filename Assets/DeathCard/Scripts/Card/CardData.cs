using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/CardData")]
public class CardData : ItemData
{
    public enum CardCategory { Move, Attack, Trap, Utility }
    public enum StatType { None, Range, Damage, EffectiveRange, Duration, Custom }

    [Serializable]
    public struct DescriptionLine
    {
        public StatType stat;
        public string labelOverride;
        public string suffix;
        [TextArea(1, 3)] public string customText;
    }

    [Header("Type")]
    public CardCategory category;
    public PlayerAnimation.AttackMode attackMode;
    public PlayerAnimation.UtilityType utilityType;
    public PlayerAnimation.TrapType trapType;

    [Header("Stats")]
    public int range;
    public int damage; 
    public int effectiveRange;
    public float effectDuration;
    public bool isMultiHit;

    [Header("Level")]
    public int level = 0;
    public int maxLevel = 3;

    [Header("UI Stat")]
    public Sprite artSprite;
    public StatType leftStatType;
    public StatType rightStatType; 
    public List<DescriptionLine> descriptionLayout;

    public string GetStatValue(StatType type)
    {
        return type switch
        {
            StatType.Range => range.ToString(),
            StatType.Damage => damage.ToString(),
            StatType.EffectiveRange => effectiveRange.ToString(),
            StatType.Duration => effectDuration.ToString("F1"),
            _ => ""
        };
    }

    public string GetFullDescription()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(itemName + ": ");
        foreach (var line in descriptionLayout)
        {
            if (line.stat == StatType.Custom)
            {
                sb.AppendLine(line.customText);
                continue;
            }

            string label = string.IsNullOrEmpty(line.labelOverride) ? line.stat.ToString() : line.labelOverride;
            string value = GetStatValue(line.stat);
            sb.AppendLine($"{label}: {value}{line.suffix}");
        }
        return sb.ToString().TrimEnd();
    }
}
