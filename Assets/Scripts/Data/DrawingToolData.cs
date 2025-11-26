using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "DrawToolData", menuName = "Data/Draw Tool Data", order = 0)]
public class DrawingToolData : ScriptableObject
{
    [ShowInInspector]
    public Dictionary<int, RockEditData> RocksToModify = new Dictionary<int, RockEditData>();
    public bool DidApplicationQuit = false;
}

[Serializable]
public class RockEditData
{
    public int Id;
    public bool IsVisible = true;
    public Color Color = Color.clear;
    public Color OutlineColor = Color.black;
    public Texture2D Texture;
    public int Health;
    public float BaseRockHealthOverride = -1;
    public RockData RockData;
}