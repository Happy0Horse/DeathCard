using System.Collections;
using UnityEngine;
using Mirror;

public class SacrificeDome : MonoBehaviour
{
    [Header("Chain Reference")]
    public bool isFirstDome = false;
    public SacrificeDome innerDome;
    public int domeIndex = 0;
    public float threshold = 500f;
    public float currentPoints = 0f;

    [Header("Pulse Settings")]
    public float flashSpeed = 5f;
    private float flashIntencity = 0f;

    [Header("Dissolve Settings")]
    public float dissolveSpeed = 1.5f;
    public float waitBeforeNextDome = 3f;

    private Renderer _renderer;
    private Material _domeMat;
    private bool _isShattering = false;
    private bool _isActive = false;
    private bool _isSkipped = false;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _domeMat = _renderer.material;
        UpdateInactiveAlpha();

        if (isFirstDome)
        {
            DomeInitializer.RegisterFirstDome(this);
        }
    }

    public void MarkSkipped()
    {
        _isSkipped = true;
    }

    public void EnableDome()
    {
        if (_isActive) return;
        _isActive = true;
        GlobalEvents.OnAnyDamageTaken -= AccumulateDamage;
        GlobalEvents.OnAnyDamageTaken += AccumulateDamage;
        Debug.Log($"[SacrificeDome] Дом {domeIndex} активирован");
        if (_domeMat != null) UpdateDomeVisuals();
    }

    void OnDestroy() => GlobalEvents.OnAnyDamageTaken -= AccumulateDamage;

    void Update()
    {
        if (!_isActive || _isShattering) return;
        if (flashIntencity > 0)
        {
            flashIntencity = Mathf.MoveTowards(flashIntencity, 0, Time.deltaTime * flashSpeed);
            _domeMat.SetFloat("_FlashIntencity", flashIntencity);
        }
    }

    void AccumulateDamage(float damage)
    {
        Debug.Log($"[SacrificeDome] Получен урон {damage}, текущие очки={currentPoints}");
        if (!_isActive || _isShattering) return;
        currentPoints += damage;
        flashIntencity = 1.0f;
        UpdateDomeVisuals();
        if (currentPoints >= threshold)
        {
            currentPoints = threshold;
            _isShattering = true;
            StartCoroutine(ShatterSequence());
        }
    }

    void UpdateInactiveAlpha()
    {
        if (_domeMat.HasProperty("_DomeColor"))
        {
            Color col = _domeMat.GetColor("_DomeColor");
            col.a = 0.2f;
            _domeMat.SetColor("_DomeColor", col);
        }
        if (_domeMat.HasProperty("_FlashIntencity"))
        {
            _domeMat.SetFloat("_FlashIntencity", 0f);
        }
    }

    void UpdateDomeVisuals()
    {
        float percent = Mathf.Clamp01(currentPoints / threshold);
        float visualAlpha = Mathf.Lerp(0.2f, 1.0f, percent);
        if (_domeMat.HasProperty("_DomeColor"))
        {
            Color col = _domeMat.GetColor("_DomeColor");
            col.a = visualAlpha;
            _domeMat.SetColor("_DomeColor", col);
        }
        if (_domeMat.HasProperty("_FlashColor"))
        {
            Color baseFlash = _domeMat.GetColor("_FlashColor");
            float h, s, v;
            Color.RGBToHSV(baseFlash, out h, out s, out v);
            float intensityBoost = Mathf.Lerp(1f, 3f, percent);
            Color finalFlash = Color.HSVToRGB(h, s, Mathf.Clamp(v * intensityBoost, 0, 5f));
            _domeMat.SetColor("_FlashColor", finalFlash);
        }
    }

    IEnumerator ShatterSequence()
    {
        GlobalEvents.OnAnyDamageTaken -= AccumulateDamage;
        float dissolve = 0;
        while (dissolve < 1.0f)
        {
            dissolve += Time.deltaTime * dissolveSpeed;
            _domeMat.SetFloat("_Dissolve", dissolve);
            yield return null;
        }

        if (!_isSkipped)
        {
            GlobalEvents.OnDomeBroken?.Invoke(); // локально для UI
            NetworkClient.Send(new DomeBrokenMessage()); // серверу
            if (innerDome != null)
                innerDome.StartCoroutine(innerDome.DelayedEnable(waitBeforeNextDome));
        }

        Destroy(gameObject);
    }

    public IEnumerator DelayedEnable(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnableDome();
    }
}