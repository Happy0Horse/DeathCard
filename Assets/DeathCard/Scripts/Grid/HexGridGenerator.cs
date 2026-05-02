using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(HexGrid))]
public class HexGridGenerator : MonoBehaviour
{
    public GameObject cellPrefab;
    public float outerRadius = 1f;
    public float gap = 0f;
    [Range(1, 50)] public int radius = 5;

    [Header("Elevation Settings")]
    public float elevationStep = 0.5f;

    private HexGrid _gridData;

    [ContextMenu("Generate Static Grid")]
    public void GenerateGrid()
    {
        if (cellPrefab == null) return;

        _gridData = GetComponent<HexGrid>();

        ClearExistingObjects();
        _gridData.Clear();

        float innerRadius = outerRadius * 0.866025404f;
        float xSpacing = (innerRadius + gap) * 2f;
        float zSpacing = (outerRadius + gap) * 1.5f;

        for (int z = -radius; z <= radius; z++)
        {
            float xOffset = (Mathf.Abs(z) % 2 == 1) ? innerRadius + gap : 0;

            for (int x = -radius; x <= radius; x++)
            {
                int q = x - (z - (z & 1)) / 2;
                int r = z;
                int hexDist = (Mathf.Abs(q) + Mathf.Abs(q + r) + Mathf.Abs(r)) / 2;

                if (hexDist > radius) continue;

                float y = (radius - hexDist) * elevationStep;
                Vector3 pos = transform.TransformPoint(new Vector3(x * xSpacing + xOffset, y, z * zSpacing));

                GameObject cellObj;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    cellObj = (GameObject)PrefabUtility.InstantiatePrefab(cellPrefab);
                    cellObj.transform.position = pos;
                }
                else
#endif
                {
                    cellObj = Instantiate(cellPrefab, pos, Quaternion.identity);
                }

                cellObj.transform.position = pos;
                cellObj.transform.SetParent(transform);
                cellObj.name = $"Hex_{q}_{r}";

                HexCell cell = cellObj.GetComponent<HexCell>();
                if (cell != null)
                {
                    cell.Initialize(new Vector2Int(q, r));
                }

                _gridData.AddCell(new Vector2Int(q, r), cellObj);
            }
        }

        _gridData.LinkNeighbors();
        Debug.Log("[HexGridGenerator] Static grid generated. You can now customize cells manually.");
    }

    private void ClearExistingObjects()
    {
#if UNITY_EDITOR
        if (!EditorUtility.DisplayDialog("Clear Grid?", "This will delete all child objects. Proceed?", "Yes", "No"))
            return;
#endif

        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }
}