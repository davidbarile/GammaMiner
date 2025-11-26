using System;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [Range(0f, 5f), SerializeField] private float speed;
    [Range(.01f, 1), SerializeField] private float energyToSizeRatio;
    [Range(0, 10), SerializeField] private float damageMultiplier;
    [Range(0, 1), SerializeField] private float innerOffset;
    [Range(0, 1), SerializeField] private float colliderOffset;
    [Range(-1, 1), SerializeField] private float yOffset;
    [Range(-1, 1), SerializeField] private float headYOffset;
    [Range(-1, 1), SerializeField] private float tailYOffset;
    [Range(-10, 10), SerializeField] private float particleYOffset;
    [Range(-2, 2), SerializeField] private float bodyHeightOffset;
    [Range(-2, 2), SerializeField] private float whiteHeightOffset;
    [Space, Range(0, 20), SerializeField] private float expireTime;
    [SerializeField] private CapsuleCollider2D collider2d;
    [SerializeField] private DamageEntity_Trigger damageEntity;
    [Header("Index 0 = white, the rest are colored")]
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private ParticleSystem[] particles;

    private float maxLaserValue;
    private int laserTicks = 0;
    private int hittingTargetTicks = 0;
    private int afterGrowTicks = 0;
    public bool IsGrowing { get; private set; }
    private GameObject hitTarget;
    private GameObject lastHitTarget;
    private float startShootTime = -1;
    private Action onLaserComplete;
    private ShipBase shipBase;
    private Vector3 lastShipPosition;
    private float laserScale = 1f;
    private bool didSwitch;
    private Action<Laser> onDestroy;

    public bool IsMiningRock => this.isMiningLaser && this.hitTarget != null;
    private bool isMiningLaser;
    private MiningToolConfig miningToolConfig;
    private bool didStopMiningLaser;
    private Vector3 startPosition;
    private Vector3 lastPosition;
    private float lastLaserLength;
    private Action<Laser, float> onMiningLaserFiring;
    private float damagePerSecondBase;

    [SerializeField] private bool shouldInitColor;

    [Header("Debug")]
    [SerializeField] private bool refreshOnValidate;
    [SerializeField] private bool shouldGrowForward;

    //debug only
    [Range(0, 10), SerializeField] private float debugEnergy;
    
    private void OnValidate()
    {
        if (this.refreshOnValidate)
            SetLength(this.debugEnergy, this.shouldGrowForward);
    }

    private void Awake()
    {
        this.damageEntity = this.GetComponent<DamageEntity_Trigger>();
        SetLasersVisible(false);
    }

    public void BeginShoot(float inEnergyLevel, float inMaxCharge, MiningToolConfig inMiningToolConfig, Action inOnLaserComplete, ShipBase inShipBase, Color inColor, float inScale = 1f, Action<Laser> inOnDestroy = null)
    {
        //print($"BeginShoot({inEnergyLevel})    isGrowing = { this.isGrowing }  inOnLaserComplete = { inOnLaserComplete }  shipBase = { inShipBase }");

        //inSpeed *= .1f;

        BeginShootBoilerPlate(inMiningToolConfig, inOnLaserComplete, inShipBase, inColor, inScale, inOnDestroy);

        this.isMiningLaser = false;
        this.maxLaserValue = inEnergyLevel * this.energyToSizeRatio;

        this.damagePerSecondBase = inMaxCharge * this.damageMultiplier * this.energyToSizeRatio / this.speed;
        this.damageEntity.DamagePerSecond = this.damagePerSecondBase;

        //print($"BeginShoot()  DamagePerSecond = { this.damageEntity.DamagePerSecond }  inEnergyLevel = {inEnergyLevel} / inMaxCharge = {inMaxCharge}    inSpeed = {inSpeed}");
    }

     public void BeginMiningShoot(float inEnergyLevel, MiningToolConfig inMiningToolConfig, Action inOnLaserComplete, ShipBase inShipBase, Color inColor, float inScale = 1f, Action<Laser> inOnDestroy = null, Action<Laser, float> inOnMiningLaserFiring = null)
    {
        //print($"BeginMiningShoot({inEnergyLevel})  inSpeed = { inSpeed }   inOnLaserComplete = { inOnLaserComplete }  shipBase = { inShipBase }");

        //inSpeed *= .1f;

        BeginShootBoilerPlate(inMiningToolConfig, inOnLaserComplete, inShipBase, inColor, inScale, inOnDestroy);

        this.isMiningLaser = true;
        this.damagePerSecondBase = inEnergyLevel * this.damageMultiplier;
        this.damageEntity.DamagePerSecond = this.damagePerSecondBase;
        this.onMiningLaserFiring = inOnMiningLaserFiring;

        //print($"BeginShoot()  DamagePerSecond = { this.damageEntity.DamagePerSecond }  inEnergyLevel = {inEnergyLevel} / inMaxCharge = {inMaxCharge}    inSpeed = {inSpeed}");
    }

    private void BeginShootBoilerPlate(MiningToolConfig inMiningToolConfig, Action inOnLaserComplete, ShipBase inShipBase, Color inColor, float inScale, Action<Laser> inOnDestroy)
    {
        TickManager.OnTick += this.Tick;

        this.onDestroy = inOnDestroy;
        this.miningToolConfig = inMiningToolConfig;

        SetColor(inColor);

        this.laserScale = inScale;
        this.transform.localScale = new Vector3(this.laserScale, this.laserScale, 1);

        this.startPosition = this.transform.position;
        this.lastPosition = this.transform.position;

        this.didSwitch = false;
        this.didStopMiningLaser = false;

        this.onLaserComplete = inOnLaserComplete;
        this.shipBase = inShipBase;

        this.laserTicks = 0;
        this.hittingTargetTicks = 0;
        this.afterGrowTicks = 0;
        this.IsGrowing = true;
        this.startShootTime = Utils.NOW;
        this.lastShipPosition = this.shipBase.transform.position;

        this.damageEntity.SetMiningToolConfig(this.miningToolConfig);

        SetLasersVisible(true);
    }

    private void Tick()
    {
        if (this.isMiningLaser)
            Tick_MiningLaser();
        else
            Tick_StandardLaser();
    }

    private void Tick_StandardLaser()
    {
        if (!this.collider2d.enabled) return;//if laser not visible

        var hasLaserExpired = Utils.NOW - this.startShootTime > this.expireTime;

        if (!hasLaserExpired)
        {
             ++this.laserTicks;

            this.IsGrowing = Mathf.CeilToInt(this.laserTicks * this.speed) <= this.maxLaserValue;

            if (!this.IsGrowing)
                ++this.afterGrowTicks;
            
            var raycastRange = 2f;
            var raycastHit = Physics2D.Raycast(this.transform.position, this.transform.up, raycastRange, LayerMask.GetMask("Rocks", "Enemy Ships", "Enemy Shields"));

            if (raycastHit.collider != null && this.hitTarget == null && raycastHit.distance > .3f)
            {
                this.hitTarget = raycastHit.collider.gameObject;
            }

            float laserLength = (this.laserTicks - this.afterGrowTicks - this.hittingTargetTicks) * this.speed;

            SetLength(laserLength, this.IsGrowing);

            SetParticlesVisibility(this.hitTarget != null);

            if (this.hitTarget != null)
                ++this.hittingTargetTicks;

            if (laserLength > 0)
            {
                var laserForwardMoveVector = this.speed * this.laserScale * this.transform.up.normalized;

                if (!this.IsGrowing)
                {
                    if (!this.didSwitch)
                    {
                        this.didSwitch = true;
                        this.transform.SetParent(GameManager.IN.ProjectilesContainer);

                        this.transform.position += this.speed * this.laserTicks * this.laserScale * this.transform.up.normalized;

                        this.onLaserComplete?.Invoke();
                        this.onLaserComplete = null;
                    }

                    if (this.hitTarget == null)
                        this.transform.position += laserForwardMoveVector;
                }
                
                // if(this.name == "Laser 0")
                // {
                //     if (this.hitTarget)
                //         print($"<color=white>[{name}] hitTarget = {this.hitTarget?.name}. frame = {Time.frameCount}</color>");
                //     else
                //         print($"<color=black>[{name}] hitTarget = NULL  frame = {Time.frameCount}</color>");
                // }

                //make laser track with ship movement
                // if (this.shipBase != null)
                // {
                //     var shipDeltaPos = this.shipBase.transform.position - this.lastShipPosition;
                //     if(this.didSwitch)
                //         this.transform.position += shipDeltaPos;
                //     this.lastShipPosition = this.shipBase.transform.position;
                // }
            }
            else
            {
                //check if laser has shrunk to zero, if so, hide
                FlagForDestroy(3f);//let particles finish
                return;
            }
        }
        else
            FlagForDestroy();
    }

    private void Tick_MiningLaser()
    {
        // print($"Tick_MiningLaser()  frame = {Time.frameCount}");

        SetParticlesVisibility(this.hitTarget != null);

        float laserLength = 0;

        var raycastRange = 50; //20f;//TODO: make this based on ship mining laser range upgrade

        var raycastHit = Physics2D.Raycast(this.transform.position, this.transform.up, raycastRange, LayerMask.GetMask("Default", "Rocks", "Both Maps", "Enemy Ships", "Enemy Shields"));

        if (raycastHit.collider != null)
        {
            this.hitTarget = raycastHit.collider.gameObject;

            laserLength = (1.8f * (raycastHit.distance / this.laserScale)) + 1f;

            SetLasersVisible(true);

            if(this.lastHitTarget != this.hitTarget)
            {
                if(this.hitTarget.TryGetComponent<Rock>(out var rock))
                {
                    if (this.miningToolConfig != null && rock != null && rock.RockData != null)
                    {
                        var damageMultiplier = this.miningToolConfig.GetDamageMultiplier(rock.RockData.RockType);
                        this.damageEntity.DamagePerSecond = this.damagePerSecondBase * damageMultiplier;
                        print($"miningToolDamageMultiplier = {damageMultiplier}   rock.RockData.RockType = {rock.RockData.RockType}   DamagePerSecond = {this.damagePerSecondBase}/{this.damageEntity.DamagePerSecond}");
                    }
                }
            }

            this.onMiningLaserFiring?.Invoke(this, 1f);
        }
        else
        {
            this.hitTarget = null;
            this.damageEntity.DamagePerSecond = this.damagePerSecondBase;
            SetLasersVisible(false);
            this.onMiningLaserFiring?.Invoke(this, 0f);
        }

        this.lastHitTarget = this.hitTarget;

        SetLength(laserLength, true);
    }

    private void FlagForDestroy(float inDelay = 0f)
    {
        TickManager.OnTick -= this.Tick;
        this.onMiningLaserFiring?.Invoke(this, 0f);
        SetLasersVisible(false);
        this.onDestroy?.Invoke(this);
        Invoke(nameof(Despawn), inDelay);
    }

    private void Despawn()
    {
        Pool.Despawn(this.gameObject);
    }

    public void StopMiningLaser()
    {
        //print($"StopMiningLaser(). {name}.  color = {this.spriteRenderers[1].color}");

        // this.IsGrowing = false;
        // this.transform.SetParent(GameManager.IN.ProjectilesContainer);
        // this.didStopMiningLaser = true;
        // this.startShootTime = 0;

        FlagForDestroy();
    }

    private void SetLength(float inLength, bool inShouldGrowForward = false)
    {
        for (var i = 0; i < this.spriteRenderers.Length; ++i)
        {
            var sr = this.spriteRenderers[i];

            float offset = i == 1 ? 0 : this.innerOffset;

            var length = inLength - offset;
            length = Mathf.Max(length, 1 - this.innerOffset);

            var whiteOffset = i == 0 ? this.whiteHeightOffset : 0;

            var bHeightOffset = i == 0 ? 0 : this.bodyHeightOffset;
            var halfBodyHeightOffset = bHeightOffset * .5f;

            if (i < 2)
            {
                sr.size = new Vector2(sr.size.x, (length / sr.transform.localScale.y) + whiteOffset + bHeightOffset);

                if (inShouldGrowForward)
                    sr.transform.localPosition = new Vector3(0, (length + offset) * .5f + this.yOffset, 0);
                else
                    sr.transform.localPosition = new Vector3(0, -1 * (length + offset) * .5f - this.yOffset, 0);
            }
            else if (i == 2) //head
            {
                if (inShouldGrowForward)
                {
                    sr.transform.localPosition = new Vector3(0, 0 + this.yOffset + this.headYOffset - halfBodyHeightOffset, 0);
                    sr.transform.localRotation = Quaternion.Euler(0, 0, 180);
                }
                else
                {
                    sr.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    sr.transform.localPosition = new Vector3(0, -1 * (0 + this.yOffset + this.headYOffset - halfBodyHeightOffset), 0);

                    foreach (var ps in this.particles)
                    {
                        ps.transform.localPosition = sr.transform.localPosition;
                    }
                }
            }
            else if (i == 3) //tail
            {
                if (inShouldGrowForward)
                {
                    sr.transform.localPosition = new Vector3(0, length + this.yOffset + this.tailYOffset + halfBodyHeightOffset, 0);
                    sr.transform.localRotation = Quaternion.Euler(0, 0, 180);

                    foreach (var ps in this.particles)
                    {
                        ps.transform.localPosition = sr.transform.localPosition;
                    }
                }
                else
                {
                    sr.transform.localPosition = new Vector3(0, -1 * (length + this.yOffset + this.tailYOffset + halfBodyHeightOffset), 0);
                    sr.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
            }

            //position particle systems
            var particlePos = new Vector3(0, -1 * (0 + this.yOffset + this.headYOffset - halfBodyHeightOffset + this.particleYOffset), 0);

            if (inShouldGrowForward)
                particlePos = new Vector3(0, length + this.yOffset + this.tailYOffset + halfBodyHeightOffset - this.particleYOffset, 0);

            foreach (var ps in this.particles)
            {
                ps.transform.localPosition = particlePos;
                ps.transform.localScale = Vector3.one * this.laserScale * .7f;//TODO: tune laser particles and delete .7f factor
            }

            if (i == 1 && this.collider2d != null)
            {
                float colHeight = length - this.colliderOffset;
                this.collider2d.size = new Vector2(this.collider2d.size.x, colHeight);

                if (inShouldGrowForward)
                    this.collider2d.offset = new Vector2(0, .5f * colHeight);
                else
                    this.collider2d.offset = new Vector2(0, -.5f * colHeight);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D inCollider)
    {        
        if (inCollider.gameObject != null)
        {
            this.hitTarget = inCollider.gameObject;
            //print("TRIGGER ENTER: this.hitTarget = " + this.hitTarget);
        }
    }

    private void OnTriggerExit2D(Collider2D inCollider)
    {
        this.hitTarget = null;
        //print("TRIGGER EXIT:  this.hitTarget = " + this.hitTarget);
    }

    private void SetParticlesVisibility(bool inIsVisible)
    {
        foreach (var ps in this.particles)
        {
            ps.gameObject.SetActive(inIsVisible);
        }
    }

    private void SetLasersVisible(bool inIsVisible)
    {
        foreach (var sr in this.spriteRenderers)
        {
            sr.gameObject.SetActive(inIsVisible);
        }

        if (!inIsVisible)
            SetParticlesVisibility(false);

        this.collider2d.enabled = inIsVisible;
    }

    public void SetColor(Color inColor)
    {
        for(int i = 0; i < this.spriteRenderers.Length; ++i)
        {
            if(i > 0) this.spriteRenderers[i].color = inColor;
        }

        foreach (var ps in this.particles)
        {
            var main = ps.main;
            var lerpColor1 = Color.Lerp(inColor, Color.white, .2f);
            var lerpColor2 = Color.Lerp(inColor, Color.white, .4f);
            main.startColor = new ParticleSystem.MinMaxGradient(lerpColor1, lerpColor2);
        }
    }
}