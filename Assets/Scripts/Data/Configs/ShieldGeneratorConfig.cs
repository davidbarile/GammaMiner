using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ShieldGeneratorConfig", menuName = "Data/ShipComponents/ShieldGeneratorConfig", order = 1)]
public class ShieldGeneratorConfig : ShopItemConfig
{
    [Header("Energy")]
    public float ChargeRate;
    public float DischargeRate = 3;
    public float DelayToDischarge = 10;

    [Header("Rings")]
    public int MaxShieldRings;

    [Header("Health")]
    public float InnerShieldBrickHealth;
    public float MiddleShieldBrickHealth;
    public float OuterShieldBrickHealth;
}

/// <summary>
/// data class for saving to JSON
/// </summary>
[Serializable]
public class ShieldGeneratorData : ShopItemDataBase
{
    [Header("Energy")]
    public float ChargeRate;
    public float DischargeRate = 3;
    public float DelayToDischarge = 10;

    [Header("Rings")]
    public int MaxShieldRings;

    [Header("Health")]
    public float InnerShieldBrickHealth;
    public float MiddleShieldBrickHealth;
    public float OuterShieldBrickHealth;
    

    public static ShieldGeneratorData GetDataFromConfig(ShieldGeneratorConfig config)
    {
        var data = new ShieldGeneratorData();

        if (config != null)
        {
            //ShopItemBase
            data.Name = config.Name;
            data.SubTitle = config.SubTitle;
            data.Id = config.Id;
            data.Price = config.Price;
            data.Quantity = config.Quantity;

            //ShieldGeneratorData
            data.NumTilesRequired = config.NumTilesRequired;

            data.ChargeRate = config.ChargeRate;
            data.DischargeRate = config.DischargeRate;
            data.DelayToDischarge = config.DelayToDischarge;

            data.MaxShieldRings = config.MaxShieldRings;

            data.InnerShieldBrickHealth = config.InnerShieldBrickHealth;
            data.MiddleShieldBrickHealth = config.MiddleShieldBrickHealth;
            data.OuterShieldBrickHealth = config.OuterShieldBrickHealth;
        }

        return data;
    }
}