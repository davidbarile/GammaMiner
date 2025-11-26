using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MissileLauncherConfig", menuName = "Data/ShipComponents/MissileLauncherConfig", order = 1)]
public class MissileLauncherConfig : MountableItemConfig
{
    [Header("Projectiles")]
    public int MaxMissiles = 30;

    [Tooltip("In Milliseconds")]
    public int CooldownTime;

    public string PrefabName;
}

/// <summary>
/// data class for saving to JSON
/// </summary>
[Serializable]
public class MissileLauncherData : MountableItemData
{
    [Header("Projectiles")]
    public int NumMissiles;
    public int MaxMissiles;
    public int CooldownTime;

    public string PrefabName;

    public static MissileLauncherData GetDataFromConfig(MissileLauncherConfig config)
    {
        var data = new MissileLauncherData();

        if (config != null)
        {
            //ShopItemBase
            data.Name = config.Name;
            data.SubTitle = config.SubTitle;
            data.Id = config.Id;
            data.Price = config.Price;
            data.Quantity = config.Quantity;
            data.NumTilesRequired = config.NumTilesRequired;

            //MissileLauncherData
            data.NumExternalMountsRequired = config.NumExternalMountsRequired;

            data.NumMissiles = config.MaxMissiles;
            data.MaxMissiles = config.MaxMissiles;
            data.CooldownTime = config.CooldownTime;

            data.PrefabName = config.PrefabName;
        }

        return data;
    }

    public static List<MissileLauncherData> GetDatasFromConfig(MissileLauncherConfig[] configArray)
    {
        List<MissileLauncherData> list = new();
        foreach (var config in configArray)
        {
            list.Add(GetDataFromConfig(config));
        }

        return list;
    }
}