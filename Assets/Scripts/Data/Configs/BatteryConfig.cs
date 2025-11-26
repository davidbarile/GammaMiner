using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BatteryConfig", menuName = "Data/ShipComponents/BatteryConfig", order = 1)]
public class BatteryConfig : ShopItemConfig
{
    [Header("Energy")]
    public float MaxEnergyCharge;
    public int SpriteIndex;
}

/// <summary>
/// data class for saving to JSON
/// </summary>
[Serializable]
public class BatteryData : ShopItemDataBase
{
    [Header("Energy")]
    public float MaxEnergyCharge;
    public float EnergyCharge { get; set; }
    public int SpriteIndex;

    public static BatteryData GetDataFromConfig(BatteryConfig config)
    {
        var data = new BatteryData();

        if (config != null)
        {
            //ShopItemBase
            data.Name = config.Name;
            data.SubTitle = config.SubTitle;
            data.Id = config.Id;
            data.Price = config.Price;
            data.Quantity = config.Quantity;
            data.NumTilesRequired = config.NumTilesRequired;

            //BatteryData
            data.MaxEnergyCharge = config.MaxEnergyCharge;
            data.SpriteIndex = config.SpriteIndex;
        }

        return data;
    }
}