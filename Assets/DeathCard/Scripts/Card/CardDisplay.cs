using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardAction actionHandler;
    [SerializeField] private GameObject stunOverlay;
    [SerializeField] private GameObject noCardOverlay;

    [Header("Images")]
    public Image cardArt;
    public Image categoryIcon;
    public Image leftStatIcon;
    public Image rightStatIcon;

    [Header("Text")]
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI leftStatText;
    public TextMeshProUGUI rightStatText;

    [Header("Stat Icon Library")]
    public Sprite damageIcon;
    public Sprite rangeIcon;
    public Sprite moveIcon;

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

    //private void OnEnable()
    //{
    //    if (actionHandler != null)
    //    {
    //        actionHandler.OnDataChanged += ApplyCardData;
    //    }
    //}

    //private void OnDisable()
    //{
    //    if (actionHandler != null)
    //    {
    //        actionHandler.OnDataChanged -= ApplyCardData;
    //    }
    //}

    public void ApplyCardData()
    {
        bool hasData = actionHandler != null && actionHandler.data != null;
        noCardOverlay.SetActive(!hasData);

        if (!hasData) return;

        noCardOverlay.SetActive(false);
        CardData data = actionHandler.data;

        cardName.text = data.itemName;
        categoryText.text = data.category.ToString();
        categoryIcon.sprite = data.categoryIcon;
        cardArt.sprite = data.artSprite;

        if (data.category == CardData.CardCategory.Attack)
        {
            SetStats(data.damage.ToString(), damageIcon, data.range.ToString(), rangeIcon);
        }
        else if (data.category == CardData.CardCategory.Move)
        {
            SetStats(data.range.ToString(), moveIcon, data.range.ToString(), rangeIcon);
        }
    }

    private void SetStats(string leftVal, Sprite leftImg, string rightVal, Sprite rightImg)
    {
        leftStatText.text = leftVal;
        leftStatIcon.sprite = leftImg;

        rightStatText.text = rightVal;
        rightStatIcon.sprite = rightImg;
    }
}