using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static HealthData;

[Serializable]
public class ShipData
{
    [Header("Shop Item Vars")]
    public string Name;
    public string Id;
    public int BasePrice;
    public int TotalPrice;

    [Header("Hull")]
    public string PrefabName;

    [Header("Space")]
    public int NumTotalTiles;
    [Space]
    public int NumUsedExternalMounts;
    public int MaxExternalMounts;
    public int MaxShieldRings;
    public int MaxTurrets;

    [Header("Health")]
    public List<HealthData> HealthDatas = new();

    //final value could be affected by amount of tile space used
    [Header("Movement")]
    public float DecelerationRate;
    public float RotationSpeed;

    [Header("Components")]
    public ReactorData ReactorData;
    public BatteryData BatteryData;
    [ShowIf("@MaxShieldRings > 0")]
    public ShieldGeneratorData ShieldGeneratorData;
    public List<VaultData> VaultDatas = new();
    public List<ThrusterData> ThrusterDatas = new();
    public List<RailGunData> RailgunDatas = new();
    public List<MissileLauncherData> MissileLauncherDatas = new();
    public List<LaserCannonData> LaserCannonDatas = new();
    [ShowIf("@MaxTurrets > 0")]
    public List<TurretData> TurretDatas = new();

    [NonSerialized]
    private Dictionary<EHealthDataType, List<HealthData>> healthDataLookup;

    public List<HealthData> GetHeathsOfType(EHealthDataType type)
    {
        if( this.healthDataLookup == null)
            this.CreateLookupDict();
            
        if (this.healthDataLookup.TryGetValue(type, out var healths))
        {
            return healths;
        }
        return new List<HealthData>();
    }

    private void CreateLookupDict()
    {
        this.healthDataLookup = new Dictionary<EHealthDataType, List<HealthData>>();
        foreach (var healthData in this.HealthDatas)
        {
            if (!this.healthDataLookup.ContainsKey(healthData.Type))
            {
                this.healthDataLookup[healthData.Type] = new List<HealthData>();
            }
            this.healthDataLookup[healthData.Type].Add(healthData);
        }
    }

    public int NumUsedTiles
    {
        get
        {
            int usedTiles = 0;

            usedTiles += ReactorData?.NumTilesRequired ?? 0;
            //Debug.Log($" -Reactor = {ReactorData.NumTilesRequired}");

            usedTiles += this.BatteryData?.NumTilesRequired ?? 0;
            //Debug.Log($" -Battery = {BatteryData.NumTilesRequired}");

            usedTiles += this.ShieldGeneratorData?.NumTilesRequired ?? 0;
            //Debug.Log($" -ShieldGenerator = {ShieldGeneratorData.NumTilesRequired}");

            foreach (var vaultData in this.VaultDatas)
            {
                usedTiles += vaultData.NumTilesRequired;
                //Debug.Log($" -Vault = {vaultData.NumTilesRequired}");
            }

            foreach (var thrusterData in this.ThrusterDatas)
            {
                usedTiles += thrusterData.NumTilesRequired;
                //Debug.Log($" -Thruster = {thrusterData.NumTilesRequired}");
            }

            foreach (var railGunData in this.RailgunDatas)
            {
                usedTiles += railGunData.NumTilesRequired;
                //Debug.Log($" -RailGun = {railGunData.NumTilesRequired}");
            }

            foreach (var missileLauncherData in this.MissileLauncherDatas)
            {
                usedTiles += missileLauncherData.NumTilesRequired;
                //Debug.Log($"MissileLauncher = {missileLauncherData.NumTilesRequired}"); 
            }

            foreach (var laserCannonData in this.LaserCannonDatas)
            {
                usedTiles += laserCannonData.NumTilesRequired;
                //Debug.Log($" -LaserCannon = {laserCannonData.NumTilesRequired}");
            }

            if (usedTiles > this.NumTotalTiles)
            {
                //Debug.Log($"<color=red>Used tiles ({usedTiles}) exceed total tiles ({this.NumTotalTiles}) for ship: {this.Name}</color>");
                usedTiles = this.NumTotalTiles; // Clamp to max available tiles
            }
            else
            {
                //Debug.Log($"<color=#00FFFF>Used tiles {usedTiles}/{this.NumTotalTiles} for ship: {this.Name}</color>");
            }

            return usedTiles;
        }
    }

    public int GetNumAvailableTiles()
    {
        return this.NumTotalTiles - this.NumUsedTiles;
    }

    public static ShipData GetDataFromConfig(ShipConfig config)
    {
        var data = new ShipData();

        if (config != null)
        {
            data.Name = config.Name;
            data.Id = config.Id;
            data.BasePrice = config.Price;
            data.TotalPrice = 999;//TODO: calculate based on components
            //hull
            data.PrefabName = config.PrefabName;
            //space
            data.NumTotalTiles = config.NumTiles;
            data.MaxExternalMounts = config.NumExternalMounts;
            //shields
            data.MaxShieldRings = config.MaxShieldRings;
            data.MaxTurrets = config.MaxTurrets;
            //health
            for (int i = 0; i < config.HealthDatas.Count; i++)
            {
                var hData = config.HealthDatas[i];
                var healthData = new HealthData
                {
                    Type = hData.Type,
                    Health = hData.Health,
                    MaxHealth = hData.MaxHealth
                };
                data.HealthDatas.Add(healthData);
            }

            //convert configs to data for JSON serialization
            data.ReactorData = ReactorData.GetDataFromConfig(config.ReactorData);
            data.BatteryData = BatteryData.GetDataFromConfig(config.BatteryData);
            data.ShieldGeneratorData = ShieldGeneratorData.GetDataFromConfig(config.ShieldGeneratorData);

            //arrays to lists
            data.VaultDatas = VaultData.GetDatasFromConfig(config.VaultDatas);
            data.ThrusterDatas = ThrusterData.GetDatasFromConfig(config.ThrusterDatas);
            data.RailgunDatas = RailGunData.GetDatasFromConfig(config.RailGunDatas);
            data.MissileLauncherDatas = MissileLauncherData.GetDatasFromConfig(config.MissileLauncherDatas);
            data.LaserCannonDatas = LaserCannonData.GetDatasFromConfig(config.LaserCannonDatas);
            data.TurretDatas = TurretData.GetDatasFromConfig(config.TurretDatas);

            data.DecelerationRate = config.DecelerationRate;
            data.RotationSpeed = config.RotationSpeed;
        }

        return data;
    }
}