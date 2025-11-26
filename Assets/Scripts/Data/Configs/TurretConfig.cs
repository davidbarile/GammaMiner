using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TurretConfig", menuName = "Data/ShipComponents/TurretConfig", order = 1)]
public class TurretConfig : MountableItemConfig
{
    [Header("Space")]
    public int NumMounts;

    [Header("Movement")]
    public float RotationSpeed;
}

/// <summary>
/// data class for saving to JSON
/// </summary>
[Serializable]
public class TurretData : MountableItemData
{
    [Header("Space")]
    public int NumMounts;

    [Header("Movement")]
    public float RotationSpeed;

    public static TurretData GetDataFromConfig(TurretConfig config)
    {
        var data = new TurretData();

        if (config != null)
        {
            //ShopItemBase
            data.Name = config.Name;
            data.SubTitle = config.SubTitle;
            data.Id = config.Id;
            data.Price = config.Price;
            data.Quantity = config.Quantity;

            //TurretData
            data.NumExternalMountsRequired = config.NumExternalMountsRequired;

            data.RotationSpeed = config.RotationSpeed;
        }

        return data;
    }

    public static List<TurretData> GetDatasFromConfig(TurretConfig[] configArray)
    {
        List<TurretData> list = new();
        foreach (var config in configArray)
        {
            list.Add(GetDataFromConfig(config));
        }

        return list;
    }
}