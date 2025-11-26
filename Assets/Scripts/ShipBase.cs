using UnityEngine;
using System.Collections;
using TMPro;
using Sirenix.OdinInspector;
using static HealthData;

public class ShipBase : MonoBehaviour
{
    protected enum TurnDirection
    {
        Straight,
        Left,
        Right
    }

    protected enum EngineState
    {
        Off,
        Thrust,
        Brake
    }

    public ShipData ShipData => this.shipData;

    [Header("For Player, set in Init() from GameManager")]
    [SerializeField] protected ShipData shipData;
    [SerializeField] protected ShipConfig shipConfig;

    [Space, SerializeField] private TextMeshPro shipNameText;

    [ReadOnly, Space, SerializeField] private int totalShipHealth;

    [Space]
    public HealthEntityParent HullHealth;
    public HealthEntity[] HealthEntities;

    public int NumRailRounds { get; protected set; }
    public int MaxRailRounds { get; protected set; }

    public int NumMissiles { get; protected set; }
    public int MaxMissiles { get; protected set; }

    [Space]
    [SerializeField] protected Rigidbody2D rigidBody2d;

    [Space]
    [Header("Must sync with ShipConfig array order")]
    [SerializeField] protected Mount[] mounts;

    [Space]
    
    [SerializeField] protected LaserCannon[] laserCannons;
    [Range(0, 1)][SerializeField] protected float energyConsumptionModifier = .1f;//move to appropriate place

    [Space]
    [SerializeField] protected Railgun[] railguns;
    [SerializeField] protected MissileLauncher[] missileLaunchers;

    [Range(0f,1f), SerializeField] private float timeBetweenMissileShots = 0f;

    [Space]
    [SerializeField] protected ShieldGenerator shieldGenerator;

    [Space]
    [SerializeField] protected Thruster[] forwardEngines;
    [SerializeField] protected Thruster[] leftTurnEngines;
    [SerializeField] protected Thruster[] rightTurnEngines;

    [Space]
    [SerializeField] protected Explosion deathExplosionPrefab;
    [SerializeField] protected Explosion childExplosionPrefab;

    [Space]
    [Tooltip("Parent Object for mini-map icons")]
    [SerializeField] protected GameObject miniMapIcon;
    [SerializeField] protected SpriteRenderer[] miniMapIcons;

    protected bool isInitialized;

    protected TurnDirection turnDirection;
    protected EngineState engineState;

    protected float rbDrag;
    protected Vector2 rbCachedVelocity;

    protected SpriteRenderer miniMapIconRenderer;

    protected int timeOfLastMissileShot = -1;
    protected float missileCooldownTime = -1;

    private readonly float rotationMultiplier = 100f;

    protected virtual void Start()
    {
        //TODO: set icon sprite based on player sensors
        SetMiniMapIcon(0);

        if (this.shipConfig && !this.isInitialized)
        {
            this.shipData = ShipData.GetDataFromConfig(this.shipConfig);
            Init(this.shipData, false);
        }
    }

    protected virtual void OnValidate()
    {
        if(this.shipConfig != null)
            this.totalShipHealth = this.shipConfig.GetTotalShipHealth();
    }

