using System;
using TMPro;
using UnityEngine;
using DG.Tweening;
using static LootData;

[RequireComponent(typeof(Collider2D))]
public class LootEntity : MonoBehaviour
{
    public LootData LootData;

    [Header("If -1, Set by LootData")]
    public int Quantity;

    [Space]
    [SerializeField] private Collider2D circleCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject crystalsOverlay;
    [SerializeField] private TextMeshPro text;
    [SerializeField] private ParticleSystem blobParticle;

    private int amountCollected;

    private Action onCollected;
    private Vector3 initScale = Vector3.zero;

    public void Configure(LootData inLootdData, int inQuantity, Action inOnCollected)
    {
        this.LootData = inLootdData;
        this.Quantity = inQuantity;
        this.onCollected = inOnCollected;

        SetText(this.Quantity.ToString());

        var ps = this.blobParticle.main;

        if (this.LootData.LootType == ELootType.Crystals)
        {
            var crystalColor = GlobalData.GetCrystalColor(this.LootData.CrystalType);
            this.spriteRenderer.color = crystalColor;

            ps.startColor = crystalColor;
        }
        else if (this.LootData.LootType == ELootType.Energy)
        {
            ps.startColor = Color.cyan;
        }

        var clampedSize = (float)this.Quantity / (float)(inLootdData.QuantityMinMax.MaxQuantity * inLootdData.IncrementSize);
        ps.startSize = Mathf.Lerp(1f, 5f, clampedSize);
    }
    
    public void OverrideConfigure(MiningToolConfig inConfig)
    {
        if (inConfig == null || this.LootData == null)
            return;

        int newQuantity = 0;
        float yieldFactor = 0f;

        var probability = inConfig.GetLootProbabilityFactor(this.LootData.LootType, this.LootData.CrystalType);

        if(probability > 0f)
        {
            if (UnityEngine.Random.value < probability)
            {
                yieldFactor = inConfig.GetLootYieldFactor(this.LootData.LootType, this.LootData.CrystalType);
                newQuantity = Mathf.RoundToInt(this.Quantity * yieldFactor);
            }
        }

        print($"DTB: LootEntity.OverrideConfigure()   Original Quantity = {this.Quantity}    New Quantity = {newQuantity}   yieldFactor = {yieldFactor}  probability = {probability}   LootType = {this.LootData.LootType}   CrystalType = {this.LootData.CrystalType}");

        Configure(this.LootData, newQuantity, this.onCollected);
    }

    private void SetText(string inText)
    {
        this.text.text = inText;
    }

