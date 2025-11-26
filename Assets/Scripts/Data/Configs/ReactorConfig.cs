using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ReactorConfig", menuName = "Data/ShipComponents/ReactorConfig", order = 1)]
public class ReactorConfig : ShopItemConfig
{
    [Header("Energy")]
    [Range(0f, 2f)] public float EnergyRechargeRate;

    public int SpriteIndex;
    public Color ReactorColor = Color.white;
}

/// <summary>
/// data class for saving to JSON
/// </summary>
[Serializable]
public class ReactorData : ShopItemDataBase
{
    [Header("Energy")]
    public float EnergyRechargeRate;
    public int SpriteIndex;
    public Color ReactorColor = Color.white;

    public static ReactorData GetDataFromConfig(ReactorConfig config)
    {
        var data = new ReactorData();

        if (config != null)
        {
            //ShopItemBase
            data.Name = config.Name;
            data.SubTitle = config.SubTitle;
            data.Id = config.Id;
            data.Price = config.Price;
            data.Quantity = config.Quantity;
            data.NumTilesRequired = config.NumTilesRequired;

            //ReactorData
            data.EnergyRechargeRate = config.EnergyRechargeRate;
            data.SpriteIndex = config.SpriteIndex;
            data.ReactorColor = config.ReactorColor;
        }

        return data;
    }
}