    public virtual void Init(ShipData inShipData, bool inIsPlayer)
    {
        this.isInitialized = true;

        this.shipData = inShipData;

        if (this.shipData == null)
        {
            Debug.Log($"<color=red>ShipData is null! Cannot initialize ship.  {name}</color>", this);
            return;
        }

        if (inShipData.HealthDatas.Count == 0)
        {
            Debug.Log($"<color=red>inShipData.HealthDatas.Count = 0  {name}</color>", this);
            return;
        }

        this.rbDrag = this.rigidBody2d.linearDamping;

        InitShipHealths();
        InitThrusters();
        InitRailguns();
        InitMissileLaunchers();
        InitLaserCannons();
        InitShieldGenerators();

        void InitShipHealths()
        {
            var hullHealthDatas = inShipData.GetHeathsOfType(EHealthDataType.Hull);
            if (hullHealthDatas.Count == 0)
            {
                Debug.LogError($"<color=red>Hull health data is empty for {name}</color>", this);
                return;
            }
            var hullHealthData = hullHealthDatas[0];

            this.HullHealth.Init(hullHealthData.Health, hullHealthData.MaxHealth, inIsPlayer);
            //remove default ReturnToPool on die behavior on children
            this.HullHealth.OnDie = null;
            this.HullHealth.OnDie += HandleDie;

            var numHealthEntities = Mathf.Min(this.HealthEntities.Length, inShipData.HealthDatas.Count);

            //skip the first health entity, which is the hull 
            for (int i = 1; i < numHealthEntities; i++)
            {
                var healthEntity = this.HealthEntities[i];
                var data = inShipData.HealthDatas[i];

                healthEntity.Init(data.Health, data.MaxHealth);
                //healthEntity.OnDie = null;
                healthEntity.OnChildDie += HandleChildDie;
            }
        }

        void InitThrusters()
        {
            var thrusterHealths = this.shipData.GetHeathsOfType(EHealthDataType.Thruster);
            var numThrusters = Mathf.Min(thrusterHealths.Count, inShipData.ThrusterDatas.Count);

            //print($"{this.name}  InitThrusters()   inShipData.ThrusterDatas = {inShipData.ThrusterDatas.Count}  thrusterHealths.Count = {thrusterHealths.Count}  numThrusters = {numThrusters}");

            for (var i = 0; i < numThrusters; i++)
            {
                var thruster = this.forwardEngines[i];
                var tData = inShipData.ThrusterDatas[i];
                var tHealthData = thrusterHealths[i];

                var shouldShow = i < inShipData.ThrusterDatas.Count;
                thruster.gameObject.SetActive(shouldShow);
            }
        }

        void InitRailguns()
        {
            this.NumRailRounds = 0;
            this.MaxRailRounds = 0;

            var numRailguns = Mathf.Min(this.railguns.Length, inShipData.RailgunDatas.Count);

            for (var i = 0; i < numRailguns; i++)
            {
                var railgun = this.railguns[i];
                var rgData = inShipData.RailgunDatas[i];

                var shouldShow = i < inShipData.RailgunDatas.Count;
                railgun.gameObject.SetActive(shouldShow);

                if (shouldShow)
                {
                    railgun.Init(rgData);
                    this.NumRailRounds += rgData.NumRoundsLight;
                    this.NumRailRounds += rgData.NumRoundsMedium;
                    this.NumRailRounds += rgData.NumRoundsHeavy;

                    this.MaxRailRounds += rgData.MaxRoundsLight;
                    this.MaxRailRounds += rgData.MaxRoundsMedium;
                    this.MaxRailRounds += rgData.MaxRoundsHeavy;

                    //Debug.Log($"<color=white>InitRailguns()  {name}  i = {i}  numRailguns = {numRailguns}.    Light = {rgData.NumRoundsLight}   Medium = {rgData.NumRoundsMedium}   Heavy = {rgData.NumRoundsHeavy}</color>", this);
                }
            }

            //Debug.Log($"<color=yellow>InitRailguns()  {name}  NumRailRounds = {this.NumRailRounds}  MaxRailRounds = {this.MaxRailRounds}</color>", this);

            SetRailRoundsCount(this.NumRailRounds);
        }

        void InitMissileLaunchers()
        {
            this.NumMissiles = 0;
            this.MaxMissiles = 0;

            for (var i = 0; i < this.missileLaunchers.Length; i++)
            {
                var ml = this.missileLaunchers[i];

                var isValid = i < inShipData.MissileLauncherDatas.Count && ml.IsActive;

                if (isValid)
                {
                    var mlData = inShipData.MissileLauncherDatas[i];
                    ml.Init(mlData);
                    this.NumMissiles += mlData.NumMissiles;
                    this.MaxMissiles += mlData.MaxMissiles;
                }
                else
                {
                    ml.Init(null);
                }
            }

            SetMissilesCount(this.NumMissiles);
        }

        void InitLaserCannons()
        {
            for (int i = 0; i < this.laserCannons.Length; i++)
            {
                var laserCannon = this.laserCannons[i];

                var isValid = i < inShipData.LaserCannonDatas.Count;
                laserCannon.SetDummy(!isValid);

                //print($"InitLaserCannons()   i = {i}   isValid = {isValid}    inShipData.LaserCannonDatas.Count = {inShipData.LaserCannonDatas.Count}");

                if (isValid)
                {
                    var data = inShipData.LaserCannonDatas[i];
                    laserCannon.Init(data);

                    //NO - going to have lasers disabled in hierarchy determine if they are active

                    // laserCannon.HealthEntity.AddHealth(laserHealths[i].Health);
                    // laserCannon.HealthEntity.OnDie = null;
                }
                else
                    laserCannon.Init(null);
            }

            //Debug.Log($"<color=yellow>InitLaserCannons()  {name}  numLaserCannons = {numLaserCannons}  inShipData.LaserCannonDatas.Count = {inShipData.LaserCannonDatas.Count}</color>", this);
        }

        void InitShieldGenerators()
        {
            var hasShieldGenerator = this.shipData.ShieldGeneratorData != null && this.shipData.ShieldGeneratorData.MaxShieldRings > 0;

            this.shieldGenerator.gameObject.SetActive(hasShieldGenerator);

            if (!hasShieldGenerator) return;

            this.shieldGenerator.InitShieldLayer(0, inShipData.ShieldGeneratorData.InnerShieldBrickHealth);
            this.shieldGenerator.InitShieldLayer(1, inShipData.ShieldGeneratorData.MiddleShieldBrickHealth);
            this.shieldGenerator.InitShieldLayer(2, inShipData.ShieldGeneratorData.OuterShieldBrickHealth);
        }

        /*this.turnDirection = TurnDirection.Straight;

        foreach (var jet in this.leftTurnEngines)
            jet.SetAnimatorState(JetEngine.ThrustMode.Off);

        foreach (var jet in this.rightTurnEngines)
            jet.SetAnimatorState(JetEngine.ThrustMode.Off);*/
    }

