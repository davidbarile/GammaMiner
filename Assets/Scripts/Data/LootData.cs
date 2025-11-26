using System;
using Sirenix.OdinInspector;
using UnityEngine;
using static CrystalData;

[CreateAssetMenu(fileName = "LootData", menuName = "Data/LootData", order = 2)]
public class LootData : ScriptableObject
{
    [Flags]
    public enum ELootType
    {
        None = 0,
        Health = 1 << 0,
        Energy = 1 << 1,
        RailRounds = 1 << 2,
        Missiles = 1 << 3,
        Credits = 1 << 4,
        Crystals = 1 << 5,
        Collectible = 1 << 6,
        Armor = 1 << 7,
        ShieldBuff = 1 << 8,
        TurningBuff = 1 << 9,
        Other = 1 << 10,
    }

    public enum ELootPickupMode
    {
        None,
        ShipCollision,
        AllProjectiles,
        Lasers,
        RailRounds
    }

    public enum ELootPickupPermissions
    {
        Player,
        Enemy,
        Any,
        None
    }

    public string Name;

    public ELootType LootType;

    [ShowIf("LootType", ELootType.Crystals)]
    public ECrystalType CrystalType = ECrystalType.None;

    [Space]
    public ELootPickupMode PickupMode;
    public ELootPickupPermissions PickupPermissions;

    [Header("Quantity to spawn on rock break")]
    public WeightedRandom QuantityMinMax;
    [Range(1, 100)] public int IncrementSize = 1;
    [Space] public string PrefabName;

    public int GetWeightedRandomQuantity()
    {
        return this.IncrementSize * this.QuantityMinMax.GetWeightedRandomQuantity();
    }
}