    private void SetAlpha(float inAlpha)
    {
        var sr = this.spriteRenderer;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, inAlpha);
    }

    public void SetCollectible(bool inIsCollectible)
    {
        if (this.Quantity <= 0)
        {
            Pool.Despawn(this.gameObject);
            this.onCollected?.Invoke();//sets inSaveData.LootQuantity = 0;
            return;
        }
        
        this.circleCollider.enabled = inIsCollectible;

        if (this.LootData.LootType == ELootType.Crystals)
        {
            var alpha = inIsCollectible ? 1 : 0;
            SetAlpha(alpha);

            //TODO: default hide, show some particle depending on sensors
            this.blobParticle.gameObject.SetActive(!inIsCollectible);
            this.crystalsOverlay.SetActive(inIsCollectible);
        }
        else
        {
            if (this.LootData.LootType == ELootType.Energy)
                this.blobParticle.gameObject.SetActive(!inIsCollectible);

            //TODO: default hide, show some particle depending on sensors
            var alpha = inIsCollectible ? 1 : LootManager.IN.DebugLootSpriteAlpha;
            SetAlpha(alpha);

            this.blobParticle.gameObject.SetActive(false);
            this.crystalsOverlay.SetActive(false);
        }

        //this.text.gameObject.SetActive(inIsCollectible);

        if (inIsCollectible)
            this.transform.SetParent(LootManager.IN.LootContainer);
    }

    private void OnTriggerEnter2D(Collider2D inOther)
    {
        var ship = inOther.gameObject.GetComponentInParent<SpaceShip>();

        var isShip = ship != null;
        var isRailRound = inOther.gameObject.GetComponent<RailRound>() != null;
        var isLaser = inOther.gameObject.GetComponent<Laser>() != null;

        var isPlayer = inOther.gameObject.CompareTag("Player");

        if (isShip)
        {
            switch (this.LootData.PickupPermissions)
            {
                case ELootPickupPermissions.None:
                    return;

                case ELootPickupPermissions.Player:
                    if (!isPlayer)
                        return;
                    break;

                case ELootPickupPermissions.Enemy:
                    if (isPlayer)
                        return;
                    break;
            }
        }
        else
        {
            switch (this.LootData.PickupMode)
            {
                case ELootPickupMode.AllProjectiles:
                    if (!isRailRound && !isLaser) return;
                    break;

                case ELootPickupMode.RailRounds:
                    if (!isRailRound) return;
                    break;

                case ELootPickupMode.Lasers:
                    if (!isLaser) return;
                    break;

                case ELootPickupMode.ShipCollision:
                    if (!isShip) return;
                    break;

                default:
                    break;
            }
        }
             
        //Debug.Log($"Loot.OnTriggerEnter2D()  PickupMode = {this.LootData.PickupMode}   isShip = {isShip}  isRailRound {isRailRound}  isLaser = {isLaser}  GO NAME = {inOther.gameObject}");

        int numLootRemaining = this.Quantity;
        this.amountCollected = this.Quantity;

        switch (this.LootData.LootType)
        {
            case ELootType.Energy:
                ship.AddEnergy(this.Quantity);
                numLootRemaining = 0;
                break;

            case ELootType.Health:
                var parentHealthEntity = inOther.gameObject.GetComponentInChildren<HealthEntityParent>(true);
                if (parentHealthEntity != null)
                    parentHealthEntity.AddHealthToChildren(this.Quantity);//TODO: fix to return remaining, etc.
                break;

            case ELootType.RailRounds:
                numLootRemaining = ship.AddRailRounds(this.Quantity);
                break;

            case ELootType.Missiles:
                numLootRemaining = ship.AddMissiles(this.Quantity);
                break;

            case ELootType.Credits:
                ship.AddCredits(this.Quantity);
                numLootRemaining = 0;
                break;

            case ELootType.Crystals:
                numLootRemaining = ship.AddCrystals(this.Quantity, this.LootData.CrystalType);
                break;

            default:
                break;
        }

        this.amountCollected = this.Quantity - numLootRemaining;

        if (this.amountCollected <= 0)
        {
            //TODO: reject sound, flash UI red, etc.
            return;//couldn't pick up any loot
        }
        else if (this.amountCollected < this.Quantity)
        {
            //picked up some, but not all
            this.Quantity = numLootRemaining;
            SetText(this.Quantity.ToString());

            if (this.initScale == Vector3.zero)
                this.initScale = this.transform.localScale;

            this.transform.DOScale(this.initScale * 1.5f, 1f).SetEase(Ease.OutElastic).OnComplete(() =>
            {
                this.transform.DOScale(this.initScale, 0.5f).SetEase(Ease.OutSine);
            });
        }
        else
        {
            //all collected
            // var scale = this.transform.localScale;
            // this.transform.DOScale(scale * 1.5f, 1f).SetEase(Ease.OutElastic).OnComplete(() =>
            // {
            //     this.transform.DOScale(scale, 0.5f).SetEase(Ease.OutSine);
            //     Pool.Despawn(this.gameObject);
            // });

            //TODO: spawn particle, etc.
            Pool.Despawn(this.gameObject);
            this.onCollected?.Invoke();
        }

        var collectedAll = this.amountCollected == this.Quantity;

        //print($"LootEntity.OnTriggerEnter2D()  Collected {this.amountCollected} of {this.Quantity}   collectedAll = {collectedAll}   Remaining = {numLootRemaining}");

        LootManager.OnLootCollected?.Invoke(this.amountCollected, collectedAll, this.LootData.LootType, this.LootData.CrystalType);
        
        SpawnLootCountCallout();
    }

    private void SpawnLootCountCallout()
    {
        var callout = Pool.Spawn<UILootCountCallout>("UI Loot Count Callout", UI.IN.UiElementsContainer, this.transform.position, Quaternion.identity);

        callout.Show(this.amountCollected, this.spriteRenderer, this.LootData, this.transform.position);
    }
}