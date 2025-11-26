using System;
using UnityEngine;
using Sirenix.OdinInspector;
using static LootData;
using static CrystalData;
using System.Text;

[CreateAssetMenu(fileName = "Prog", menuName = "Data/ProgressConfig", order = 2)]
public class ProgressConfig : ScriptableObject
{
    [SerializeField, TextArea(0, 20), ReadOnly] private string lootSummary;

    [Space]
    public ProgressData ProgressData;

    #region OnValidate
    private void OnValidate()
    {
        int maxLootCount = 0, minLootCount = 0;

        int minEnergy = 0, maxEnergy = 0;
        int minHealth = 0, maxHealth = 0;
        int minRailRounds = 0, maxRailRounds = 0;
        int minMissiles = 0, maxMissiles = 0;
        int minCredits = 0, maxCredits = 0;

        int minEnergyLoot = 0, maxEnergyLoot = 0;
        int minHealthLoot = 0, maxHealthLoot = 0;
        int minRailRoundsLoot = 0, maxRailRoundsLoot = 0;
        int minMissilesLoot = 0, maxMissilesLoot = 0;
        int minCreditsLoot = 0, maxCreditsLoot = 0;

        int maxCrystals = 0, minCrystals = 0;
        int maxCrystalLoot = 0, minCrystalLoot = 0;

        int minGreenCrystals = 0, maxGreenCrystals = 0;
        int minBlueCrystals = 0, maxBlueCrystals = 0;
        int minRedCrystals = 0, maxRedCrystals = 0;
        int minYellowCrystals = 0, maxYellowCrystals = 0;
        int minPurpleCrystals = 0, maxPurpleCrystals = 0;
        int minBlackCrystals = 0, maxBlackCrystals = 0;

        int minGreenCrystalLoots = 0, maxGreenCrystalLoots = 0;
        int minBlueCrystalLoots = 0, maxBlueCrystalLoots = 0;
        int minRedCrystalLoots = 0, maxRedCrystalLoots = 0;
        int minYellowCrystalLoots = 0, maxYellowCrystalLoots = 0;
        int minPurpleCrystalLoots = 0, maxPurpleCrystalLoots = 0;
        int minBlackCrystalLoots = 0, maxBlackCrystalLoots = 0;

        foreach (var loot in ProgressData.RockLoot)
        {
            minLootCount += loot.QuantityMinMax.MinQuantity;
            maxLootCount += loot.QuantityMinMax.MaxQuantity;

            if (loot.LootData.LootType == ELootType.Energy)
            {
                minEnergyLoot += loot.QuantityMinMax.MinQuantity;
                maxEnergyLoot += loot.QuantityMinMax.MaxQuantity;
                minEnergy += loot.QuantityMinMax.MinQuantity * loot.QuantityMinMax.MinQuantity;
                maxEnergy += loot.QuantityMinMax.MaxQuantity * loot.QuantityMinMax.MaxQuantity;
            }
            else if (loot.LootData.LootType == ELootType.Health)
            {
                minHealthLoot += loot.QuantityMinMax.MinQuantity;
                maxHealthLoot += loot.QuantityMinMax.MaxQuantity;
                minHealth += loot.QuantityMinMax.MinQuantity * loot.QuantityMinMax.MinQuantity;
                maxHealth += loot.QuantityMinMax.MaxQuantity * loot.QuantityMinMax.MaxQuantity;
            }
            else if (loot.LootData.LootType == ELootType.Credits)
            {
                minCreditsLoot += loot.QuantityMinMax.MinQuantity;
                maxCreditsLoot += loot.QuantityMinMax.MaxQuantity;
                minCredits += loot.QuantityMinMax.MinQuantity * loot.QuantityMinMax.MinQuantity;
                maxCredits += loot.QuantityMinMax.MaxQuantity * loot.QuantityMinMax.MaxQuantity;
            }
            else if (loot.LootData.LootType == ELootType.RailRounds)
            {
                minRailRoundsLoot += loot.QuantityMinMax.MinQuantity;
                maxRailRoundsLoot += loot.QuantityMinMax.MaxQuantity;
                minRailRounds += loot.QuantityMinMax.MinQuantity * loot.QuantityMinMax.MinQuantity;
                maxRailRounds += loot.QuantityMinMax.MaxQuantity * loot.QuantityMinMax.MaxQuantity;
            }
            else if (loot.LootData.LootType == ELootType.Missiles)
            {
                minMissilesLoot += loot.QuantityMinMax.MinQuantity;
                maxMissilesLoot += loot.QuantityMinMax.MaxQuantity;
                minMissiles += loot.QuantityMinMax.MinQuantity * loot.QuantityMinMax.MinQuantity;
                maxMissiles += loot.QuantityMinMax.MaxQuantity * loot.QuantityMinMax.MaxQuantity;
            }
            else if (loot.LootData.LootType == ELootType.Crystals)
            {
                minCrystalLoot += loot.QuantityMinMax.MinQuantity;
                maxCrystalLoot += loot.QuantityMinMax.MaxQuantity;

                minCrystals += loot.LootData.QuantityMinMax.MinQuantity * loot.QuantityMinMax.MinQuantity;
                maxCrystals += loot.LootData.QuantityMinMax.MaxQuantity * loot.QuantityMinMax.MaxQuantity;

                switch (loot.LootData.CrystalType)
                {
                    case ECrystalType.Crystal4:
                        minGreenCrystals += loot.LootData.QuantityMinMax.MinQuantity * loot.QuantityMinMax.MinQuantity;
                        maxGreenCrystals += loot.LootData.QuantityMinMax.MaxQuantity * loot.QuantityMinMax.MaxQuantity;
                        minGreenCrystalLoots += loot.QuantityMinMax.MinQuantity;
                        maxGreenCrystalLoots += loot.QuantityMinMax.MaxQuantity;
                        break;
                    case ECrystalType.Crystal5:
                        minBlueCrystals += loot.LootData.QuantityMinMax.MinQuantity * loot.QuantityMinMax.MinQuantity;
                        maxBlueCrystals += loot.LootData.QuantityMinMax.MaxQuantity * loot.QuantityMinMax.MaxQuantity;
                        minBlueCrystalLoots += loot.QuantityMinMax.MinQuantity;
                        maxBlueCrystalLoots += loot.QuantityMinMax.MaxQuantity;
                        break;
                    case ECrystalType.Crystal1:
                        minRedCrystals += loot.LootData.QuantityMinMax.MinQuantity * loot.QuantityMinMax.MinQuantity;
                        maxRedCrystals += loot.LootData.QuantityMinMax.MaxQuantity * loot.QuantityMinMax.MaxQuantity;
                        minRedCrystalLoots += loot.QuantityMinMax.MinQuantity;
                        maxRedCrystalLoots += loot.QuantityMinMax.MaxQuantity;
                        break;
                    case ECrystalType.Crystal3:
                        minYellowCrystals += loot.LootData.QuantityMinMax.MinQuantity * loot.QuantityMinMax.MinQuantity;
                        maxYellowCrystals += loot.LootData.QuantityMinMax.MaxQuantity * loot.QuantityMinMax.MaxQuantity;
                        minYellowCrystalLoots += loot.QuantityMinMax.MinQuantity;
                        maxYellowCrystalLoots += loot.QuantityMinMax.MaxQuantity;
                        break;
                    case ECrystalType.Crystal6:
                        minPurpleCrystals += loot.LootData.QuantityMinMax.MinQuantity * loot.QuantityMinMax.MinQuantity;
                        maxPurpleCrystals += loot.LootData.QuantityMinMax.MaxQuantity * loot.QuantityMinMax.MaxQuantity;
                        minPurpleCrystalLoots += loot.QuantityMinMax.MinQuantity;
                        maxPurpleCrystalLoots += loot.QuantityMinMax.MaxQuantity;
                        break;
                    case ECrystalType.Crystal7:
                        minBlackCrystals += loot.LootData.QuantityMinMax.MinQuantity * loot.QuantityMinMax.MinQuantity;
                        maxBlackCrystals += loot.LootData.QuantityMinMax.MaxQuantity * loot.QuantityMinMax.MaxQuantity;
                        minBlackCrystalLoots += loot.QuantityMinMax.MinQuantity;
                        maxBlackCrystalLoots += loot.QuantityMinMax.MaxQuantity;
                        break;
                }
            }
        }

        var summary = new StringBuilder();

        if (minLootCount > 0 || maxLootCount > 0)
            summary.AppendLine($"Loot items: {minLootCount}-{maxLootCount}");

        if (minEnergy > 0 || maxEnergy > 0)
            summary.AppendLine($"   Energy: {minEnergy}-{maxEnergy}  ({minEnergyLoot}-{maxEnergyLoot} loot items)");

        if (minHealth > 0 || maxHealth > 0)
            summary.AppendLine($"   Health: {minHealth}-{maxHealth}  ({minHealthLoot}-{maxHealthLoot} loot items)");

        if (minCredits > 0 || maxCredits > 0)
            summary.AppendLine($"   Credits: {minCredits}-{maxCredits}  ({minCreditsLoot}-{maxCreditsLoot} loot items)");

        if (minRailRounds > 0 || maxRailRounds > 0)
            summary.AppendLine($"   Rail Rounds: {minRailRounds}-{maxRailRounds}  ({minRailRoundsLoot}-{maxRailRoundsLoot} loot items)");

        if (minMissiles > 0 || maxMissiles > 0)
            summary.AppendLine($"   Missiles: {minMissiles}-{maxMissiles}  ({minMissilesLoot}-{maxMissilesLoot} loot items)");

        if (minCrystals > 0 || maxCrystals > 0)
            summary.AppendLine($"   Crystals: {minCrystals}-{maxCrystals}  ({minCrystalLoot}-{maxCrystalLoot} loot items)");

        if (minGreenCrystals > 0 || maxGreenCrystals > 0)
            summary.AppendLine($"      Green: {minGreenCrystals}-{maxGreenCrystals}  ({minGreenCrystalLoots}-{maxGreenCrystalLoots} loot items)");

        if (minBlueCrystals > 0 || maxBlueCrystals > 0)
            summary.AppendLine($"      Blue: {minBlueCrystals}-{maxBlueCrystals}  ({minBlueCrystalLoots}-{maxBlueCrystalLoots} loot items)");

        if (minRedCrystals > 0 || maxRedCrystals > 0)
            summary.AppendLine($"      Red: {minRedCrystals}-{maxRedCrystals}  ({minRedCrystalLoots}-{maxRedCrystalLoots} loot items)");

        if (minYellowCrystals > 0 || maxYellowCrystals > 0)
            summary.AppendLine($"      Yellow: {minYellowCrystals}-{maxYellowCrystals}  ({minYellowCrystalLoots}-{maxYellowCrystalLoots} loot items)");

        if (minPurpleCrystals > 0 || maxPurpleCrystals > 0)
            summary.AppendLine($"      Purple: {minPurpleCrystals}-{maxPurpleCrystals}  ({minPurpleCrystalLoots}-{maxPurpleCrystalLoots} loot items)");

        if (minBlackCrystals > 0 || maxBlackCrystals > 0)
            summary.AppendLine($"      Black: {minBlackCrystals}-{maxBlackCrystals}  ({minBlackCrystalLoots}-{maxBlackCrystalLoots} loot items)");

        this.lootSummary = summary.ToString();
    }
    #endregion
}

[Serializable]
public class ProgressData
{
    [Header("Criteria to Unlock")]
    public int MapNum;//TODO: tie this to criteria
    public string CriteriaToComplete;//TODO: make this an enum or flags
    // combination of number of crystals, etc. gathered, item bought in shop, level reached, etc.

    [Header("For weighted random, add multiples of same kind")]
    public ProgressLootData[] RockLoot;//you could add one LootData - Crystals with 2-10 green, and another with 10-15 green, etc.
}

[Serializable]
public class ProgressLootData
{
    public LootData LootData;

    [Header("Number of rocks to spawn this loot in")]
    public WeightedRandom QuantityMinMax;
}