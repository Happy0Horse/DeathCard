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

    private void EnsureInitialized()
    {
        if (_bloodyMaterialInstance != null || _holographicMaterialInstance != null) return;

        if (dissolveMaterialTemplate != null)
        {
            _dissolveMaterialInstance = new Material(dissolveMaterialTemplate);
            dissolveOverlay.material = _dissolveMaterialInstance;
            dissolveOverlay.gameObject.SetActive(true);
            _dissolveMaterialInstance.SetFloat("_DissolveAmount", 0f);
        }

        if (bloodyMaterialTemplate != null)
        {
            _bloodyMaterialInstance = new Material(bloodyMaterialTemplate);
            bloodyOverlay.material = _bloodyMaterialInstance;
            bloodyOverlay.gameObject.SetActive(false);
        }

        if (holographicMaterialTemplate != null)
        {
            _holographicMaterialInstance = new Material(holographicMaterialTemplate);
            holographicOverlay.material = _holographicMaterialInstance;
            holographicOverlay.gameObject.SetActive(false);
        }
    }

    private void Awake() => EnsureInitialized();

    public void Bake(Action onBakeComplete = null)
    {
        EnsureInitialized();
        if (_bakeCoroutine != null) StopCoroutine(_bakeCoroutine);
        _bakeCoroutine = StartCoroutine(DoBake(onBakeComplete));
    }

    private IEnumerator DoBake(Action onBakeComplete)
    {
        Vector3 originalPos = dissolveOverlay != null ? dissolveOverlay.rectTransform.localPosition : Vector3.zero;
        Vector3 hidePos = new Vector3(0, 10000, 0);

        if (dissolveOverlay != null) dissolveOverlay.rectTransform.localPosition = hidePos;
        if (bloodyOverlay != null) bloodyOverlay.rectTransform.localPosition = hidePos;
        if (holographicOverlay != null) holographicOverlay.rectTransform.localPosition = hidePos;

        yield return new WaitForEndOfFrame();

        Vector3[] corners = new Vector3[4];
        cardRect.GetWorldCorners(corners);

        Camera cam = uiCamera != null ? uiCamera : Camera.main;
        Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        Vector2 topRight = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);

        int x = Mathf.Max(0, Mathf.RoundToInt(bottomLeft.x));
        int y = Mathf.Max(0, Mathf.RoundToInt(bottomLeft.y));
        int width = Mathf.RoundToInt(topRight.x - bottomLeft.x);
        int height = Mathf.RoundToInt(topRight.y - bottomLeft.y);

        width = Mathf.Clamp(width, 1, Screen.width - x);
        height = Mathf.Clamp(height, 1, Screen.height - y);

        if (width > 0 && height > 0)
        {
            if (_bakedTexture != null) Destroy(_bakedTexture);
            _bakedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            _bakedTexture.ReadPixels(new Rect(x, y, width, height), 0, 0);
            _bakedTexture.Apply();

            if (_dissolveMaterialInstance != null) _dissolveMaterialInstance.SetTexture("_BakedTex", _bakedTexture);
            if (_bloodyMaterialInstance != null) _bloodyMaterialInstance.SetTexture("_BakedTex", _bakedTexture);
            if (_holographicMaterialInstance != null) _holographicMaterialInstance.SetTexture("_BakedTex", _bakedTexture);
        }

        if (dissolveOverlay != null) dissolveOverlay.rectTransform.localPosition = originalPos;
        if (bloodyOverlay != null) bloodyOverlay.rectTransform.localPosition = originalPos;
        if (holographicOverlay != null) holographicOverlay.rectTransform.localPosition = originalPos;

        if (dissolveOverlay != null) dissolveOverlay.gameObject.SetActive(true);
        if (bloodyOverlay != null) bloodyOverlay.gameObject.SetActive(_bloodyMaterialInstance != null && _bloodyMaterialInstance.GetFloat("_Intensity") > 0);
        if (holographicOverlay != null) holographicOverlay.gameObject.SetActive(_holographicMaterialInstance != null && _holographicMaterialInstance.GetFloat("_Intensity") > 0);

        onBakeComplete?.Invoke();
    }

    public void AnimateDissolve(float target, Action onComplete = null)
    {
        EnsureInitialized();
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
        EnsureInitialized();
        bloodyOverlay.gameObject.SetActive(intensity > 0);
        _bloodyMaterialInstance.SetFloat("_Intensity", intensity);
    }

    public void SetHolographicIntensity(float intensity)
    {
        EnsureInitialized();
        holographicOverlay.gameObject.SetActive(intensity > 0);
        _holographicMaterialInstance.SetFloat("_Intensity", intensity);
    }

    public Texture2D GetBakedTexture() => _bakedTexture;
    public Material GetDissolveMaterial() { EnsureInitialized(); return _dissolveMaterialInstance; }
    public Material GetBloodyMaterial() { EnsureInitialized(); return _bloodyMaterialInstance; }
    public Material GetHolographicMaterial() { EnsureInitialized(); return _holographicMaterialInstance; }

    private void OnDestroy()
    {
        if (_bakedTexture != null)
            Destroy(_bakedTexture);
    }
}