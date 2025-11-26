using System;
using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;
using static HealthData;
using static CrystalData;

public class SpaceShip : ShipBase
{
    public static SpaceShip PlayerShip;

    private readonly int ANIM_STATE = Animator.StringToHash("state");

    public bool IsPlayer { get; private set; }

    [Space]
    [SerializeField] private SpriteRenderer[] energyMeterFills;
    [SerializeField] private TextMeshPro[] energyMeterTexts;

    [Space]
    [SerializeField] private Animator lasersAnimator;

    [Space]
    [SerializeField] private PointEffector2D pointEffector2D;

    private Action<Laser, float> onMiningLaserFiring;

    public int TotalCrystals { get; private set; }

    private bool isInputAssigned;

    private bool shouldRechargeEnergy;
    private bool isChargingLaser;
    private bool isChargingShields;
    private bool isChargingingThrusters;
     private bool isMiningLasersFiring;

    private bool isLaserAnimOpen;
    private bool isMiningLaserMode;
    private bool isEngineDisabled;
    private DateTime stoppedChargingShieldsTime;
    public bool IsMissileHudButtonDisabled { get; private set; }

    private enum ELasersPosition
    {
        Closed,
        Open,
        Closing,
        Opening
    }

    //this handles display
    public float EnergyLevel
    {
        get { return this.shipData.BatteryData.EnergyCharge; }

        private set
        {
            this.shipData.BatteryData.EnergyCharge = Mathf.Clamp(value, 0, this.shipData.BatteryData.MaxEnergyCharge);

            float percent = this.shipData.BatteryData.EnergyCharge / this.shipData.BatteryData.MaxEnergyCharge;

            var energyMeterGradient = GlobalData.IN.BatteryMeterGradients[(int)this.ShipData.ReactorData.SpriteIndex];

            foreach (var fillbar in this.energyMeterFills)
            {
                fillbar.transform.localScale = new Vector3(1, percent, 1);
                fillbar.color = energyMeterGradient.Evaluate(percent);
            }

            int amount = Mathf.RoundToInt(this.shipData.BatteryData.EnergyCharge);

            foreach (var text in this.energyMeterTexts)
            {
                text.text = amount.ToString();
            }

            if (this.IsPlayer)
                HUD.OnEnergyChanged?.Invoke(amount);
        }
    }

    public void HandleMiningLaserFiring(Laser inLaser, float inPercent)
    {
        foreach (var lc in this.laserCannons)
        {
            if (lc.LastShotLaser == inLaser)
            {
                lc.SetEnergyMeterLevel(inPercent);

                if(lc.IsMiningRock)
                {
                    this.isMiningLasersFiring = true;
                }
            }
        }
    }

    protected override void Start()
    {
        //case where shipConfig is set in the inspector (enemies, etc.)
        if (this.shipConfig && !this.isInitialized)
        {
            this.shipData = ShipData.GetDataFromConfig(this.shipConfig);
            Init(this.shipData, false);
        }
    }

    public override void Init(ShipData inShipData, bool inIsPlayer)
    {
        this.IsPlayer = inIsPlayer;

        base.Init(inShipData, inIsPlayer);

        this.EnergyLevel = inShipData.BatteryData.MaxEnergyCharge;//always reset to max energy charge

        this.shouldRechargeEnergy = true;

        SetShipNameText(string.Empty);

        if (inIsPlayer)
        {
            this.name = $"~PLAYER SHIP~ ({inShipData.Name})";

            var shipTitle = $"{inShipData.Name.ToUpper()}\n<size=70%>Player</size>";
            SetShipNameText(shipTitle);

            var allChildren = this.GetComponentsInChildren<Transform>(true);

            foreach (var child in allChildren)
            {
                child.tag = "Player";
            }

            if (!this.isInputAssigned)
            {
                this.isInputAssigned = true;
                RegisterInputs();
            }

            this.HullHealth.OnDie += GameManager.IN.OnPlayerDie;

            HUD.IN.Init(this);
        }
        else
        {
            if(inShipData != null)
            {
                this.name = $"Enemy Ship ({inShipData.Name})";
                
                var shipTitle = $"{inShipData.Name.ToUpper()}\n<size=70%>Enemy</size>";
                SetShipNameText(shipTitle);
            }
        }
    }

