using TMPro;
using UnityEngine;

public class DescriptionUIController : MonoBehaviour
{
    [SerializeField] private GameObject overlay;
    [SerializeField] private TextMeshProUGUI infoText;

    private void OnEnable()
    {
        CardDisplay.OnRequestDescription += HandleRequest;
        CardDisplay.OnHideDescription += HandleHide;
    }

    private void OnDisable()
    {
        CardDisplay.OnRequestDescription -= HandleRequest;
        CardDisplay.OnHideDescription -= HandleHide;
    }

    private void HandleRequest(CardDisplay card, string text, bool isSticky)
    {
        if (!string.IsNullOrEmpty(text))
        {
            infoText.text = text;
            overlay.SetActive(false);
        }
        else
        {
            overlay.SetActive(true);
        }
    }

    private void HandleHide(CardDisplay card)
    {
        overlay.SetActive(true);
    }
}