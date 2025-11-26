using UnityEngine;

[CreateAssetMenu(fileName = "RailGunRounds", menuName = "Data/ShipComponents/Ammunition/RailGunRounds", order = 1)]
public class RailGunRoundsConfig : ShopItemConfig
{
    //TODO: make fun variation of rounds, e.g. explosive, armor piercing, etc.
    public enum ERailRoundType
    {
        Small,
        Medium,
        Large
    }

    public ERailRoundType RailgunRoundType;
    public int Damage;
}