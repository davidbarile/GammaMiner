using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static CrystalData;
using static MiningToolConfig;

public class GlobalData : MonoBehaviour
{
    public static GlobalData IN;

    [Serializable]
    public class IconData
    {
        public Sprite Sprite;
        public Sprite MonotoneSprite;
    }

    public Gradient[] LaserMeterGradients;
    public Gradient[] BatteryMeterGradients;

    [Space, SerializeField] private CrystalsConfig crystalsConfig;

    [Space, SerializeField] private IconData[] shipIconDatas;
    [Space, SerializeField] private IconData[] reactorIconDatas;
    [Space, SerializeField] private IconData[] batteryIconDatas;
    [Space, SerializeField] private IconData[] vaultIconDatas;

    [Header("Mining Laser Configs")]

    [Space, SerializeField] private MiningToolConfig[] miningToolConfigs;

    private Dictionary<EMiningToolType, MiningToolConfig> miningToolDataDict = new();

    private Dictionary<ECrystalType, CrystalData> crystalDataDict = new();

    private void Awake()
    {
        foreach (var config in this.miningToolConfigs)
        {
            if (!this.miningToolDataDict.ContainsKey(config.MiningToolType))
            {
                config.CreateDictionaries();
                this.miningToolDataDict.Add(config.MiningToolType, config);
            }
            else
                Debug.LogError($"<color=red>Duplicate MiningToolType {config.MiningToolType} in GlobalData.miningToolConfigs</color>");
        }

        foreach (var crystalData in this.crystalsConfig.CrystalDatas)
        {
            if (!this.crystalDataDict.ContainsKey(crystalData.CrystalType))
            {
                this.crystalDataDict.Add(crystalData.CrystalType, crystalData);
            }
            else
                Debug.LogError($"<color=red>Duplicate CrystalType {crystalData.CrystalType} in GlobalData.crystalsConfig</color>");
        }
    }

    public static MiningToolConfig GetMiningToolConfig(EMiningToolType inMiningToolType)
    {
        if (IN.miningToolDataDict.TryGetValue(inMiningToolType, out var config))
        {
            return config;
        }
        Debug.LogError($"<color=red>No MiningToolConfig found for MiningToolType {inMiningToolType}</color>");
        return null;
    }

    public static CrystalData GetCrystalData(ECrystalType inCrystalType)
    {
        if (IN.crystalDataDict.TryGetValue(inCrystalType, out var crystalData))
        {
            return crystalData;
        }
        Debug.LogError($"<color=red>No CrystalData found for CrystalType {inCrystalType}</color>");
        return null;
    }

    public static Color GetCrystalColor(ECrystalType inCrystalType)
    {
        var crystalData = GetCrystalData(inCrystalType);

        if (crystalData != null)
            return crystalData.CrystalColor;

        return Color.white; // Default color if not found
    }

    public static string GetCrystalName(ECrystalType inCrystalType)
    {
        var crystalData = GetCrystalData(inCrystalType);

        if (crystalData != null)
            return crystalData.Name;

        return "Unknown Crystal"; // Default name if not found
    }

    public static float GetCrystalRadioactivityLevel(ECrystalType inCrystalType)
    {
        var crystalData = GetCrystalData(inCrystalType);

        if (crystalData != null)
            return crystalData.RadioactivityLevel;

        return 0f; // Default radioactivity level if not found
    }

    public static IconData GetShipIconSprite(int inIconIndex)
    {
        return IN.shipIconDatas[inIconIndex];
    }

    public static IconData GetReactorIconSprite(int inIconIndex)
    {
        return IN.reactorIconDatas[inIconIndex];
    }

    public static IconData GetBatteryIconSprite(int inIconIndex)
    {
        return IN.batteryIconDatas[inIconIndex];
    }

    public static IconData GetVaultIconSprite(int inIconIndex)
    {
        return IN.vaultIconDatas[inIconIndex];
    }
}