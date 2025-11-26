using System;
using UnityEngine;

public enum EShopItemType
{
    None,
    RailRounds,
    Missiles,
    Ship,
    Reactor,
    Battery,
    Vault,
    Thrusters,
    LaserCannons,
    ShieldGenerators,
    Armor,
    Railguns,
    MissileLaunchers,
    Turrets,
    Repairs,
    Sensors,
    Tools
}

[CreateAssetMenu(fileName = "ShopItem", menuName = "Data/ShopItem", order = 5)]
public class ShopItemConfig : ScriptableObject
{
    [Header("Shop Item Vars")]
    public string Name;
    public string SubTitle;
    public string Id;
    [SerializeField] private bool shouldAutoGenerateId = true;
    public int Price;
    public EShopItemType ShopItemType;
    public Sprite Sprite;
    public int Quantity = 1;
    public int NumTilesRequired; // Space required for this item, e.g. 1 tile in the ship
    public Color Color = Color.white;
    [Space] public bool OverwriteTypeDescription;
    [TextArea(0, 10)] public string Description;

    //TODO: add randomize of Quantities and Costs

    private void OnValidate()
    {
        if(this.shouldAutoGenerateId)
            this.Id = this.name;
    }
}

[Serializable]
public abstract class MountableItemConfig : ShopItemConfig
{
    [Header("MountableItemData vars")]
    public int NumExternalMountsRequired = 1;
}