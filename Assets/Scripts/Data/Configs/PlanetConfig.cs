using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "PlanetConfig", menuName = "Data/Planets/PlanetConfig", order = 1)]
public class PlanetConfig : ScriptableObject
{
    [Header("Planet Settings")]
    public Vector2 RotationRange = Vector2.zero;
    public Vector2 ScaleRange = Vector2.one;

    [Header("Cloud Settings")]
    public bool HasClouds;
    [ShowIf("HasClouds")]
    public float CloudsAngleOffset;
    [ShowIf("HasClouds")]
    public float CloudRotationSpeed;
    [ShowIf("HasClouds")]
    public List<CloudData> CloudDatas = new();

    [Header("Stripes Settings")]
    public bool HasStripes;
    [ShowIf("HasStripes")]
    public float StripesAngleOffset;
    [ShowIf("HasStripes")]
    public float StripesRotationSpeed;
    [ShowIf("HasStripes")]
    public List<CloudData> StripeDatas = new();

    [Header("Crater Settings")]
    public bool HasCraters;
}

[Serializable]
public class PlanetData
{
    public string Name;
    public int RandomSeed = -1;
    public string RandomSeedString;
    public Vector3 Position;
    public Vector2 OverrideScaleRange = new(-1f, -1f);
}

[Serializable]
public class CloudData
{
    public bool IsVisible;
    public float CloudRotationSpeed;
}