    private void RegisterInputs()
    {
        #region collapse
        InputManager.OnTurnLeftPress += RotateLeft;
        InputManager.OnTurnRightPress += RotateRight;
        InputManager.OnNoTurnPress += StopRotation;

        InputManager.OnThrustPress += Thrust;
        InputManager.OnBrakePress += Brake;
        InputManager.OnNoThrust += StopThrust;

        InputManager.OnDashPress += StartDash;
        InputManager.OnDashRelease += StopDash;

        InputManager.OnShootRailgunsHold += ShootRailguns;
        InputManager.OnShootMissilePress += ShootMissile;
        InputManager.OnMissileTargetingPress += OpenLaserAnim;
        InputManager.OnMissileTargetingRelease += CloseLaserAnim;

        InputManager.OnLaserHold += HandleLaserHold;
        InputManager.OnLaserRelease += HandleLaserRelease;

        InputManager.OnShieldsHold += ChargeShields;
        InputManager.OnShieldsRelease += StopChargingShields;

        this.onMiningLaserFiring += HandleMiningLaserFiring;

        TickManager.OnTick += Tick;
        #endregion
    }

    protected override void OnDestroy()
    {
        //print($"<color=red>SpaceShip.OnDestroy(): {this.name}</color>");
        if (this.isInputAssigned)
        {
            if (GameManager.IN != null)
                this.HullHealth.OnDie -= GameManager.IN.OnPlayerDie;

            #region collapse
            InputManager.OnTurnLeftPress -= RotateLeft;
            InputManager.OnTurnRightPress -= RotateRight;
            InputManager.OnNoTurnPress -= StopRotation;

            InputManager.OnThrustPress -= Thrust;
            InputManager.OnBrakePress -= Brake;
            InputManager.OnNoThrust -= StopThrust;

            InputManager.OnShootRailgunsHold -= ShootRailguns;
            InputManager.OnShootMissilePress -= ShootMissile;
            InputManager.OnMissileTargetingPress -= OpenLaserAnim;
            InputManager.OnMissileTargetingRelease -= CloseLaserAnim;

            InputManager.OnLaserHold -= HandleLaserHold;
            InputManager.OnLaserRelease -= HandleLaserRelease;

            InputManager.OnShieldsHold -= ChargeShields;
            InputManager.OnShieldsRelease -= StopChargingShields;

            this.onMiningLaserFiring -= HandleMiningLaserFiring;
            TickManager.OnTick -= Tick;
            #endregion
        }

        base.OnDestroy();
    }

