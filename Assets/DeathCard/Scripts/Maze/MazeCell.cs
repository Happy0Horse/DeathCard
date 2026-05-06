using System.Collections.Generic;
using UnityEngine;

// This needs seed random
public class MazeCell : MonoBehaviour
{
    public int gridX;
    public int gridY;
    public bool visited;

    [Header("Standard Walls")]
    public GameObject wallTop;
    public GameObject wallRight;
    public GameObject wallBottom;
    public GameObject wallLeft;

    [Header("Pillars")]
    public GameObject pillarTR;
    public GameObject pillarTL;
    public GameObject pillarBL;
    public GameObject pillarBR;

    [Header("Floor")]
    public MeshRenderer floorRenderer;

    [Header("Wall Variations")]
    public List<WeightedPrefab> wallVariants;

    public void RemoveWallTop() => wallTop?.SetActive(false);
    public void RemoveWallRight() => wallRight?.SetActive(false);
    public void RemoveWallBottom() => wallBottom?.SetActive(false);
    public void RemoveWallLeft() => wallLeft?.SetActive(false);

    public void RemovePillarTR() => pillarTR?.SetActive(false);
    public void RemovePillarTL() => pillarTL?.SetActive(false);
    public void RemovePillarBL() => pillarBL?.SetActive(false);
    public void RemovePillarBR() => pillarBR?.SetActive(false);

    public void SetFloorMaterial(Material mat)
    {
        if (floorRenderer != null)
        {
            floorRenderer.material = mat;
        }
    }

    public void TryReplaceWithVariant(GameObject originalWall, System.Random _rng)
    {
        if (originalWall == null || !originalWall.activeSelf) return;
        if (wallVariants == null || wallVariants.Count == 0) return;

        GameObject variantPrefab = GetWeightedVariant(_rng);

        if (variantPrefab != null)
        {
            Instantiate(variantPrefab, originalWall.transform.position, originalWall.transform.rotation, transform);
            originalWall.SetActive(false);
        }
    }

    private GameObject GetWeightedVariant(System.Random _rng)
    {
        int roll = _rng.Next(0, 100);
        int current = 0;

        foreach (var v in wallVariants)
        {
            current += v.weight;
            if (roll < current) return v.prefab;
        }

        return null;
    }
}