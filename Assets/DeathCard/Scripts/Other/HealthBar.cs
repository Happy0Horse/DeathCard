using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;
    private Material _healthMat;

    void Start()
    {
        _healthMat = fillImage.material;
    }

    public void SetHealth(float current, float max)
    {
        float percent = current / max;
        Debug.Log("Percent " + percent);
        _healthMat.SetFloat("FillAmount", percent);
    }
}