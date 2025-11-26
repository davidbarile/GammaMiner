using UnityEngine;

[CreateAssetMenu(fileName = "Missiles", menuName = "Data/ShipComponents/Ammunition/Missiles", order = 2)]
public class MissilesConfig : ShopItemConfig
{
    //TODO: make fun variation of missiles, e.g. long range, air-to-air, mining, incendiary, EMP, etc.
    public enum EMissileType
    {
        Small,
        Medium,
        Large
    }

    public EMissileType MissileType;
    public int Damage;
    public float Speed;
}