using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RailGunConfig", menuName = "Data/ShipComponents/RailGunConfig", order = 1)]
public class RailGunConfig : ShopItemConfig
{
    public int NumExternalMountsRequired;

    [Header("Fire Rate")]
    public float RoundsPerSecond;

    [Header("Projectiles")]
    public int MaxRoundsLight;
    public int MaxRoundsMedium;
    public int MaxRoundsHeavy;

    [Space]
    public string PrefabName = "RailRound";
}

/// <summary>
/// data class for saving to JSON
/// </summary>
[Serializable]
public class RailGunData : ShopItemDataBase
{
    [Header("Space Required")]
    public int NumExternalMountsRequired;

    [Header("Fire Rate")]
    public float RoundsPerSecond;

    [Header("Projectiles")]
    public int MaxRoundsLight;
    public int MaxRoundsMedium;
    public int MaxRoundsHeavy;

    [Space]
    public int NumRoundsLight;
    public int NumRoundsMedium;
    public int NumRoundsHeavy;

    [Space]
    public string PrefabName;

    public int TotalRoundsRemaining
    {
        get
        {
            return this.NumRoundsLight + this.NumRoundsMedium + this.NumRoundsHeavy;
        }
    }

    public static RailGunData GetDataFromConfig(RailGunConfig config)
    {
        var data = new RailGunData();

        if (config != null)
        {
            //ShopItemBase
            data.Name = config.Name;
            data.SubTitle = config.SubTitle;
            data.Id = config.Id;
            data.Price = config.Price;
            data.Quantity = config.Quantity;

            //RailGunData
            data.NumTilesRequired = config.NumTilesRequired;
            data.NumExternalMountsRequired = config.NumExternalMountsRequired;

            data.RoundsPerSecond = config.RoundsPerSecond;

            data.NumRoundsLight = config.MaxRoundsLight;
            data.NumRoundsMedium = config.MaxRoundsMedium;
            data.NumRoundsHeavy = config.MaxRoundsHeavy;

            data.MaxRoundsLight = config.MaxRoundsLight;
            data.MaxRoundsMedium = config.MaxRoundsMedium;
            data.MaxRoundsHeavy = config.MaxRoundsHeavy;

            data.PrefabName = config.PrefabName;
        }

        return data;
    }

    public static List<RailGunData> GetDatasFromConfig(RailGunConfig[] configArray)
    {
        List<RailGunData> list = new();
        foreach (var config in configArray)
        {
            list.Add(GetDataFromConfig(config));
        }

        return list;
    }
}