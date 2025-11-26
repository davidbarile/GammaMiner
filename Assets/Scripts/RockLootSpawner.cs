using System;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rock))]
public class RockLootSpawner : MonoBehaviour
{
    [Header("(If not 1, will be overridden by RockCluster)")]
    [Range(0, 20)] public int LootProbabilityWeight = 1;

    [Space] public LootData LootData;

    public LootEntity LootEntity { get; private set; }

    [Header("Set by LootData"), ReadOnly]
    public int Quantity = -1;
    public int MaxQuantity = -1;

    [SerializeField] private SpriteRenderer lootProbabilityOverlay;

    [SerializeField] private Color[] lootProbabilityColors;
    
    private MiningToolConfig miningToolConfig;

    private void OnValidate()
    {
        if (this.lootProbabilityOverlay == null)
            return;

        int index = Mathf.Clamp(this.LootProbabilityWeight, 0, this.lootProbabilityColors.Length - 1);
        this.lootProbabilityOverlay.color = this.lootProbabilityColors[index];
    }

    public static LootEntity SpawnLoot(string inPrefabName)
    {
        return Pool.Spawn<LootEntity>(inPrefabName);
    }

    public void InitLoot(Rock inRock, bool isReadyToCollect, Action inOnLootCollected)
    {
        var loot = RockLootSpawner.SpawnLoot(this.LootData.PrefabName);
        this.LootEntity = loot;
        loot.name = $"{this.LootData.PrefabName}_{inRock.name}";

        loot.transform.SetParent(this.transform);
        loot.transform.localScale = 2 * Vector3.one / this.transform.parent.localScale.x;
        loot.transform.position = this.transform.position;

        var initPos = this.transform.position;

        loot.Configure(this.LootData, this.Quantity, inOnLootCollected);
        loot.SetCollectible(isReadyToCollect);

        if (isReadyToCollect)
            loot.transform.SetParent(this.transform.parent);

        if(isReadyToCollect)
            Invoke(nameof(ReparentLoot), .5f);
    }

    private void ReparentLoot()
    {
        this.LootEntity.transform.SetParent(LootManager.IN.LootContainer);
        this.LootEntity.transform.position = this.transform.position;
    }

    public void OnRockDestroy(HealthEntity inHealthEntity)
    {
        if (this.LootEntity == null) return;

        if(this.miningToolConfig != null)
            this.LootEntity.OverrideConfigure(this.miningToolConfig);

        this.LootEntity.SetCollectible(true);
    }

    public void SetMiningToolData(MiningToolConfig inConfig)
    {
        if (this.LootEntity == null || this.LootEntity.LootData == null) return;

        this.miningToolConfig = inConfig;

        if (inConfig == null) return;
    }
}