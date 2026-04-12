using UnityEngine;

public class DecorationCuller : MonoBehaviour
{
    [SerializeField] private GameObject visuals;

    private void Awake()
    {
        if (visuals == null && transform.childCount > 0)
            visuals = transform.GetChild(0).gameObject;

        if (visuals != null) visuals.SetActive(false);
    }

    public void SetVisible(bool visible)
    {
        if (visuals != null) visuals.SetActive(visible);
    }
}