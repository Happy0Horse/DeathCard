using UnityEngine;

public class DamageDome : MonoBehaviour
{
    public float threshold = 500f;
    public float currentPoints = 0f;

    private Renderer _renderer;
    private Material _domeMat;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _domeMat = _renderer.material;

        GlobalEvents.OnAnyDamageTaken += AccumulateDamage;

        UpdateDomeAlpha();
    }

    void OnDestroy()
    {
        GlobalEvents.OnAnyDamageTaken -= AccumulateDamage;
    }

    void AccumulateDamage(float damage)
    {
        currentPoints += damage;
        UpdateDomeAlpha();

        if (currentPoints >= threshold)
        {
            OnThresholdReached();
        }
    }

    void UpdateDomeAlpha()
    {
        float percent = Mathf.Clamp01(currentPoints / threshold);
        float alpha = Mathf.Lerp(0.5f, 1.0f, percent);

        Color col = _domeMat.color;
        col.a = alpha;
        _domeMat.color = col;
    }

    void OnThresholdReached()
    {
        Debug.Log("Dome Threshold Reached!");
    }


}