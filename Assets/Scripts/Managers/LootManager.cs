using System;
using System.Collections.Generic;
using UnityEngine;
using static CrystalData;
using static LootData;
using static RockData;

public class LootManager : MonoBehaviour
{
    [Serializable]
    public class LootInMapData
    {
        public int NumRocksWithLoot;
        public int TotalRocksWithLoot;
        public int NumLoots;
        public int TotalLoots;
    }

    public static LootManager IN;

    public static Action<int, bool, ELootType, ECrystalType> OnLootCollected;

    [Range(0f, 1f)] public float DebugLootSpriteAlpha;
    [Range(0f, 20f), SerializeField] private float lootMultiplier = 1f;
    public bool DebugShowHealthText;

    public LootData[] AllLootDatas;

    public Transform LootContainer { get; private set; }

    private List<LootData> allLoots = new();

    private List<Rock> allRocksInMap = new();
    private List<Rock> lootableRocksInMap = new();
    public List<Rock> RocksWithLoot { get; private set; } = new();

    public Dictionary<ELootType, LootInMapData> LootsInMap { get; private set; } = new();
    public Dictionary<ECrystalType, LootInMapData> CrystalLootsInMap { get; private set; } = new();

    private void Awake()
    {
        var go = new GameObject("Loot Container");
        DontDestroyOnLoad(go);
        this.LootContainer = go.transform;
    }

    private void Start()
    {
        LootManager.OnLootCollected += HandleLootCollected;
    }

    private void OnDestroy()
    {
        LootManager.OnLootCollected -= HandleLootCollected;
    }

    private void InitLevelLootDicts()
    {
        this.LootsInMap.Clear();
        this.CrystalLootsInMap.Clear();

        InitElementInLootDict(ELootType.Health);
        InitElementInLootDict(ELootType.Energy);
        InitElementInLootDict(ELootType.RailRounds);
        InitElementInLootDict(ELootType.Missiles);
        InitElementInLootDict(ELootType.Credits);
        InitElementInLootDict(ELootType.Collectible);
        InitElementInLootDict(ELootType.Armor);
        InitElementInLootDict(ELootType.ShieldBuff);
        InitElementInLootDict(ELootType.TurningBuff);
        InitElementInLootDict(ELootType.Other);

        InitElementInCrystalDict(ECrystalType.Crystal1);
        InitElementInCrystalDict(ECrystalType.Crystal2);
        InitElementInCrystalDict(ECrystalType.Crystal3);
        InitElementInCrystalDict(ECrystalType.Crystal4);
        InitElementInCrystalDict(ECrystalType.Crystal5);
        InitElementInCrystalDict(ECrystalType.Crystal6);
        InitElementInCrystalDict(ECrystalType.Crystal7);
        InitElementInCrystalDict(ECrystalType.Crystal8);

        void InitElementInLootDict(ELootType inKey)
        {
            this.LootsInMap.Add(inKey, new LootInMapData
            {
                NumRocksWithLoot = 0,
                TotalRocksWithLoot = 0,
                NumLoots = 0,
                TotalLoots = 0
            });
        }

        void InitElementInCrystalDict(ECrystalType inKey)
        {
            this.CrystalLootsInMap.Add(inKey, new LootInMapData
            {
                NumRocksWithLoot = 0,
                TotalRocksWithLoot = 0,
                NumLoots = 0,
                TotalLoots = 0
            });
        }
    }