    private void Tick()
    {
        if (this.IsPlayer)
        {
            TileLoadingManager.IN.UpdatePlayerCoords(this.transform);
        }

        this.shouldRechargeEnergy = !this.isChargingingThrusters && !this.isChargingLaser && !this.isChargingShields && !this.isMiningLasersFiring;

        if (this.shouldRechargeEnergy)
        {
            var rechargeRate = UiDebugPanel.HasInfiniteEnergy ? 999 : this.shipData.ReactorData.EnergyRechargeRate;
            if (this.EnergyLevel < this.shipData.BatteryData.MaxEnergyCharge)
                this.EnergyLevel = Mathf.Min(this.EnergyLevel + rechargeRate, this.shipData.BatteryData.MaxEnergyCharge);
        }
        else if (this.EnergyLevel > 0)
        {
            //energize thrusters
            if (this.isChargingingThrusters && !AreEnginesDead())
            {
                float energyAmount = Mathf.Min(this.EnergyLevel, GetThrustersDischargeRate());
                this.EnergyLevel -= energyAmount * this.energyConsumptionModifier;//lessen the drain on ship energy

                float GetThrustersDischargeRate()
                {
                    float dischargeRate = 0;

                    var thrusterHealths = this.shipData.GetHeathsOfType(EHealthDataType.Thruster);

                    //temp fix
                    var numThrusters = Mathf.Min(thrusterHealths.Count, this.shipData.ThrusterDatas.Count);

                    for (int i = 0; i < numThrusters; i++)
                    {
                        var health = thrusterHealths[i];

                        //check if dead
                        // var thruster = this.forwardEngines[i];
                        // if (!thruster.gameObject.activeInHierarchy) continue;

                        if (health.IsDead) continue;

                        var data = this.shipData.ThrusterDatas[i];

                        dischargeRate += data.DischargeRate;
                    }

                    return dischargeRate;
                }
            }

            //charge lasers
            if (this.isChargingLaser)
            {
                float energyAmount = Mathf.Min(this.EnergyLevel, GetLasersChargeRate());
                this.EnergyLevel -= energyAmount * this.energyConsumptionModifier;//lessen the drain on ship energy

                float safetyCounter = energyAmount;//in case no laser can be charged, no infinite loop

                while (energyAmount > 0 && safetyCounter > 0)
                {
                    foreach (var lc in this.laserCannons)
                    {
                        if (lc.IsActive && !lc.IsShooting && lc.HasData && lc.EnergyLevel < lc.MaxCharge)
                        {
                            ++lc.EnergyLevel;
                            --energyAmount;
                        }

                        --safetyCounter;
                    }
                }

                float GetLasersChargeRate()
                {
                    float chargeRate = 0;

                    //maybe add code to activate/deactivate laser cannons if data exists or not

                    for (int i = 0; i < this.laserCannons.Length; i++)
                    {
                        var lc = this.laserCannons[i];
                        if (lc == null || !lc.gameObject.activeInHierarchy || !lc.HasData) continue;
                        chargeRate += lc.ChargeRate;
                    }

                    return chargeRate;
                }
            }

            //mining lasers draining energy
            if (this.isMiningLasersFiring)
            {
                float energyAmount = 0;

                foreach (var lc in this.laserCannons)
                {
                    if (lc.IsActive && lc.IsMiningRock)
                    {
                        energyAmount += lc.ChargeRate;
                    }
                }

                energyAmount = Mathf.Min(this.EnergyLevel, energyAmount);
                this.EnergyLevel -= energyAmount * Time.deltaTime;

                this.isMiningLasersFiring = false;
                this.shouldRechargeEnergy = false;
            }
            
            //charge shields
            if(this.shipData.ShieldGeneratorData != null)
            {
                if (this.isChargingShields)
                {
                    float energyAmount = Mathf.Min(this.EnergyLevel, this.shipData.ShieldGeneratorData.ChargeRate);
                    this.EnergyLevel -= energyAmount * this.energyConsumptionModifier;//lessen the drain on ship energy

                    this.shieldGenerator.ChargeShields(energyAmount);

                    if (this.shieldGenerator.TotalShieldsHealth < this.shieldGenerator.TotalShieldsMaxHealth)
                        this.stoppedChargingShieldsTime = DateTime.Now;
                }
            }
        }

        if (!this.isChargingShields && this.shieldGenerator != null && this.shipData.ShieldGeneratorData != null && this.shieldGenerator.TotalShieldsHealth > 0)
        {
            var timeSinceStoppedCharging = DateTime.Now.Subtract(this.stoppedChargingShieldsTime);

            if (timeSinceStoppedCharging.TotalSeconds > this.shipData.ShieldGeneratorData.DelayToDischarge)
            {
                this.shieldGenerator.DischargeShields(this.shipData.ShieldGeneratorData.DelayToDischarge);
            }
        }

        this.isChargingingThrusters = false;
        this.isChargingShields = false;

        /*
        if (this.turnDirection != TurnDirection.Left)
        {
            foreach (var jet in this.leftTurnEngines)
                jet.SetAnimatorState(JetEngine.ThrustMode.End);
        }
        else if (this.turnDirection != TurnDirection.Right)
        {
            foreach (var jet in this.rightTurnEngines)
                jet.SetAnimatorState(JetEngine.ThrustMode.End);
        }*/

        //this.turnDirection = TurnDirection.Straight;
    }

