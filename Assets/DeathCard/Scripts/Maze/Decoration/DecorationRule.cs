using System;
using UnityEngine;

[Serializable]
public class DecorationRule
{
    public string ruleName;
    public bool isEnabled = true;
    [Header("Must match one in group")]
    public string mountKey;
    public GameObject prefab;

    [Header("Placement Logic")]
    [Tooltip("% chance to appear. Isn`t used if interval is > 0.")]
    [Range(0, 100)] public int chance = 20;

    [Tooltip("Every N cells, try to spawn. 0 = purely random.")]
    public int interval = 5;

    [Tooltip("Randomize the interval by +/- this amount.")]
    public int intervalRandomness = 2;

    [Header("Constraints")]
    public bool deadEndsOnly = false;

    [HideInInspector] public int stepsSinceLast = 0;
    [HideInInspector] public int currentTargetInterval = 0;
}