using System;
using UnityEngine;

[Serializable]
public class ShopItemDataBase
{
    [Header("Shop Item")]
    public string Name;
    public string SubTitle;
    public string Id;
    public int Price;
    public int Quantity = 1;

    public int NumTilesRequired; // Space required for this item, e.g. 1 tile in the ship

    //public static ShopItemDataBase GetDataFromConfig(ShopItemConfig config)
    //{
    //    var data = new ShopItemDataBase();

    //    if (config != null)
    //    {
    //        data.Name = config.Name;
    //        data.SubTitle = config.SubTitle;
    //        data.Id = config.Id;
    //        data.Price = config.Price;
    //        data.Quantity = config.Quantity;
    //    }

    //    return data;
    //}
}

[Serializable]
public abstract class MountableItemData : ShopItemDataBase
{
    [Header("MountableItemData vars")]
    public int NumExternalMountsRequired = 1;
}