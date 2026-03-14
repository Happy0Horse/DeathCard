using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CanvasToMeshScaler : MonoBehaviour
{
    [Header("Placement Settings")]
    [SerializeField] private float yOffset = 1f;

    [Header("Resolution Settings")]
    [SerializeField] private float resolution = 50f;

    [Header("Visual Settings")]
    [SerializeField] private bool disablePanelImage = false;

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (Application.isPlaying) this.enabled = false;
    }

    void OnValidate()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += SyncUI;
#endif
    }

    public void SyncUI()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall -= SyncUI;
#endif

        if (this == null || rect == null) return;

        rect.localPosition = new Vector3(0, yOffset, 0);
        rect.localRotation = Quaternion.Euler(90, 0, 0);

        float res = Mathf.Max(1f, resolution);
        rect.sizeDelta = new Vector2(res, res);

        float invRes = 1f / res;
        rect.localScale = new Vector3(invRes, invRes, invRes);

        ToggleChildImage();
    }

    private void ToggleChildImage()
    {
        if (transform.childCount > 0)
        {
            Transform firstChild = transform.GetChild(0);
            Image img = firstChild.GetComponent<Image>();

            if (img != null)
            {
                img.enabled = !disablePanelImage;
            }
        }
    }
}