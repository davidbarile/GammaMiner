using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "ShipConfig", menuName = "Data/ShipConfig", order = 1)]
public class ShipConfig : ShopItemConfig
{
    [Header("Hull")]
    public string PrefabName;

    [Header("Space")]
    [ReadOnly, Space, SerializeField] private int totalShipSpace;
    public int NumTiles;
    public int NumExternalMounts;
    public int MaxShieldRings;
    public int MaxTurrets;

    [Header("Health")]
    [ReadOnly, Space, SerializeField] private int totalShipHealth;
    public List<HealthData> HealthDatas = new();

    //final value could be affected by amount of tile space used
    [Header("Movement")]
    public float DecelerationRate;
    public float RotationSpeed;

    [Header("Components")]
    public ReactorConfig ReactorData;
    public BatteryConfig BatteryData;
    [ShowIf("@MaxShieldRings > 0")]
    public ShieldGeneratorConfig ShieldGeneratorData;
    public VaultConfig[] VaultDatas;
    public ThrusterConfig[] ThrusterDatas;
    public RailGunConfig[] RailGunDatas;
    public MissileLauncherConfig[] MissileLauncherDatas;
    public LaserCannonConfig[] LaserCannonDatas;

    [ShowIf("@MaxTurrets > 0")]
    public TurretConfig[] TurretDatas;

    private void OnValidate()
    {
        this.totalShipHealth = GetTotalShipHealth();
        this.totalShipSpace = GetTotalShipSpace();
    }

    public int GetTotalShipHealth()
    {
        var totalHealth = 0;

        //skip the first health entity, which is the hull 
        for (int i = 0; i < this.HealthDatas.Count; i++)
        {
            var data = this.HealthDatas[i];

            if (data != null)
                totalHealth += data.Health;
        }
        return totalHealth;
    }

    public int GetTotalShipSpace()
    {
        var space = 0;
        space += this.ReactorData.NumTilesRequired;
        space += this.BatteryData.NumTilesRequired;
        space += this.ShieldGeneratorData?.NumTilesRequired ?? 0;
        foreach (var vault in this.VaultDatas)
        {
            space += vault.NumTilesRequired;
        }
        foreach (var thruster in this.ThrusterDatas)
        {
            space += thruster.NumTilesRequired;
        }
        foreach (var railGun in this.RailGunDatas)
        {
            space += railGun.NumTilesRequired;
        }
        foreach (var missileLauncher in this.MissileLauncherDatas)
        {
            space += missileLauncher.NumTilesRequired;
        }
        foreach (var laserCannon in this.LaserCannonDatas)
        {
            space += laserCannon.NumTilesRequired;
        }
        foreach (var turret in this.TurretDatas)
        {
            space += turret.NumTilesRequired;
        }
        return space;
    }
}