    //TODO: break this out into ShipBase too
    public void Thrust(float inAmount)
    {
        if (AreEnginesDead()) return;

        if (this.EnergyLevel > 50)
            this.isEngineDisabled = false;

        if (this.EnergyLevel <= 5)
            this.isEngineDisabled = true;

        //print($"Thrust() this.EnergyLevel = {this.EnergyLevel}   this.isEngineDisabled = {this.isEngineDisabled}");

        if (this.isEngineDisabled)
        {
            StopThrust();
            return;
        }

        this.rigidBody2d.linearDamping = this.rbDrag;
        var totalAcceleration = GetAccelerationFromThrusters();
        this.rigidBody2d.AddRelativeForce(Vector2.up * totalAcceleration * inAmount);

        this.isChargingingThrusters = true;

        if (this.engineState != EngineState.Off)
        {
            foreach (var jet in this.forwardEngines)
            {
                if (jet.gameObject.activeInHierarchy)
                    jet.SetAnimatorState(Thruster.ThrustMode.Start);
            }
        }

        this.engineState = EngineState.Thrust;
    }

    //TODO: cache this and hook up to thruster onDie event
    private bool AreEnginesDead()
    {
        var thrusterHealths = this.shipData.GetHeathsOfType(EHealthDataType.Thruster);

        foreach (var health in thrusterHealths)
        {
            if (!health.IsDead) return false;
        }
        return true;
    }

    //TODO: cache this and hook up to thruster onDie event
    private float GetAccelerationFromThrusters()
    {
        float acceleration = 0;
        var thrusterHealths = this.shipData.GetHeathsOfType(EHealthDataType.Thruster);

        for (int i = 0; i < thrusterHealths.Count; i++)
        {
            var health = thrusterHealths[i];

            if (health.IsDead) continue;

            if (i < this.shipData.ThrusterDatas.Count)
            {
                var data = this.shipData.ThrusterDatas[i];

                acceleration += data.AccelerationRate;
            }
        }

        return acceleration;
    }

    public void Brake(float inAmount)
    {
        if (AreEnginesDead()) return;

        StopThrust();

        if (this.rigidBody2d.linearVelocity.magnitude > 0)
            this.rigidBody2d.linearDamping = this.shipData.DecelerationRate * Mathf.Abs(inAmount);

        //print($"inAmount = {inAmount}   this.shipData.DecelerationRate = {this.shipData.DecelerationRate}  this.rigidBody2d.linearDamping = {this.rigidBody2d.linearDamping}  this.rigidBody2d.linearVelocity = {this.rigidBody2d.linearVelocity}");
    }

    public void StopThrust()
    {
        this.rigidBody2d.linearDamping = this.rbDrag;

        foreach (var jet in this.forwardEngines)
        {
            if (jet.gameObject.activeInHierarchy)
                jet.SetAnimatorState(Thruster.ThrustMode.End);
        }

        this.engineState = EngineState.Off;
    }

    public void StartDash()
    {
        var dashForce = GetDashforce();
        var energyCost = dashForce * .25f;

          if (this.EnergyLevel > 50)
            this.isEngineDisabled = false;

        if (this.isEngineDisabled || this.EnergyLevel < energyCost)
        {
            this.isEngineDisabled = true;
            return;
        }

        this.EnergyLevel -= energyCost;
        //print($"StartDash() dashForce = {dashForce}");
        this.rbCachedVelocity = this.rigidBody2d.linearVelocity;
        this.rigidBody2d.AddRelativeForce(Vector2.up * dashForce, ForceMode2D.Impulse);

        DOTween.KillAll();

        //TODO: show thrust (and hide after dash ends, if not thrusting)

        float GetDashforce()
        {
            float dashForce = 0;
            var thrusterHealths = this.shipData.GetHeathsOfType(EHealthDataType.Thruster);
            var numThrusters = Mathf.Min(thrusterHealths.Count, this.shipData.ThrusterDatas.Count);

            for (int i = 0; i < numThrusters; i++)
            {
                var health = thrusterHealths[i];

                if (health.IsDead) continue;

                var data = this.shipData.ThrusterDatas[i];
                dashForce += data.AccelerationRate;
            }

            return dashForce * 5;//TODO: add as setting in shipData/thrusterData
        }
    }

