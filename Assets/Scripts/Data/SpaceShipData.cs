using UnityEngine;

[CreateAssetMenu(fileName = "ShipData(old)", menuName = "Data/SpaceShipData(old)", order = 1)]
public class SpaceShipData : ScriptableObject
{
    [Header("Health")]
    public int BodyHealth = 500;
    public int NoseHealth = 50;
    public int LeftWingHealth = 100;
    public int RightWingHealth = 100;
    public int EngineHealth = 150;

    [Header("Energy")]
    public float EnergyLevel = 10000;
    public float EnergyRechargeRate = 1;//fun to modify this with a boost
    [Space]
    public float LaserChargeRate = 10;
    public float JetsChargeRate = 5;
    [Space]
    public float ShieldsChargeRate = 10;//fun to modify this with a boost
    public float ShieldsDischargeRate = 3;
    public float ShieldsDelayToDischarge = 10;

    [Header("Lasers")]
    public int MaxLaserCharge = 500;

    [Header("Shields")]
    public float OuterShieldBrickHealth = 30;
    public float MiddleShieldBrickHealth = 50;
    public float InnerShieldBrickHealth = 100;

    [Header("Projectiles")]
    public int NumRailRounds = 500;
    public int MaxRailRounds = 1000;
    [Space]
    public int NumMissiles = 20;
    public int MaxMissiles = 30;

    [Header("Movement")]
    public float AccelerationRate = 40;
    public float DecelerationRate = 3;
    public float MaxAcceleration = 100;
    public float RotationSpeed = 3;
}