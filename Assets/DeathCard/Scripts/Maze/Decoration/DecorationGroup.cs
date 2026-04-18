using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DecorationGroup
{
    [Header("Must match one in rule")]
    public string key;
    public List<DecorationMount> mounts = new List<DecorationMount>();
}