    protected virtual void OnDestroy()
    {
        //TODO: remove this - these should be handled by the HealthEntity
        this.HullHealth.OnDestroy();

        foreach (var health in this.HealthEntities)
        {
            health.OnDestroy();
        }
    }

    public virtual void Reset()
    {
        this.gameObject.SetActive(true);

        foreach (var healthEntity in this.HealthEntities)
        {
            healthEntity.AddHealth(healthEntity.MaxHealth);
            healthEntity.gameObject.SetActive(true);
        }

        ResetRigidbody();

        Reinitialize();
    }

    protected virtual void Reinitialize()
    {
        Init(this.shipData, false);
    }

    protected void SetShipNameText(string inName)
    {
        if (this.shipNameText != null)
            this.shipNameText.text = inName;
    }

    public virtual int AddRailRounds(int inAmountToAdd)
    {
        if (!this.gameObject.activeInHierarchy) return inAmountToAdd;

        var numRailguns = Mathf.Min(this.railguns.Length, this.shipData.RailgunDatas.Count);

        if (numRailguns == 0 || this.NumRailRounds == this.MaxRailRounds) return inAmountToAdd;

        var numRailRounds = 0;

        while (inAmountToAdd > 0 && numRailRounds < this.MaxRailRounds)
        {
            numRailRounds = 0;

            for (var i = 0; i < numRailguns; i++)
            {
                var rgData = this.shipData.RailgunDatas[i];
                var railgun = this.railguns[i];

                if (rgData == null || !railgun.gameObject.activeInHierarchy) continue;

                if (inAmountToAdd > 0 && rgData.NumRoundsHeavy < rgData.MaxRoundsHeavy)
                {
                    ++rgData.NumRoundsHeavy;
                    --inAmountToAdd;
                }

                if (inAmountToAdd > 0 && rgData.NumRoundsMedium < rgData.MaxRoundsMedium)
                {
                    ++rgData.NumRoundsMedium;
                    --inAmountToAdd;
                }

                if (inAmountToAdd > 0 && rgData.NumRoundsLight < rgData.MaxRoundsLight)
                {
                    ++rgData.NumRoundsLight;
                    --inAmountToAdd;
                }

                numRailRounds += rgData.NumRoundsLight + rgData.NumRoundsMedium + rgData.NumRoundsHeavy;
            }
        }

        //Debug.Log($"<color=yellow> AddRailRounds({inAmountToAdd})  numRailRounds: {numRailRounds}   max = {this.MaxRailRounds}</color>");

        SetRailRoundsCount(numRailRounds);//TODO: maybe change to show light, med, heavy railgun rounds, in which case connect to railgun data

        return inAmountToAdd;
    }

