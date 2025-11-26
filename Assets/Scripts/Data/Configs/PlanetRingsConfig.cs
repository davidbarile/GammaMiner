using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RingsConfig", menuName = "Data/Planets/RingsConfig", order = 2)]
public class PlanetRingsConfig : ScriptableObject
{
    [Header("Rings Settings")]
    public bool HasRings;
    public float RingsScale = 1f;
    public float RingsAngleOffset;
    public float RingsRotationSpeed;
    public float RingFlattenAmount;
    public List<RingData> RingDatas = new();
    public List<DustData> DustDatas = new();
}

[Serializable]
public class RingData
{
    public bool IsVisible;
    public bool IsEnabled;
}

[Serializable]
public class DustData
{
    public bool IsVisible;
    public bool IsEnabled;
    public float RotationSpeed;
}