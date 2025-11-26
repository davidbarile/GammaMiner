using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static MiningToolConfig;

[CreateAssetMenu(fileName = "LaserCannonConfig", menuName = "Data/ShipComponents/LaserCannonConfig", order = 1)]
public class LaserCannonConfig : MountableItemConfig
{
    [Space] public bool IsMiningLaser;

    [Header("Energy")]
    [Tooltip("Discharge rate when mining laser")]
    public float ChargeRate = 3;
    public int MaxCharge = 500;

    [ShowIf("IsMiningLaser")]
    public float MiningLaserDamagePerSecond = 10f;

    public Color LaserColor = Color.white;
    public int EnergyMeterGradientIndex = 0;

    public string PrefabName = "Laser";

    public EMiningToolType MiningToolType;
}

/// <summary>
/// data class for saving to JSON
/// </summary>
[Serializable]
public class LaserCannonData : MountableItemData
{
    [Space] public bool IsMiningLaser;

    [Header("Energy")]
    public float ChargeRate = 3;
    public int MaxCharge = 500;
    public float MiningLaserDamagePerSecond = 10f;

    public Color LaserColor = Color.white;
    public int EnergyMeterGradientIndex = 0;
    public string PrefabName;
    public EMiningToolType MiningToolType;

    public static LaserCannonData GetDataFromConfig(LaserCannonConfig config)
    {
        var data = new LaserCannonData();

        if (config != null)
        {
            //ShopItemBase
            data.Name = config.Name;
            data.SubTitle = config.SubTitle;
            data.Id = config.Id;
            data.Price = config.Price;
            data.Quantity = config.Quantity;
            data.NumTilesRequired = config.NumTilesRequired;

            //LaserCannonData
            data.NumExternalMountsRequired = config.NumExternalMountsRequired;

            data.IsMiningLaser = config.IsMiningLaser;
            data.ChargeRate = config.ChargeRate;
            data.MaxCharge = config.MaxCharge;
            data.LaserColor = config.LaserColor;
            data.EnergyMeterGradientIndex = config.EnergyMeterGradientIndex;
            data.PrefabName = config.PrefabName;
            data.MiningToolType = config.MiningToolType;
            data.MiningLaserDamagePerSecond = config.MiningLaserDamagePerSecond;
        }

        return data;
    }

    public static List<LaserCannonData> GetDatasFromConfig(LaserCannonConfig[] configArray)
    {
        List<LaserCannonData> list = new();
        foreach (var config in configArray)
        {
            list.Add(GetDataFromConfig(config));
        }

        return list;
    }
}