    public void InitRockLootDataFromProgressData(ProgressData inProgressData)
    {
        int level = inProgressData.MapNum;

        this.allRocksInMap = new List<Rock>(TileLoadingManager.IN.AllRocksInLevel);//maybe use AllActiveRocksInLevel

        // allRocksInMap.Sort((a, b) =>
        // {
        //     float distA = Vector3.Distance(a.transform.position, TileLoadingManager.PlayerPosition);
        //     float distB = Vector3.Distance(b.transform.position, TileLoadingManager.PlayerPosition);
        //     return distA.CompareTo(distB);
        // });

        //this.allRocksInMap.RandomizeList();

        this.allLoots.Clear();
        this.RocksWithLoot.Clear();
        this.lootableRocksInMap.Clear();

        for (var mult = 0; mult < this.lootMultiplier; mult++)
        {
            foreach (var progressLootData in inProgressData.RockLoot)
            {
                var quantity = progressLootData.QuantityMinMax.GetWeightedRandomQuantity();
                for (int i = 0; i < quantity; i++)
                {
                    this.allLoots.Add(progressLootData.LootData);
                }
            }
        }

        var allLootsCount = this.allLoots.Count;

        var weightedRocksInLevel = new List<Rock>();

        foreach (var rock in this.allRocksInMap)
        {
            if (rock.RockData == null ||
                rock.RockData.LootType == ELootType.None ||
                rock.RockData.RockType == ERockType.Unbreakable)
                continue;

            int numTimesToAddToList = Mathf.RoundToInt(rock.LootSpawner.LootProbabilityWeight);

            if (numTimesToAddToList > 1)
                numTimesToAddToList *= 2;

            for (int i = 0; i < numTimesToAddToList; i++)
            {
                weightedRocksInLevel.Add(rock);
            }

            this.lootableRocksInMap.Add(rock);
        }

        var numWeightedRocks = weightedRocksInLevel.Count;

        weightedRocksInLevel.RandomizeList();

        var lootIndex = 0;
        var rockIndex = 0;

        //fill rocks with loot
        while (this.allLoots.Count > 0 && weightedRocksInLevel.Count > 0)
        {
            var lootData = this.allLoots[lootIndex];
            var rock = weightedRocksInLevel[rockIndex];

            while (rock.HasLootAssigned && weightedRocksInLevel.Count > 1)
            {
                //find next rock that doesn't have loot assigned
                weightedRocksInLevel.RemoveAt(rockIndex);
                rockIndex = Mathf.Clamp(rockIndex, 0, weightedRocksInLevel.Count - 1);
                rock = weightedRocksInLevel[rockIndex];
            }

            if (weightedRocksInLevel.Count == 0 || (weightedRocksInLevel.Count == 1 && rock.HasLootAssigned))
                break;

            //here we need to check if rock can accept this loot type (crystals)
            //if not, do we skip to next rock / next loot

            var isValidLoot = rock.RockData.LootType.HasFlag(lootData.LootType);
            var isCrystal = lootData.LootType == ELootType.Crystals;
            var isValidCrystal = isCrystal && rock.RockData.CrystalType.HasFlag(lootData.CrystalType);

            if (!isValidLoot || (isCrystal && !isValidCrystal))
            {
                //try next rock
                if (lootIndex < this.allLoots.Count - 1)
                    lootIndex++;
                else
                {
                    if (rockIndex < weightedRocksInLevel.Count - 1)
                        rockIndex++;
                    else
                    {
                        //we've tried all rocks and loots
                        Debug.LogError($"Ran out of valid rocks for loots in level {level}");
                        break;
                    }
                }
                continue;
            }

            int quantity = lootData.GetWeightedRandomQuantity();
            rock.AddLootToSaveData(lootData, quantity);
            rock.HasLootAssigned = true;//flag it so we don't assign more than one loot item to a rock
            this.RocksWithLoot.Add(rock);

            //AddLootToDict(lootData, quantity);

            weightedRocksInLevel.RemoveAt(rockIndex);
            this.allLoots.RemoveAt(lootIndex);

            lootIndex = 0;
            rockIndex = 0;
        }

        if (allLootsCount > this.lootableRocksInMap.Count)
            Debug.LogError($"Too much loot to fill all rocks in level {level} - {this.lootableRocksInMap.Count} lootable rocks but {allLootsCount} loot items.  Remainging loots = {this.allLoots.Count}");

        Debug.Log($"Setting loot for level {level}   this.rocksWithLoot = {this.RocksWithLoot.Count}/{this.lootableRocksInMap.Count}    allLoots = {allLootsCount}  Remainging loots = {this.allLoots.Count}      {this.allRocksInMap.Count} rocks in level       weightedRocksInLevel = {numWeightedRocks}");
    }

