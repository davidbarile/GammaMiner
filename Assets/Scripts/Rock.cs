using System;
using UnityEngine;
using DG.Tweening;
using static Explosion;
using static HealthEntity;

public class Rock : MonoBehaviour
{
    public string ID;
    public bool IsVisible = true;//this is a hack because hidden prefabs are reappearing

    public bool DrawingChangedThisFrame { get; set; }

    public bool HasLootAssigned;

    public SpriteRenderer Outline => this.outline;
    [SerializeField] private SpriteRenderer outline;

    public SpriteRenderer Fill => this.fill;
    [SerializeField] private SpriteRenderer fill;

    public RockLootSpawner LootSpawner;
    public RockColorAnim RockColorAnim;
    public HealthEntity HealthEntity;

    public RockData RockData;

    public float BaseRockHealthOverride = -1;
    public bool IsAnimatedTexture;

    public Explosion ExplosionPrefab;
    public int ExplosionBaseDamage;

    private float outlineWidth;

    private RockSaveData saveData = null;

    public void SetAlpha(float inAlpha)
    {
        if (this.Fill)
        {
            if (inAlpha == 0)
                this.Fill.color = new Color(this.Fill.color.r, this.Fill.color.g, this.Fill.color.b, .2f);
            else
                this.Fill.color = new Color(this.Fill.color.r, this.Fill.color.g, this.Fill.color.b, inAlpha);

            TileEditorTool.SetDirty(this.Fill);
        }

        if (this.Outline)
        {
            this.Outline.color = new Color(this.Outline.color.r, this.Outline.color.g, this.Outline.color.b, inAlpha);
            TileEditorTool.SetDirty(this.Outline);
        }

        TileEditorTool.SetDirty(this);
    }

    public void Rename(string inNewName)
    {
        this.name = inNewName;

        TileEditorTool.SetDirty(this);
        TileEditorTool.SetDirty(this.gameObject);
    }

    public void SetFillColor(Color inColor)
    {
        if (this.Fill)
        {
            this.Fill.color = inColor;
            TileEditorTool.SetDirty(this.Fill);
        }

        TileEditorTool.SetDirty(this);
    }

    public void SetOutlineColor(Color inColor)
    {
        if (this.Outline)
        {
            this.Outline.color = inColor;
            TileEditorTool.SetDirty(this.Outline);
        }

        TileEditorTool.SetDirty(this);
    }

    private void Start()
    {
        this.gameObject.SetActive(this.IsVisible);

        if (!TileEditorTool.IsEditing) return;

        if (this.RockColorAnim != null)
            this.RockColorAnim.enabled = this.IsAnimatedTexture;
    }

    private void OnValidate()
    {
        // this.fill = this.transform.GetChild(0).GetComponent<SpriteRenderer>();
        // this.shape = this.GetComponent<ProtoShape2D>();
        this.HealthEntity = this.GetComponent<HealthEntity>();
        this.LootSpawner = this.GetComponent<RockLootSpawner>();
        this.RockColorAnim = this.GetComponent<RockColorAnim>();
    }

    private void OnEnable()
    {
        if (!this.HealthEntity)
            this.HealthEntity = this.GetComponent<HealthEntity>();

        this.HealthEntity.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        this.HealthEntity.OnHealthChanged -= HandleHealthChanged;
    }

    public void ApplyRockData(RockData inRockData)
    {
        //Debug.Log($"ApplyRockData({inRockData})   this = {name}");

        this.RockData = inRockData;

        this.RockColorAnim.enabled = inRockData.IsAnimatedTexture;

        this.IsAnimatedTexture = inRockData.IsAnimatedTexture;

        if (inRockData.IsAnimatedTexture)
        {
            this.RockColorAnim.ApplyRockData(inRockData);
            TileEditorTool.SetDirty(this.RockColorAnim);

            SetFillColor(inRockData.RockAnimColors[0]);
            SetOutlineColor(inRockData.RockOutlineColors[0]);
        }
    }