    public virtual void SetRailRoundsCount(int inAmount)
    {
        //TODO: connect this to data, and specify light, medium, heavy rounds
        this.NumRailRounds = Mathf.Clamp(inAmount, 0, this.MaxRailRounds);
    }

    public virtual int AddMissiles(int inAmountToAdd)
    {
        if (!this.gameObject.activeInHierarchy) return inAmountToAdd;

        var numMissileLaunchers = Mathf.Min(this.missileLaunchers.Length, this.shipData.MissileLauncherDatas.Count);

        if (numMissileLaunchers == 0 || this.NumMissiles == this.MaxMissiles) return inAmountToAdd;

        var numMissiles = 0;

        while (inAmountToAdd > 0 && numMissiles < this.MaxMissiles)
        {
            numMissiles = 0;

            for (var i = 0; i < numMissileLaunchers; i++)
            {
                var mlData = this.shipData.MissileLauncherDatas[i];
                var missileLauncher = this.missileLaunchers[i];

                if (mlData == null || !missileLauncher.gameObject.activeInHierarchy) continue;

                if (inAmountToAdd > 0 && mlData.NumMissiles < mlData.MaxMissiles)
                {
                    ++mlData.NumMissiles;
                    --inAmountToAdd;
                }

                // if (inAmountToAdd > 0 && mlData.NumRoundsMedium < mlData.MaxRoundsMedium)
                // {
                //     ++mlData.NumRoundsMedium;
                //     --inAmountToAdd;
                // }

                // if (inAmountToAdd > 0 && mlData.NumRoundsLight < mlData.MaxRoundsLight)
                // {
                //     ++mlData.NumRoundsLight;
                //     --inAmountToAdd;
                // }

                numMissiles += mlData.NumMissiles;
            }
        }

        //Debug.Log($"<color=yellow> AddMissiles({inAmountToAdd})  numMissiles: {numMissiles}   max = {this.MaxMissiles}</color>");

        SetMissilesCount(numMissiles);//TODO: maybe change to show light, med, heavy railgun rounds, in which case connect to railgun data

        return inAmountToAdd;
    }

    public virtual void SetMissilesCount(int inAmount)
    {
        //TODO: connect this to data, and specify missile types
        this.NumMissiles = Mathf.Clamp(inAmount, 0, this.MaxMissiles);
    }

    public void SetMiniMapIcon(int inIndex)
    {
        if (inIndex < 0 || inIndex >= this.miniMapIcons.Length)
        {
            Debug.LogError($"<color=red>SetMiniMapIcon()  Invalid index: {inIndex} for {name}</color>", this);
            return;
        }

        for(int i = 0; i < this.miniMapIcons.Length; i++)
        {
            var shouldShow = i == inIndex;
            var iconRenderer = this.miniMapIcons[i];

            if (shouldShow)
                this.miniMapIconRenderer = iconRenderer;
                
            if (iconRenderer != null)
                iconRenderer.gameObject.SetActive(false);
        }
    }

    protected virtual void HandleDie(HealthEntity inHealthEntity = null)
    {
        //Debug.Log($"HandleDie({this.name})");

        var explosion = Pool.Spawn<Explosion>(this.deathExplosionPrefab.name, GameManager.IN.ProjectilesContainer, this.transform.position, Quaternion.identity);

        explosion.Explode(100);

        //Destroy(this.gameObject);
        this.gameObject.SetActive(false);

        //Poolable.ReturnToPool(this);
    }

