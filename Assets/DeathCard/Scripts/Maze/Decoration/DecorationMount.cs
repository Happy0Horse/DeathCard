using System;
using UnityEngine;

[Serializable]
public class DecorationMount
{
    public Transform transform;

    [Header("0: Top, 1: Right, 2: Bottom, 3: Left")]
    public int wallDirection = 0;
}