    public void InitHealth(Explosion inExplosionPrefab = null, int inExplosionBaseDamage = 0, float inClusterBaseRockHealth = -1, bool shouldUseRigidbodyAutoMass = true)
    {
        float multiplier = 1;

        if (shouldUseRigidbodyAutoMass)
        {
            var rb = this.GetComponent<Rigidbody2D>();
            rb.useAutoMass = true;
            multiplier = rb.mass;
            //Debug.Log($"{rock.name} mass = {rb.mass}");
        }

        float rockHealth = 0f;

        if (this.RockData != null)
            rockHealth = this.RockData.BaseHealth;
        else
            rockHealth = this.BaseRockHealthOverride == -1 ? inClusterBaseRockHealth : this.BaseRockHealthOverride;

        int rockHealthInt = rockHealth == 0 ? 0 : Mathf.CeilToInt(rockHealth * multiplier);

        if(this.RockData != null && this.RockData.RockType == RockData.ERockType.Unbreakable)
        {
            this.HealthEntity.Init(1000, 1000);
            this.HealthEntity.enabled = false;
            return;
        }

        this.HealthEntity.DestroyMode = EDestroyMode.HideObject;
        this.HealthEntity.Init(rockHealthInt, rockHealthInt);

        this.HealthEntity.enabled = rockHealthInt != 0;

        if (this.ExplosionPrefab == null && inExplosionPrefab != null)
        {
            this.ExplosionPrefab = inExplosionPrefab;
            this.ExplosionBaseDamage = inExplosionBaseDamage;
        }

        if (this.ExplosionPrefab != null || inExplosionPrefab != null)
            this.HealthEntity.OnDie += this.Explode;

        if (this.RockData != null)
        {
            if (this.RockData.DoesDamageOnImpact)
            {
                var impactDamage = this.TryGetComponent<DamageEntity_Collision>(out var existingDamage) ? existingDamage : this.gameObject.AddComponent<DamageEntity_Collision>();
                impactDamage.Damage = Mathf.RoundToInt(this.RockData.ImpactDamageBase);
                impactDamage.DamagePerSecond = Mathf.RoundToInt(this.RockData.ImpactDamagePerSecondBase);
            }
            else
            {
                if (this.TryGetComponent<DamageEntity_Collision>(out var existingDamage))
                    Destroy(existingDamage);
            }
        }
    }

    public void Explode(HealthEntity inHealthEntity = null)
    {
        if (this.ExplosionPrefab == null)
        {
            var cluster = this.GetComponentInParent<RockCluster>();
            if (cluster != null)
            {
                this.ExplosionPrefab = cluster.ExplosionPrefab;
                this.ExplosionBaseDamage = cluster.ExplosionBaseDamage;
            }
        }

        if (this.ExplosionPrefab == null)
            return;

        var explosion = Pool.Spawn<Explosion>(this.ExplosionPrefab.name, GameManager.IN.ProjectilesContainer, this.transform.position, Quaternion.identity);

        //Debug.Log($"ROCK: {this.name} explosion = {explosion}   this.transform.parent = {this.transform.parent.name}     this.Fill = {this.Fill}   this.Shape = {this.Shape}", this.gameObject);

        if (this.Fill)
            explosion.SetParticleColors(this.Fill.color);

        explosion.Explode(this.ExplosionBaseDamage, null, false, 1, true, EExplosionIgnore.Rock);

        NavMeshManager.IN.FlagNavMeshForRebuild();
    }

    public void ApplySaveData(RockSaveData inSaveData)
    {
        this.IsVisible = inSaveData.Health > 0;

        this.HealthEntity.ApplySaveData(inSaveData.Health);//this hides if dead

        this.LootSpawner.enabled = inSaveData.LootData != null;//keep it enabled if there is loot data, even if quantity is 0

        if (inSaveData.LootData != null)
        {
            this.LootSpawner.LootData = inSaveData.LootData;
            this.LootSpawner.Quantity = inSaveData.LootQuantity;
            this.LootSpawner.MaxQuantity = inSaveData.MaxLootQuantity;

            if (inSaveData.LootQuantity > 0)
            {
                this.LootSpawner.InitLoot(this, this.HealthEntity.IsDead, () =>
                {
                    inSaveData.LootQuantity = 0;
                });
            }
        }
    }