    public void SetLevelLootDataFromRocks()
    {
        InitLevelLootDicts();

        foreach (var rock in TileLoadingManager.IN.AllRocksInLevel)
        {
            if (rock.RockData == null ||
                rock.RockData.LootType == ELootType.None ||
                rock.RockData.RockType == ERockType.Unbreakable)
                continue;

            if (rock.LootSpawner.LootData != null)
            {
                if (rock.LootSpawner.LootData.LootType != ELootType.Crystals)
                {
                    AddToLootDict(rock.LootSpawner.LootData.LootType, rock.LootSpawner.Quantity, rock.LootSpawner.MaxQuantity, rock.IsVisible);
                }
                else
                {
                    AddToCrystalDict(rock.LootSpawner.LootData.CrystalType, rock.LootSpawner.Quantity, rock.LootSpawner.MaxQuantity, rock.IsVisible);
                }
            }
        }

        void AddToLootDict(ELootType inKey, int inQuantity, int inMaxQuantity, bool inIsRockVisible)
        {
            if (!this.LootsInMap.ContainsKey(inKey))
            {
                this.LootsInMap.Add(inKey, new LootInMapData
                {
                    NumRocksWithLoot = 0,
                    TotalRocksWithLoot = 0,
                    NumLoots = 0,
                    TotalLoots = 0
                });
            }

            this.LootsInMap[inKey].NumLoots += inQuantity;
            this.LootsInMap[inKey].TotalLoots += inMaxQuantity;
            if(inIsRockVisible)
                this.LootsInMap[inKey].NumRocksWithLoot++;
            this.LootsInMap[inKey].TotalRocksWithLoot++;
        }

        void AddToCrystalDict(ECrystalType inKey, int inQuantity, int inMaxQuantity, bool inIsRockVisible)
        {
            if (!this.CrystalLootsInMap.ContainsKey(inKey))
            {
                this.CrystalLootsInMap.Add(inKey, new LootInMapData
                {
                    NumRocksWithLoot = 0,
                    TotalRocksWithLoot = 0,
                    NumLoots = 0,
                    TotalLoots = 0
                });
            }
            this.CrystalLootsInMap[inKey].NumLoots += inQuantity;
            this.CrystalLootsInMap[inKey].TotalLoots += inMaxQuantity;
            if (inIsRockVisible)
                this.CrystalLootsInMap[inKey].NumRocksWithLoot++;
            this.CrystalLootsInMap[inKey].TotalRocksWithLoot++;
        }

        //init HUD display
        HUD.OnLootInLevelDataChanged?.Invoke(ELootType.None, this.LootsInMap[ELootType.Health], this.LootsInMap);
        HUD.OnCrystalLootInLevelDataChanged?.Invoke(ECrystalType.None, this.CrystalLootsInMap[ECrystalType.Crystal1], this.CrystalLootsInMap);
    }

    /// <summary>
    /// fired from LootEntity event when collected
    /// </summary>
    private void HandleLootCollected(int inQuantity, bool inIsRockDepleted, ELootType inLootType, ECrystalType inCrystalType = ECrystalType.None)
    {
        if (inLootType == ELootType.Crystals)
        {
            var lootInLevelData = this.CrystalLootsInMap[inCrystalType];
            if (lootInLevelData != null)
            {
                lootInLevelData.NumLoots -= inQuantity;

                if (inIsRockDepleted)
                    lootInLevelData.NumRocksWithLoot--;

                HUD.OnCrystalLootInLevelDataChanged?.Invoke(inCrystalType, this.CrystalLootsInMap[inCrystalType], this.CrystalLootsInMap);
            }
        }
        else
        {
            var lootInLevelData = this.LootsInMap[inLootType];
            if (lootInLevelData != null)
            {
                lootInLevelData.NumLoots -= inQuantity;

                if (inIsRockDepleted)
                    lootInLevelData.NumRocksWithLoot--;

                HUD.OnLootInLevelDataChanged?.Invoke(inLootType, this.LootsInMap[inLootType], this.LootsInMap);
            }
        }
    }

    /// <summary>
    /// Adds loot to the appropriate dictionary for tracking purposes
    /// </summary>
    // private void AddLootToDict(LootData inLootData, int inQuantity)
    // {
    //     LootInMapData lootInLevelData = null;
    //     if (inLootData.LootType == ELootType.Crystals)
    //     {
    //         lootInLevelData = this.CrystalLootsInMap[inLootData.CrystalData.CrystalType];
    //     }
    //     else
    //     {
    //         lootInLevelData = this.LootsInMap[inLootData.LootType];
    //     }

    //     if (lootInLevelData != null)
    //     {
    //         lootInLevelData.NumRocksWithLoot++;
    //         lootInLevelData.TotalRocksWithLoot++;
    //         lootInLevelData.NumLoots += inQuantity;
    //         lootInLevelData.TotalLoots += inQuantity;
    //     }
    // }
}