using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;
    public float smoothSpeed = 5f;

    private Material _healthMat;
    private float _targetPercent;
    private float _currentVelocity;
    private float _displayPercent;

    void Start()
    {
        _healthMat = fillImage.material;

        PlayerStat stats = GetComponentInParent<PlayerStat>();
        if (stats != null)
        {
            _targetPercent = stats.currentHealth / stats.maxHealth;
            _displayPercent = _targetPercent;
            _healthMat.SetFloat("_FillAmount", _displayPercent);
        }
    }

    void Update()
    {
        if (!Mathf.Approximately(_displayPercent, _targetPercent))
        {
            _displayPercent = Mathf.Lerp(_displayPercent, _targetPercent, Time.deltaTime * smoothSpeed);
            _healthMat.SetFloat("_FillAmount", _displayPercent);
        }
    }

    public void SetHealth(float current, float max)
    {
        _targetPercent = current / max;
    }
}