    public void StopDash()
    {
        //this.rigidBody2d.velocity = this.rbCachedVelocity;
        DOTween.To(() => this.rigidBody2d.linearVelocity, x => this.rigidBody2d.linearVelocity = x, this.rbCachedVelocity, .25f);
    }

    public void ShootRailguns()
    {
        //print($"<color=yellow>ShootRailguns().  frame = {Time.frameCount}</color>");

        if (!this.gameObject.activeInHierarchy) return;

        var numRailguns = Mathf.Min(this.railguns.Length, this.shipData.RailgunDatas.Count);
        if (numRailguns == 0 || this.NumRailRounds <= 0) return;

        this.NumRailRounds = 0;

        for (var i = 0; i < numRailguns; i++)
        {
            var rgData = this.shipData.RailgunDatas[i];
            var railgun = this.railguns[i];

            if (rgData == null || !railgun.gameObject.activeInHierarchy || rgData.TotalRoundsRemaining <= 0) continue;

            if (DateTime.UtcNow - railgun.LastShotTime >= TimeSpan.FromSeconds(1f / rgData.RoundsPerSecond))
            {
                railgun.Shoot(this.rigidBody2d.linearVelocity);

                if (rgData.NumRoundsHeavy > 0)
                    --rgData.NumRoundsHeavy;
                else if (rgData.NumRoundsMedium > 0)
                    --rgData.NumRoundsMedium;
                else if (rgData.NumRoundsLight > 0)
                    --rgData.NumRoundsLight;
            }

            this.NumRailRounds += rgData.NumRoundsLight + rgData.NumRoundsMedium + rgData.NumRoundsHeavy;
        }
        
        SetRailRoundsCount(this.NumRailRounds);//TODO: maybe change to show light, med, heavy railgun rounds, in which case connect to railgun data
    }

    public override void ShootMissile()
    {
        //print("ShootMissile()");

        base.ShootMissile();

        if (this.IsPlayer && !this.IsMissileHudButtonDisabled)
        {
            if (this.timeOfLastMissileShot != -1)
            {
                this.IsMissileHudButtonDisabled = true;
                HUD.IN.SetMissilesButtonActive(false);
                CursorManager.IN.SetTargetModeActive(false);
                Invoke(nameof(ResetMissileHudButton), this.missileCooldownTime);
            }
        }   
    }

    private void ResetMissileHudButton()
    {
        this.IsMissileHudButtonDisabled = false;
        HUD.IN.SetMissilesButtonActive(this.MaxMissiles > 0);

        if(InputManager.IN.IsShiftPressed)
            CursorManager.IN.SetTargetModeActive(true);
    }

    protected override IEnumerator ShootMissileCo()
    {
        PopLaserAnimOpen();

        StartCoroutine(base.ShootMissileCo());

        yield break;
    }

    private void HandleLaserHold()
    {
        this.isMiningLaserMode = false;

        foreach (var lc in this.laserCannons)
        {
            if (lc.IsMiningLaser)
            {
                this.isMiningLaserMode = true;
                ShootLasers();
                return;
            }
        }

        ChargeLasers();
    }

    private void ChargeLasers()
    {
        //print($"ChargeLasers()");

        bool atLeastOneLaserCharging = false;

        bool isAnyLaserActive = false;

        foreach (var lc in this.laserCannons)
        {
            if (lc.IsCharging)
                atLeastOneLaserCharging = true;

            if (lc.IsActive)
                isAnyLaserActive = true;
        }

        //disable laser charging if any laser is shooting
        foreach (var lc in this.laserCannons)
        {
            if (lc.IsShooting)
            {
                atLeastOneLaserCharging = false;
                break;
            }
        }

        this.isChargingLaser = atLeastOneLaserCharging;

        if(isAnyLaserActive && this.ShipData.LaserCannonDatas.Count > 0)
            OpenLaserAnim();
    }

    private void OpenLaserAnim()
    {
        if (this.lasersAnimator == null) return;

        if (this.isLaserAnimOpen)
            this.lasersAnimator.SetInteger(ANIM_STATE, (int)ELasersPosition.Open);
        else
            this.lasersAnimator.SetInteger(ANIM_STATE, (int)ELasersPosition.Opening);
    }

