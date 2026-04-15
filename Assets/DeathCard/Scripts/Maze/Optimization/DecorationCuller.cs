using UnityEngine;

public class DecorationCuller : MonoBehaviour
{
    [Header("Culling Objects")] 
    [SerializeField] private GameObject visuals;
    [SerializeField] private Light lightSource;

    private void Awake()
    {
        if (visuals == null && transform.childCount > 0)
            visuals = transform.GetChild(0).gameObject;

        lightSource = GetComponentInChildren<Light>();

        if (visuals != null) visuals.SetActive(false);
        if (lightSource != null) lightSource.shadows = LightShadows.None;
    }

    public void SetVisible(bool visible)
    {
        if (visuals != null) visuals.SetActive(visible);
    }

    public void SetShadows(bool shadowsOn)
    {
        if (lightSource != null)
        {
            lightSource.shadows = shadowsOn ? LightShadows.Soft : LightShadows.None;
        }
    }
}