using UnityEngine;

[CreateAssetMenu(fileName = "RepairsConfig", menuName = "Data/RepairsConfig", order = 2)]
public class RepairsConfig : ShopItemConfig
{
    public enum ERepairType
    {
        None,
        HealHull,
        RestoreHullToMax,
        HealComponents,
        RestoreDestroyedComponents,
        RestoreAll,
        RadiationDecontamination
    }

    [Space(20)]
    public ERepairType RepairType;

    public bool ShouldCalculatePrice;

    public int RepairAmount;
}