    private void PopLaserAnimOpen()
    {
        if (this.lasersAnimator == null) return;
        
        this.isLaserAnimOpen = true;
        this.lasersAnimator.SetInteger(ANIM_STATE, (int)ELasersPosition.Open);
    }

    private void CloseLaserAnim()
    {
        if (this.lasersAnimator == null) return;

        if (this.isLaserAnimOpen)
            this.lasersAnimator.SetInteger(ANIM_STATE, (int)ELasersPosition.Closing);
        else
            this.lasersAnimator.SetInteger(ANIM_STATE, (int)ELasersPosition.Closed);
    }

    public void HandleLaserRelease()
    {
        if (this.isMiningLaserMode)
            StopMiningLasers();
        else
        {
            ShootLasers();
            this.isChargingLaser = false;
        }
    }
    
    private void StopMiningLasers()
    {
        this.isMiningLasersFiring = false;

        foreach (var lc in this.laserCannons)
        {
            lc.StopShooting();   
        }

        CloseLaserAnim();
    }

    public override void ShootLasers()
    {
        //print("ShootLaser()");
        foreach (var lc in this.laserCannons)
        {
            if (lc.IsShooting)
                return;
        }

        PopLaserAnimOpen();

        foreach (var lc in this.laserCannons)
        {
            if (lc.HasData && lc.gameObject.activeInHierarchy)
                lc.Shoot(CloseLaserAnim, this, this.onMiningLaserFiring);
        }
    }

    public void ChargeShields()
    {
        if (!this.shieldGenerator.AreAllShieldsCharged)
        {
            this.isChargingShields = true;
        }
        else
        {
            StopChargingShields();
        }
    }

    public void StopChargingShields()
    {
        this.isChargingShields = false;
    }

    public override void SetRailRoundsCount(int inAmount)
    {
        base.SetRailRoundsCount(inAmount);
        
        if (this.IsPlayer)
            HUD.OnRailRoundsChanged?.Invoke(this.NumRailRounds);
    }

    public override void SetMissilesCount(int inAmount)
    {
        base.SetMissilesCount(inAmount);

        if (this.IsPlayer)
            HUD.OnMissilesChanged?.Invoke(this.NumMissiles);
    }

    public int AddCrystals(int inAmountToAdd, ECrystalType inCrystalType)
    {
        if (inAmountToAdd <= 0)
        {
            // Handle negative or zero crystal amounts
            print($"<color=red>Attempting to add non-positive crystal amount: {inAmountToAdd} of type {inCrystalType}</color>");
            return inAmountToAdd;
        }

        this.TotalCrystals = 0;

        var successfullyAdded = 0;

        for (int i = 0; i < PlayerData.Data.ShipData.VaultDatas.Count; i++)
        {
            var vaultData = PlayerData.Data.ShipData.VaultDatas[i];

            //check if vault allows this crystal type
            if (!vaultData.AllowedCrystalTypes.HasFlag(inCrystalType))
            {
                print($"<color=orange>Vault {vaultData.SubTitle} does not allow crystal type {inCrystalType}</color>");
                continue;
            }

            var availableSpace = vaultData.Capacity - vaultData.GetUsedStorage();
            var amountToAddToStorage = Mathf.Min(inAmountToAdd, availableSpace);

            print($"SpaceShip.UsedStorage {vaultData.GetUsedStorage()} / {vaultData.Capacity} = availableSpace {availableSpace}");

            if (amountToAddToStorage > 0)
            {
                var crystalTypeString = inCrystalType.ToString();

                vaultData.StoredCrystalsDict[crystalTypeString] += amountToAddToStorage;

                inAmountToAdd -= amountToAddToStorage;
                successfullyAdded += amountToAddToStorage;

                print($"Added {amountToAddToStorage} of {inCrystalType} to vault {vaultData.Name}. Remaining amount: {inAmountToAdd}");
            }

            //continue thru all vaults even if one is full to get total crystals
            this.TotalCrystals += vaultData.GetUsedStorage();
        }

        if (inAmountToAdd > 0)
        {
            // If there are still crystals left to add, log a message
            print($"<color=red>Not enough space in vaults for {inAmountToAdd} of {inCrystalType}. Added only {successfullyAdded}.</color>");
        }

        if (this.IsPlayer)
            HUD.OnCrystalsChanged?.Invoke(PlayerData.Data.ShipData);
            
        return inAmountToAdd;//return remaining amount that couldn't be added
    }

