using System;
using UnityEngine;

public class LaserCannon : Weapon
{
    [SerializeField] private SpriteRenderer[] energyMeterFills;

    [Header("Show/Hide Elements")]
    [SerializeField] private GameObject[] activeElements;
    [SerializeField] private GameObject[] dummyElements;

    public float MaxCharge { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsMiningLaser => this.laserCannonData != null && this.laserCannonData.IsMiningLaser;

    public bool HasData => this.laserCannonData != null;
    public float ChargeRate => this.laserCannonData != null ? this.laserCannonData.ChargeRate : 0f;
    private LaserCannonData laserCannonData;
    private float energyLevel;
    private bool isDummy;

    public bool IsMiningRock => this.lastShotLaser != null && this.lastShotLaser.IsMiningRock;
    public Laser LastShotLaser => this.lastShotLaser;
    private Laser lastShotLaser;

    public bool IsCharging => this.IsActive && this.EnergyLevel < this.MaxCharge;
    public bool IsShooting => this.lastShotLaser != null && this.lastShotLaser.IsGrowing;

    //this handles display
    public float EnergyLevel
    {
        get { return this.energyLevel; }

        set
        {
            this.energyLevel = value;

            float percent = 0;

            if (this.MaxCharge > 0)
                percent = Mathf.Clamp01(this.energyLevel / this.MaxCharge);

            SetEnergyMeterLevel(percent);
        }
    }

    public void SetEnergyMeterLevel(float inPercent)
    {
        inPercent = Mathf.Clamp01(inPercent);

        foreach (var fillbar in this.energyMeterFills)
        {
            fillbar.transform.localScale = new Vector3(1, inPercent, 1);
            var gradient = GlobalData.IN.LaserMeterGradients[this.laserCannonData.EnergyMeterGradientIndex];
            fillbar.color = gradient.Evaluate(inPercent);
        }
    }

    public void Init(LaserCannonData inData)
    {
        this.laserCannonData = inData;

        if (inData == null) return;

        this.MaxCharge = inData.MaxCharge;
        this.EnergyLevel = 0;
    }

    public void Shoot(Action inOnLaserComplete, ShipBase inShipBase, Action<Laser, float> inOnMiningLaserFiring)
    {
        //print("Shoot Laser of power " + this.EnergyLevel);
        if (this.lastShotLaser != null && this.lastShotLaser.IsGrowing)
            return;

        if (this.IsMiningLaser && this.lastShotLaser != null)
            return;

        this.lastShotLaser = Pool.Spawn<Laser>(this.laserCannonData.PrefabName, this.SpawnPoint, this.SpawnPoint.position, this.SpawnPoint.rotation);
        //this.lastShotLaser = Pool.Spawn<Laser>("Laser2", this.SpawnPoint, this.SpawnPoint.position, this.SpawnPoint.rotation);

        var miningToolData = GlobalData.GetMiningToolConfig(this.laserCannonData.MiningToolType);
        if (this.IsMiningLaser)
        {
            var damagePerSecond = this.laserCannonData.MiningLaserDamagePerSecond;
            this.lastShotLaser.BeginMiningShoot(damagePerSecond, miningToolData, inOnLaserComplete, inShipBase, this.laserCannonData.LaserColor, 2f, (laser) => { if(this.lastShotLaser == laser) this.lastShotLaser = null; }, inOnMiningLaserFiring);
        }
        else
        {
            this.lastShotLaser.BeginShoot(this.EnergyLevel, this.MaxCharge, miningToolData, inOnLaserComplete, inShipBase, this.laserCannonData.LaserColor, 2f, (laser) => { if(this.lastShotLaser == laser) this.lastShotLaser = null; });
            this.EnergyLevel = 0;
        }

        //TODO: start timer do drain ship energy over time
    }
    
    public void StopShooting()
    {
        if (this.lastShotLaser != null)
        {
            this.lastShotLaser.StopMiningLaser();
            this.lastShotLaser = null;
        }
    }

    //TODO: move this to Mounts
    public void SetDummy(bool inIsDummy)
    {
        this.isDummy = inIsDummy;

        foreach (var element in this.activeElements)
        {
            element.SetActive(!inIsDummy);
        }

        foreach (var element in this.dummyElements)
        {
            element.SetActive(inIsDummy);
        }
    }
}