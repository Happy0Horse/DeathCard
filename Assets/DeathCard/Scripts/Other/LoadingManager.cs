using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public enum LayoutPosition { Left, Right, Top }

    [System.Serializable]
    public struct LayoutData
    {
        public LayoutPosition position;
        public GameObject root;
        public TextMeshProUGUI messageField;
    }

    [System.Serializable]
    public struct BackgroundData
    {
        public Sprite sprite;
        public List<LayoutPosition> compatibleLayouts;
    }

    [System.Serializable]
    public struct WeightedMessage
    {
        public string text;
        public float weight;
    }

    public CanvasGroup masterGroup;
    public Image faderImage;
    public Image backgroundImage;

    public List<LayoutData> layouts;
    public List<BackgroundData> backgrounds;
    public List<WeightedMessage> messages;

    public float masterFadeSpeed = 2f;
    public float innerFadeSpeed = 5f;
    public float cycleInterval = 5f;

    public bool isLoading;
    private bool _lastLoadingState;
    private LayoutPosition _currentPos;
    private Coroutine _cycleCoroutine;

    private Sprite _lastSprite;
    private string _lastMessage;

    void Update()
    {
        float targetAlpha = isLoading ? 1 : 0;
        if (!Mathf.Approximately(masterGroup.alpha, targetAlpha))
        {
            masterGroup.alpha = Mathf.MoveTowards(masterGroup.alpha, targetAlpha, Time.deltaTime * masterFadeSpeed);
        }

        if (isLoading && !_lastLoadingState)
        {
            OnLoadingStarted();
        }
        else if (!isLoading && _lastLoadingState)
        {
            OnLoadingStopped();
        }

        _lastLoadingState = isLoading;
    }

    void OnLoadingStarted()
    {
        int index = Random.Range(0, layouts.Count);
        _currentPos = layouts[index].position;

        foreach (var l in layouts)
        {
            l.root.SetActive(l.position == _currentPos);
        }

        ApplyNewContent();
        _cycleCoroutine = StartCoroutine(ContentCycle());
    }

    void OnLoadingStopped()
    {
        if (_cycleCoroutine != null) StopCoroutine(_cycleCoroutine);

        Color c = faderImage.color;
        c.a = 0;
        faderImage.color = c;
    }

    IEnumerator ContentCycle()
    {
        while (isLoading)
        {
            yield return new WaitForSeconds(cycleInterval);

            yield return StartCoroutine(FadeFader(0, 1));
            ApplyNewContent();
            yield return StartCoroutine(FadeFader(1, 0));
        }
    }

    void ApplyNewContent()
    {
        List<BackgroundData> validBgs = backgrounds.FindAll(b => b.compatibleLayouts.Contains(_currentPos));

        if (validBgs.Count > 1)
        {
            validBgs.RemoveAll(b => b.sprite == _lastSprite);
        }

        if (validBgs.Count > 0)
        {
            Sprite selectedSprite = validBgs[Random.Range(0, validBgs.Count)].sprite;
            backgroundImage.sprite = selectedSprite;
            _lastSprite = selectedSprite;
        }

        List<WeightedMessage> validMessages = new List<WeightedMessage>(messages);

        if (validMessages.Count > 1)
        {
            validMessages.RemoveAll(m => m.text == _lastMessage);
        }

        float totalWeight = 0;
        foreach (var m in validMessages) totalWeight += m.weight;
        float roll = Random.Range(0, totalWeight);
        float s = 0;

        foreach (var m in validMessages)
        {
            s += m.weight;
            if (roll <= s)
            {
                var activeLayout = layouts.Find(l => l.position == _currentPos);
                if (activeLayout.messageField != null)
                {
                    activeLayout.messageField.text = m.text;
                    _lastMessage = m.text;
                }
                break;
            }
        }
    }

    IEnumerator FadeFader(float start, float end)
    {
        Color c = faderImage.color;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * innerFadeSpeed;
            c.a = Mathf.Lerp(start, end, t);
            faderImage.color = c;
            yield return null;
        }
    }
}