    /// <summary>
    /// When a nose, wing, jets is destroyed
    /// </summary>
    protected virtual void HandleChildDie(HealthEntity inHealthEntity)
    {
        //Debug.Log($"HandleChildDie({inHealthEntity.name})    this = {this.name}");
        inHealthEntity.gameObject.SetActive(false);

        var explosion = Pool.Spawn<Explosion>(this.childExplosionPrefab.name, GameManager.IN.ProjectilesContainer, this.transform.position, Quaternion.identity);

        explosion.Explode(10);
    }

    public void RotateLeft(float inAmount)
    {
        //Debug.Log("RotateLeft()");

        //this.rigidbody2D.AddTorque(this.shipData.RotationSpeed);

        var rotationSpeed = this.shipData.RotationSpeed * this.rotationMultiplier * Time.deltaTime;

        this.transform.Rotate(0, 0, rotationSpeed * -inAmount);

        if (this.turnDirection != TurnDirection.Left)
        {
            foreach (var jet in this.leftTurnEngines)
            {
                if (jet.gameObject.activeInHierarchy)
                    jet.SetAnimatorState(Thruster.ThrustMode.Start);
            }
        }

        this.turnDirection = TurnDirection.Left;
    }

    public void RotateRight(float inAmount)
    {
        //Debug.Log("RotateRight()");

        //this.rigidbody2D.AddTorque(-this.shipData.RotationSpeed);

        var rotationSpeed = this.shipData.RotationSpeed * this.rotationMultiplier * Time.deltaTime;

        this.transform.Rotate(0, 0, rotationSpeed * -inAmount);

        if (this.turnDirection != TurnDirection.Right)
        {
            foreach (var jet in this.rightTurnEngines)
            {
                if (jet.gameObject.activeInHierarchy)
                    jet.SetAnimatorState(Thruster.ThrustMode.Start);
            }
        }

        this.turnDirection = TurnDirection.Right;
    }

    public void StopRotation()
    {
        if (this.turnDirection != TurnDirection.Straight)
        {
            //Debug.Log("StopRotation()");

            foreach (var jet in this.leftTurnEngines)
            {
                if (jet.gameObject.activeInHierarchy)
                    jet.SetAnimatorState(Thruster.ThrustMode.End);
            }

            foreach (var jet in this.rightTurnEngines)
            {
                if (jet.gameObject.activeInHierarchy)
                    jet.SetAnimatorState(Thruster.ThrustMode.End);
            }

            this.turnDirection = TurnDirection.Straight;
        }
    }

    public virtual void ShootLasers()
    {
        foreach (var laserCannon in this.laserCannons)
        {
            if (laserCannon.gameObject.activeInHierarchy && laserCannon.HasData)
                laserCannon.Shoot(null, this, null);
        }
    }

    public virtual void ShootMissile()
    {
        if (this.timeOfLastMissileShot > -1 && Utils.NOW - this.timeOfLastMissileShot < this.missileCooldownTime)
            return;

        this.timeOfLastMissileShot = -1;
        this.missileCooldownTime = -1;

        StartCoroutine(ShootMissileCo());
    }

    protected virtual IEnumerator ShootMissileCo()
    {
        if (this.NumMissiles <= 0)
            yield break;

        var numMissilesFired = 0;
        this.missileCooldownTime = 0;

        foreach (var missileLauncher in this.missileLaunchers)
        {
            if (missileLauncher == null || !missileLauncher.gameObject.activeInHierarchy) continue;

            var success = missileLauncher.Shoot(this.rigidBody2d.linearVelocity);

            if (success)
            {
                ++numMissilesFired;
                this.timeOfLastMissileShot = Utils.NOW;
                this.missileCooldownTime = Mathf.Max(this.missileCooldownTime, missileLauncher.MissileLauncherData.CooldownTime);
            }
            
            if(this.timeBetweenMissileShots > 0f)
                yield return new WaitForSeconds(this.timeBetweenMissileShots * Time.deltaTime * 60f);
        }

        SetMissilesCount(this.NumMissiles - numMissilesFired);
    }

    public void ResetRigidbody()
    {
        this.rigidBody2d.linearVelocity = new Vector2();
        this.rigidBody2d.angularVelocity = 0;
    }
}