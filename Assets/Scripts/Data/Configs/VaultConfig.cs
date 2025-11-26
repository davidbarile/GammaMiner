using System;
using System.Collections.Generic;
using UnityEngine;
using static CrystalData;

[CreateAssetMenu(fileName = "VaultConfig", menuName = "Data/ShipComponents/VaultConfig", order = 1)]
public class VaultConfig : ShopItemConfig
{
    [Header("Storage")]
    public int Capacity;
    
    public ECrystalType AllowedCrystalTypes;

    [Space]
    public float MaxRadiationLevel;
    public int MaxRadiationFlushes;
    [Range(0f, 1f)]
    public float RadiationShielding;
    [Space]
    public int SpriteIndex;
}

/// <summary>
/// data class for saving to JSON
/// </summary>
[Serializable]
public class VaultData : ShopItemDataBase
{
    [Header("Storage")]
    public Dictionary<string, int> StoredCrystalsDict = new();

    public int Capacity;

    public ECrystalType AllowedCrystalTypes;

    [Range(0f, 1f)]
    public float RadiationShielding;

    [Space]
    public float RadiationLevel = 0;
    public float MaxRadiationLevel;

    [Space]
    public int RadiationFlushes = 0;
    public int MaxRadiationFlushes;

    [Space]
    public int SpriteIndex;

    public int GetUsedStorage()
    {
         var total = 0;
            foreach (var kvp in StoredCrystalsDict)
            {
                total += kvp.Value;
            }
            return total;
    }

    public static VaultData GetDataFromConfig(VaultConfig config)
    {
        var data = new VaultData();

        if (config != null)
        {
            //ShopItemBase
            data.Name = config.Name;
            data.SubTitle = config.SubTitle;
            data.Id = config.Id;
            data.Price = config.Price;
            data.Quantity = config.Quantity;
            data.NumTilesRequired = config.NumTilesRequired;

            //VaultData
            data.Capacity = config.Capacity;
            data.AllowedCrystalTypes = config.AllowedCrystalTypes;
            data.MaxRadiationLevel = config.MaxRadiationLevel;
            data.MaxRadiationFlushes = config.MaxRadiationFlushes;
            data.RadiationShielding = config.RadiationShielding;
            data.SpriteIndex = config.SpriteIndex;

            var allowedCrystalTypes = Enum.GetValues(config.AllowedCrystalTypes.GetType());
            foreach (var crystalType in allowedCrystalTypes)
            {
                int intValue = (int)crystalType;
                if (intValue != 0 && config.AllowedCrystalTypes.HasFlag((Enum)crystalType))
                {
                    data.StoredCrystalsDict.Add(crystalType.ToString(), 0);
                }
            }
        }

        return data;
    }

    public static List<VaultData> GetDatasFromConfig(VaultConfig[] configArray)
    {
        List<VaultData> list = new();
        foreach (var config in configArray)
        {
            list.Add(GetDataFromConfig(config));
        }

        return list;
    }
}