    private void HandleHealthChanged(int inHealthValue)
    {
        if (this.saveData == null)
        {
            //first time getting here this game session, get or create save data
            if (MapProgressData.Data.CurrentMapSaveData.DirtyRocks.TryGetValue(this.ID, out this.saveData))
            {
                this.saveData.Health = this.HealthEntity.Health;
                this.saveData.RockName = this.name;
                if (this.HealthEntity.Health <= 0 && this.saveData.TimeMined == -1)
                    this.saveData.TimeMined = Utils.NOW;
            }
            else
            {
                //not found, so create new save data
                this.saveData = new RockSaveData()
                {
                    Health = this.HealthEntity.Health,
                    RockName = this.name,
                };
                MapProgressData.Data.CurrentMapSaveData.DirtyRocks.Add(this.ID, this.saveData);
            }
        }
        else
        {
            //already have save data, just update it
            this.saveData.Health = this.HealthEntity.Health;
            this.saveData.RockName = this.name;

            if (this.HealthEntity.Health <= 0 && this.saveData.TimeMined == -1)
                this.saveData.TimeMined = Utils.NOW;
        }
    }

    public void AddLootToSaveData(LootData inLootData, int inQuantity)
    {
        if (inLootData == null)
        {
            Debug.LogError($"Rock.AddLootToSaveData()  inLootData = NULL   inQuantity = {inQuantity}", this);
            return;
        }

        if (this.saveData == null)
        {
            //first time getting here this game session, get or create save data
            if (MapProgressData.Data.CurrentMapSaveData.DirtyRocks.TryGetValue(this.ID, out this.saveData))
            {
                this.saveData.LootData = inLootData;
                this.saveData.LootQuantity = inQuantity;
            }
            else
            {
                //not found, so create new save data
                this.saveData = new RockSaveData()
                {
                    Health = this.HealthEntity.Health,
                    RockName = this.name,
                    LootData = inLootData,
                    LootQuantity = inQuantity,
                    MaxLootQuantity = inQuantity
                };
                MapProgressData.Data.CurrentMapSaveData.DirtyRocks.Add(this.ID, this.saveData);
            }
        }
        else
        {
            //already have save data, just update it
            this.saveData.LootData = inLootData;
            this.saveData.LootQuantity = inQuantity;
        }
    }

    public void Revive()
    {
        this.IsVisible = true;
        this.gameObject.SetActive(true);

        this.HealthEntity.Health = 0;
        this.HealthEntity.AddHealth(this.HealthEntity.MaxHealth);
       
        var originalScale = this.HealthEntity.transform.localScale;
        this.HealthEntity.transform.localScale = Vector3.zero;
        this.HealthEntity.transform.DOScale(originalScale, .7f).SetEase(Ease.OutElastic);
        //TODO: reset loot

        if (this.saveData == null)
        {
            //first time getting here this game session, get or create save data
            if (MapProgressData.Data.CurrentMapSaveData.DirtyRocks.TryGetValue(this.ID, out this.saveData))
            {
                this.saveData.Health = this.HealthEntity.MaxHealth;
                this.saveData.TimeMined = -1;
            }
            else
            {
                //not found, so create new save data
                this.saveData = new RockSaveData()
                {
                    Health = this.HealthEntity.Health,
                    RockName = this.name,
                };
                MapProgressData.Data.CurrentMapSaveData.DirtyRocks.Add(this.ID, this.saveData);
                
                Debug.LogError($"Rock.Revive()  but could not find existing save data to update!", this);
            }
        }
        else
        {
            //already have save data, just update it
            this.saveData.Health = this.HealthEntity.MaxHealth;
            this.saveData.TimeMined = -1;
        }

        if(this.LootSpawner.LootData != null)
        {
            if (this.LootSpawner.LootEntity != null)
                Pool.Despawn(this.LootSpawner.LootEntity.gameObject);
                
            this.LootSpawner.InitLoot(this, false, () =>
            {
                this.saveData.LootQuantity = 0;
            });
        }

        ApplySaveData(this.saveData);
    }
}