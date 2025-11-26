using System.Collections.Generic;
using UnityEngine;

public class MapSaveData : MonoBehaviour
{
    public int MapNum;
    public Dictionary<string, RockSaveData> DirtyRocks = new();
}

public class RockSaveData
{
    public string RockName;//can be removed
    public int TimeMined = -1;
    public int Health;
    public LootData LootData = null;
    public int LootQuantity = 0;
    public int MaxLootQuantity = 0;
}