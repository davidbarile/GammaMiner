using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using static CrystalData;
using static LootData;
using static RockData;

[CreateAssetMenu(fileName = "MiningToolConfig", menuName = "Data/MiningToolConfig", order = 9)]

public class MiningToolConfig : ScriptableObject
{
    public enum EMiningToolType
    {
        None,
        Railgun,
        LaserCannon,
        Missile,
        MiningLaser_Green,
        MiningLaser_Blue,
        MiningLaser_Red,
        MiningLaser_Purple
    }

    public EMiningToolType MiningToolType;
    public MiningRockDamageMod[] MiningRockDamageMods;
    public LootProbabilityMod[] LootProbabilityMods;

    private Dictionary<ERockType, MiningRockDamageMod> miningRockDamageDict = new();
    private Dictionary<ELootType, LootProbabilityMod> lootProbabilityDict = new();
    private Dictionary<ECrystalType, LootProbabilityMod> crystalProbabilityDict = new();

    public void CreateDictionaries()
    {
        foreach (var mod in this.MiningRockDamageMods)
        {
            if (!this.miningRockDamageDict.ContainsKey(mod.MineableRockTypes))
            {
                this.miningRockDamageDict.Add(mod.MineableRockTypes, mod);
                mod.CreateDictionaries();
            }
            else
                Debug.LogError($"<color=red>Duplicate RockType {mod.MineableRockTypes} in MiningLaserData.MiningRockDamageMods</color>");
        }

        foreach (var mod in this.LootProbabilityMods)
        {
            if(mod.LootType == ELootType.Crystals)
            {
                if (!this.crystalProbabilityDict.ContainsKey(mod.CrystalType))
                {
                    this.crystalProbabilityDict.Add(mod.CrystalType, mod);
                    mod.CreateCrystalDictionaries();
                }
                else
                    Debug.LogError($"<color=red>Duplicate CrystalType {mod.CrystalType} in MiningLaserData.LootProbabilityMods</color>");
            }
            else if (!this.lootProbabilityDict.ContainsKey(mod.LootType))
            {
                this.lootProbabilityDict.Add(mod.LootType, mod);
                mod.CreateDictionaries();
            }
            else
                Debug.LogError($"<color=red>Duplicate LootType {mod.LootType} in MiningLaserData.LootProbabilityMods</color>");
        }
    }

    public float GetDamageMultiplier(ERockType inRockType)
    {
        foreach (var kvp in this.miningRockDamageDict)
        {
            if (kvp.Key.HasFlag(inRockType))
            {
                return kvp.Value.GetDamageMultiplier(inRockType);
            }
        }
        return 0f;
    }

    public float GetLootProbabilityFactor(ELootType inLootType, ECrystalType inCrystalType = ECrystalType.None)
    {
        foreach (var kvp in this.lootProbabilityDict)
        {
            if (kvp.Key.HasFlag(inLootType))
            {
                if (inLootType == ELootType.Crystals)
                {
                    if (kvp.Value.CrystalType.HasFlag(inCrystalType))
                    {
                        return kvp.Value.GetCrystalLootProbabilityFactor(inCrystalType);
                    }
                }
                return kvp.Value.GetLootProbabilityFactor(inLootType);
            }
        }
        return 0f;
    }

    public float GetLootYieldFactor(ELootType inLootType, ECrystalType inCrystalType = ECrystalType.None)
    {
        foreach (var kvp in this.lootProbabilityDict)
        {
            if (kvp.Key.HasFlag(inLootType))
            {
                if (inLootType == ELootType.Crystals)
                {
                    if (kvp.Value.CrystalType.HasFlag(inCrystalType))
                    {
                        return kvp.Value.GetCrystalLootYieldFactor(inCrystalType);
                    }
                }
                return kvp.Value.GetLootYieldFactor(inLootType);
            }
        }
        return 0f;
    }
}

[Serializable]
public class MiningRockDamageMod
{
    public ERockType MineableRockTypes;
    [Range(0f, 5f)] public float DamageMultiplier = 1f;

    private Dictionary<ERockType, float> rockTypeDamageDict = new();

    public void CreateDictionaries()
    {
        foreach (ERockType rockType in Enum.GetValues(typeof(ERockType)))
        {
            if (MineableRockTypes.HasFlag(rockType) && !rockTypeDamageDict.ContainsKey(rockType))
            {
                rockTypeDamageDict.Add(rockType, DamageMultiplier);
            }
        }
    }

    public float GetDamageMultiplier(ERockType inRockType)
    {
        if (rockTypeDamageDict.TryGetValue(inRockType, out var multiplier))
        {
            return multiplier;
        }
        return 0f;
    }
}

[Serializable]
public class LootProbabilityMod
{
    public ELootType LootType;
    [ShowIf("LootType", ELootType.Crystals)]
    public ECrystalType CrystalType;
    [Range(0f, 1f)] public float LootProbabilityFactor = 1f;
    [Range(0f, 1f)] public float LootYieldFactor = 1f;

    private Dictionary<ELootType, float> lootTypeProbabilityDict = new();
    private Dictionary<ELootType, float> lootTypeYieldDict = new();
    private Dictionary<ECrystalType, float> crystalTypeProbabilityDict = new();
    private Dictionary<ECrystalType, float> crystalTypeYieldDict = new();

    public void CreateDictionaries()
    {
        foreach (ELootType lootType in Enum.GetValues(typeof(ELootType)))
        {
            if (LootType.HasFlag(lootType) && !lootTypeProbabilityDict.ContainsKey(lootType))
                lootTypeProbabilityDict.Add(lootType, LootProbabilityFactor);

            if (!lootTypeYieldDict.ContainsKey(lootType))
                lootTypeYieldDict.Add(lootType, LootYieldFactor);
        }
    }

    public void CreateCrystalDictionaries()
    {
        foreach (ECrystalType crystalType in Enum.GetValues(typeof(ECrystalType)))
        {
            if (CrystalType.HasFlag(crystalType) && !crystalTypeProbabilityDict.ContainsKey(crystalType))
                crystalTypeProbabilityDict.Add(crystalType, LootProbabilityFactor);
                    
            if (!crystalTypeYieldDict.ContainsKey(crystalType))
                crystalTypeYieldDict.Add(crystalType, LootYieldFactor);
        }
    }

    public float GetLootProbabilityFactor(ELootType inLootType)
    {
        if (lootTypeProbabilityDict.TryGetValue(inLootType, out var factor))
        {
            return factor;
        }
        return 0f;
    }

    public float GetCrystalLootProbabilityFactor(ECrystalType inCrystalType)
    {
        if (crystalTypeProbabilityDict.TryGetValue(inCrystalType, out var factor))
        {
            return factor;
        }
        return 0f;
    }

    public float GetLootYieldFactor(ELootType inLootType)
    {
        if (lootTypeYieldDict.TryGetValue(inLootType, out var factor))
        {
            return factor;
        }
        return 0f;
    }

    public float GetCrystalLootYieldFactor(ECrystalType inCrystalType)
    {
        if (crystalTypeYieldDict.TryGetValue(inCrystalType, out var factor))
        {
            return factor;
        }
        return 0f;
    }
}