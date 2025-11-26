using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThrusterConfig", menuName = "Data/ShipComponents/ThrusterConfig", order = 1)]
public class ThrusterConfig : MountableItemConfig
{
    [Header("Energy")]
    public float DischargeRate;

    [Header("Movement")]
    public float AccelerationRate;
    public float MaxAcceleration;
}

/// <summary>
/// data class for saving to JSON
/// </summary>
[Serializable]
public class ThrusterData : MountableItemData
{
    [Header("Energy")]
    public float DischargeRate;

    [Header("Movement")]
    public float AccelerationRate;
    public float MaxAcceleration;

    public static ThrusterData GetDataFromConfig(ThrusterConfig config)
    {
        var data = new ThrusterData();

        if (config != null)
        {
            //ShopItemBase
            data.Name = config.Name;
            data.SubTitle = config.SubTitle;
            data.Id = config.Id;
            data.Price = config.Price;
            data.Quantity = config.Quantity;
            data.NumTilesRequired = config.NumTilesRequired;

            //ThrusterData
            data.NumExternalMountsRequired = config.NumExternalMountsRequired;

            data.DischargeRate = config.DischargeRate;

            data.AccelerationRate = config.AccelerationRate;
            data.MaxAcceleration = config.MaxAcceleration;
        }

        return data;
    }

    public static List<ThrusterData> GetDatasFromConfig(ThrusterConfig[] configArray)
    {
        List<ThrusterData> list = new();
        foreach (var config in configArray)
        {
            list.Add(GetDataFromConfig(config));
        }

        return list;
    }
}