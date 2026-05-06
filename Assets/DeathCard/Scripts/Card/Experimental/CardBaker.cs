using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardBaker : MonoBehaviour
{
    [SerializeField] private CardDisplay cardDisplay;
    [SerializeField] private Material dissolveMaterialTemplate;
    [SerializeField] private Image dissolveOverlay;
    [SerializeField] private float dissolveDuration = 1.5f;

    [SerializeField] private Material bloodyMaterialTemplate;
    [SerializeField] private Image bloodyOverlay;

    [SerializeField] private Material holographicMaterialTemplate;
    [SerializeField] private Image holographicOverlay;

    [SerializeField] private RectTransform cardRect;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private RawImage debugPreview;

    private Texture2D _bakedTexture;
    private Material _dissolveMaterialInstance;
    private Material _bloodyMaterialInstance;
    private Material _holographicMaterialInstance;

    private Coroutine _dissolveCoroutine;
    private Coroutine _bakeCoroutine;

    private void Awake()
    {
        _dissolveMaterialInstance = new Material(dissolveMaterialTemplate);
        dissolveOverlay.material = _dissolveMaterialInstance;
        dissolveOverlay.gameObject.SetActive(true);
        _dissolveMaterialInstance.SetFloat("_DissolveAmount", 0f);

        _bloodyMaterialInstance = new Material(bloodyMaterialTemplate);
        bloodyOverlay.material = _bloodyMaterialInstance;
        bloodyOverlay.gameObject.SetActive(false);

        _holographicMaterialInstance = new Material(holographicMaterialTemplate);
        holographicOverlay.material = _holographicMaterialInstance;
        holographicOverlay.gameObject.SetActive(false);
    }

    public void Bake(Action onBakeComplete = null)
    {
        if (_bakeCoroutine != null) StopCoroutine(_bakeCoroutine);
        _bakeCoroutine = StartCoroutine(DoBake(onBakeComplete));
    }

    private IEnumerator DoBake(Action onBakeComplete)
    {
        Vector3 originalPos = dissolveOverlay.rectTransform.localPosition;
        Vector3 hidePos = new Vector3(0, 10000, 0);

        dissolveOverlay.rectTransform.localPosition = hidePos;
        bloodyOverlay.rectTransform.localPosition = hidePos;
        holographicOverlay.rectTransform.localPosition = hidePos;

        yield return new WaitForEndOfFrame();

        Vector3[] corners = new Vector3[4];
        cardRect.GetWorldCorners(corners);

        Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]);
        Vector2 topRight = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[2]);

        int width = Mathf.RoundToInt(topRight.x - bottomLeft.x);
        int height = Mathf.RoundToInt(topRight.y - bottomLeft.y);

        if (width > 0 && height > 0)
        {
            int x = Mathf.RoundToInt(bottomLeft.x);
            int y = Mathf.RoundToInt(bottomLeft.y);

            if (_bakedTexture != null) Destroy(_bakedTexture);
            _bakedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            _bakedTexture.ReadPixels(new Rect(x, y, width, height), 0, 0);
            _bakedTexture.Apply();

            _dissolveMaterialInstance.SetTexture("_BakedTex", _bakedTexture);
            _bloodyMaterialInstance.SetTexture("_BakedTex", _bakedTexture);
            _holographicMaterialInstance.SetTexture("_BakedTex", _bakedTexture);
        }

        dissolveOverlay.rectTransform.localPosition = originalPos;
        bloodyOverlay.rectTransform.localPosition = originalPos;
        holographicOverlay.rectTransform.localPosition = originalPos;

        dissolveOverlay.gameObject.SetActive(true);
        bloodyOverlay.gameObject.SetActive(_bloodyMaterialInstance.GetFloat("_Intensity") > 0);
        holographicOverlay.gameObject.SetActive(_holographicMaterialInstance.GetFloat("_Intensity") > 0);

        onBakeComplete?.Invoke();
    }

    public void AnimateDissolve(float target, Action onComplete = null)
    {
        if (_dissolveMaterialInstance == null) return;
        if (_dissolveCoroutine != null) StopCoroutine(_dissolveCoroutine);
        float flippedTarget = 1f - target;
        _dissolveCoroutine = StartCoroutine(DoAnimateDissolve(flippedTarget, onComplete));
    }

    private IEnumerator DoAnimateDissolve(float target, Action onComplete)
    {
        float current = _dissolveMaterialInstance.GetFloat("_DissolveAmount");
        float elapsed = 0f;

        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dissolveDuration);
            _dissolveMaterialInstance.SetFloat("_DissolveAmount", Mathf.Lerp(current, target, t));
            yield return null;
        }

        _dissolveMaterialInstance.SetFloat("_DissolveAmount", target);
        onComplete?.Invoke();
    }

    public void SetBloodyIntensity(float intensity)
    {
        if (_bloodyMaterialInstance == null) return;
        bloodyOverlay.gameObject.SetActive(intensity > 0);
        _bloodyMaterialInstance.SetFloat("_Intensity", intensity);
    }

    public void SetHolographicIntensity(float intensity)
    {
        if (_holographicMaterialInstance == null) return;
        holographicOverlay.gameObject.SetActive(intensity > 0);
        _holographicMaterialInstance.SetFloat("_Intensity", intensity);
    }

    public Texture2D GetBakedTexture() => _bakedTexture;
    public Material GetDissolveMaterial() => _dissolveMaterialInstance;
    public Material GetBloodyMaterial() => _bloodyMaterialInstance;
    public Material GetHolographicMaterial() => _holographicMaterialInstance;

    private void OnDestroy()
    {
        if (_bakedTexture != null)
        {
            Destroy(_bakedTexture);
        }
    }
}