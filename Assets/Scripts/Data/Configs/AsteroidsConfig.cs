using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "AsteroidsConfig", menuName = "Data/Planets/AsteroidsConfig", order = 3)]
public class AsteroidsConfig : ScriptableObject
{
    [Header("Asteroids Settings")]
    public bool HasEntities;

    [ShowIf("HasEntities")]
    public bool IsMoonConfig;

    [ShowIf("@HasEntities && !IsMoonConfig")]
    public List<AsteroidBeltData> AsteroidBeltDatas = new();

    [ShowIf("@HasEntities && IsMoonConfig")]
    public List<AsteroidBeltData> MoonDatas = new();
}

[Serializable]
public class AsteroidBeltData
{
    public bool IsVisible;

    [ShowIf("IsVisible")]
    public bool IsMoon;

    [ShowIf("IsVisible")]
    public Vector2 NumberOfAsteroidsRange = Vector2.one;
    [ShowIf("IsVisible")]
    public Vector2 AsteroidSizeRange = Vector2.one;
    [ShowIf("IsVisible")]
    public Vector2 YOffsetRange = Vector2.zero;
    [ShowIf("IsVisible")]
    [Space] public Vector2 OrbitRadiusRange = new(5f, 10f);
    [ShowIf("IsVisible")]
    public Vector2 OrbitAngleRange = new(-1f, -1f);

    [ShowIf("IsVisible")]
    [Space] public Vector2 OrbitRotationSpeedRange = Vector2.zero;
    [ShowIf("IsVisible")]
    public Vector2 AsteroidRotationSpeedRange = Vector2.zero;

    [ShowIf("IsVisible")]
    [Space] public int AsteroidBaseDepth = -100;//may not need

    //NOTE: these are for storing colors to make ConfigureFromData easier (not have to take a bunch of color params)

    //[Header("Asteroid Colors")]
    [HideInInspector] public Color AsteroidOutlineColor1 = Color.black;
    [HideInInspector] public Color AsteroidOutlineColor2 = Color.black;
    [HideInInspector] public Color AsteroidFillColor1 = Color.white;
    [HideInInspector] public Color AsteroidFillColor2 = Color.white;
    
    //[Header("Moon Colors")]
    [HideInInspector] public Color MoonOutlineColor = Color.black;
    [HideInInspector] public Color MoonFillColor = Color.white;
}