    public void RecalculateCrystalsInVaults()
    {
        this.TotalCrystals = 0;

        foreach (var vaultData in PlayerData.Data.ShipData.VaultDatas)
        {
            int totalCrystalsInVault = 0;

            foreach (var kvp in vaultData.StoredCrystalsDict)
            {
                totalCrystalsInVault += kvp.Value;
            }

            this.TotalCrystals += totalCrystalsInVault;
        }

        if (this.IsPlayer)
            HUD.OnCrystalsChanged?.Invoke(PlayerData.Data.ShipData);
    }

    /// <summary>
    /// Returns the total number of crystals of a specific type across all vaults.
    /// </summary>
    public int GetCrystalsOfType(ECrystalType inCrystalType)
    {
        int totalCrystals = 0;

        foreach (var vaultData in PlayerData.Data.ShipData.VaultDatas)
        {
            if (vaultData.StoredCrystalsDict.TryGetValue(inCrystalType.ToString(), out int amount))
            {
                totalCrystals += amount;
            }
        }

        return totalCrystals;
    }

    public bool RemoveCrystalsOfType(ECrystalType inCrystalType, int inAmount, out int outAmount, out int remainingAmount)
    {
        outAmount = 0;
        remainingAmount = GetCrystalsOfType(inCrystalType);

        if (inAmount <= 0)
        {
            print($"<color=red>Attempting to remove non-positive crystal amount: {inAmount} of type {inCrystalType}</color>");
            return false;
        }

        foreach (var vaultData in PlayerData.Data.ShipData.VaultDatas)
        {
            if (!vaultData.StoredCrystalsDict.ContainsKey(inCrystalType.ToString())) continue;

            int availableAmount = vaultData.StoredCrystalsDict[inCrystalType.ToString()];

            if (availableAmount <= 0) continue;

            int amountToRemove = Mathf.Min(inAmount, availableAmount);
            vaultData.StoredCrystalsDict[inCrystalType.ToString()] -= amountToRemove;

            outAmount += amountToRemove;
            inAmount -= amountToRemove;

            // if (vaultData.StoredCrystalsDict[inCrystalType.ToString()] <= 0)
            //     vaultData.StoredCrystalsDict.Remove(inCrystalType.ToString());

            if (inAmount <= 0) break;
        }

        this.TotalCrystals -= outAmount;

        if(inAmount > 0)
        {
            print($"<color=red>Not enough crystals of type {inCrystalType} to remove. Requested: {inAmount}, Removed: {outAmount}</color>");
        }
        else
        {
            print($"Removed {outAmount} of {inCrystalType}. Total remaining: {this.TotalCrystals}");
        }

        remainingAmount = GetCrystalsOfType(inCrystalType);

        if (this.IsPlayer)
            HUD.OnCrystalsChanged?.Invoke(PlayerData.Data.ShipData);

        return outAmount > 0;
    }

    public void AddEnergy(float inAmount)
    {
        this.EnergyLevel += inAmount;

        //TODO: play particles, sounds, etc.
    }

    public void AddCredits(int inAmountToAdd)
    {
        PlayerData.Data.AddCredits(inAmountToAdd);
    }

    public void SetPointEffectorEnabled(bool inIsEnabled)
    {
        if (this.pointEffector2D == null) return;

        this.pointEffector2D.enabled = inIsEnabled;
    }

    /// <summary>
    /// Called from timeline animation
    /// </summary>
    public void ChangeLasersToOpen()
    {
        this.isLaserAnimOpen = true;
    }
    /// <summary>
    /// Called from timeline animation
    /// </summary>
    public void ChangeLasersToClosed()
    {
        this.isLaserAnimOpen = false;
    }

    public override void Reset()
    {
        base.Reset();
    }

    protected override void Reinitialize()
    {
        Init(this.shipData, this.IsPlayer);
    }
}