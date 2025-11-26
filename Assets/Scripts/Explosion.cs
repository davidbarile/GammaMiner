using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalData;

public class Explosion : MonoBehaviour
{
    public struct DamagedObjectData
    {
        public float Distance;
        public HealthEntity HealthEntity;
        public int Damage;
    }

    public enum EExplosionIgnore
    {
        None,
        Missile,
        Rock,
        All
    }

    private static WaitForSeconds _waitForSeconds0_05 = new(0.05f);

    [Space]
    [Range(0, 10)] public float ForceMultiplier = 1;
    [Range(0, 50)] public float EffectRadius = 5;

    [Header("0 = do not delete")]
    [Range(0, 10)] public float DelayToDelete = 2;

    [SerializeField] private SpriteRenderer circle;

    [Header("Particles")]
    [SerializeField] private bool shouldColorParticles;

    [Header("Serialize automatically on Awake()")]
    [SerializeField] private ParticleSystem[] particles;
    [Header("Serialize manually")]
    [SerializeField] private ParticleSystem[] piecesParticles;
    [SerializeField] private ParticleSystem[] smokeParticles;

    private List<DamagedObjectData> damagedHealthEntityDatas = new();

    private Collider2D[] objectsInExplosionRadius;

    private MiningToolConfig miningToolConfig;

    private void Awake()
    {
        if (this.particles.Length == 0)
            this.particles = this.GetComponentsInChildren<ParticleSystem>();
    }

    private void OnValidate()
    {
        if (this.circle)
            this.circle.transform.localScale = Vector3.one * this.EffectRadius * 2;

        if (this.particles.Length == 0)
            this.particles = this.GetComponentsInChildren<ParticleSystem>();
    }
    
    public void SetMiningToolConfig(MiningToolConfig inConfig)
    {
        this.miningToolConfig = inConfig;
    }

    public void Explode(int inDamage = 0, HealthEntity inHitHealthEntity = null, bool inIsLockedTarget = false, float inLockTargetDamageMultiplier = 1, bool inDistanceAffectsDamage = true, EExplosionIgnore inExplosionIgnore = EExplosionIgnore.None)
    {
        this.damagedHealthEntityDatas.Clear();

        if (this.EffectRadius > 0)
        {
            this.objectsInExplosionRadius = Physics2D.OverlapCircleAll(this.transform.position, this.EffectRadius);

            foreach (var item in this.objectsInExplosionRadius)
            {
                if (!item.gameObject.activeInHierarchy) continue;

                if (!item.TryGetComponent<Rigidbody2D>(out var rb)) continue;

                var distance = item.transform.position - this.transform.position;

                if (distance.magnitude > 0)
                {
                    var explosionForce = this.ForceMultiplier / distance.magnitude * 500;
                    rb.AddForce(distance.normalized * explosionForce, ForceMode2D.Impulse);

                    var healthObj = HealthEntity.GetHealthEntity(item.gameObject);

                    if (healthObj == null) continue;

                    bool shouldIgnore = false;

                    Rock hitRock = null;

                    if (item.TryGetComponent<ShieldBrick>(out var brick))
                    {
                        shouldIgnore = true;
                    }
                    else if (inExplosionIgnore == EExplosionIgnore.Missile)
                    {
                        if (item.TryGetComponent<Missile>(out var missile))
                            shouldIgnore = true;
                    }
                    else if (inExplosionIgnore == EExplosionIgnore.Rock)
                    {
                        if (item.TryGetComponent<Rock>(out hitRock))
                            shouldIgnore = true;
                    }

                    if (shouldIgnore) continue;

                    var factor = 1f;
                    if (this.miningToolConfig != null && hitRock != null)
                    {
                        factor = this.miningToolConfig.GetDamageMultiplier(hitRock.RockData.RockType);
                    }

                    float damageByDistanceCalc = 1 - Mathf.Min(distance.magnitude / this.EffectRadius, 1);
                    float damagePercent = inDistanceAffectsDamage ? damageByDistanceCalc : 1;
                    var damage = Mathf.RoundToInt(inDamage * damagePercent * factor);

                    // print($"Explosion.Explode()   healthObj = {healthObj.name}   hitRock = {hitRock?.name}   factor = {factor}   damage = {damage}   inDamage = {inDamage}");

                    if (healthObj == inHitHealthEntity)
                    {
                        damagePercent = 1;

                        if (inIsLockedTarget)
                            damagePercent *= inLockTargetDamageMultiplier;

                        damage = Mathf.RoundToInt(inDamage * damagePercent * factor);

                        if (hitRock != null && hitRock.TryGetComponent<RockLootSpawner>(out var rockLootSpawner))
                        {
                            rockLootSpawner.SetMiningToolData(this.miningToolConfig);
                        }

                        // if (inIsLockedTarget)
                        //     Debug.Log($"LOCKED:{healthObj.name}{healthObj.gameObject.GetInstanceID()}   frame = {Time.frameCount}", this.gameObject);

                        healthObj.TakeDamage(damage);
                    }
                    else
                    {
                        var damagedObj = new DamagedObjectData()
                        {
                            Distance = distance.magnitude,
                            HealthEntity = healthObj,
                            Damage = damage
                        };

                        this.damagedHealthEntityDatas.Add(damagedObj);
                    }

                    //Debug.Log($"inDamage = {inDamage}   damage = {damage}     damagePercent = {damagePercent}    distance.magnitude = {distance.magnitude}   explosionForce = {explosionForce}    name = {healthObj.name}   ignore = {inExplosionIgnore}", healthObj.gameObject);                
                }
            }
        }

        PlayParticles();

        if (this.damagedHealthEntityDatas.Count > 0)
            StartCoroutine(DelayedDamageCo());

        if (this.DelayToDelete > 0)
            Invoke(nameof(Delete), this.DelayToDelete);
    }

    private IEnumerator DelayedDamageCo()
    {
        this.damagedHealthEntityDatas.Sort((a, b) => a.Distance.CompareTo(b.Distance));

        foreach (var damagedObj in this.damagedHealthEntityDatas)
        {
            yield return _waitForSeconds0_05;

            if (damagedObj.HealthEntity.TryGetComponent<RockLootSpawner>(out var rockLootSpawner))
            {
                rockLootSpawner.SetMiningToolData(this.miningToolConfig);
            }
                        
            damagedObj.HealthEntity.TakeDamage(damagedObj.Damage);
        }

        this.damagedHealthEntityDatas.Clear();
    }

    public void PlayParticles()
    {
        foreach (var ps in this.particles)
        {
            ps.Play();
        }
    }

    public void SetParticleColors(Color inColor)
    {
        if(!this.shouldColorParticles) 
            return;

        Color darkColor = Color.Lerp(inColor, Color.black, .2f);
        foreach (var ps in this.piecesParticles)
        {
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(inColor, darkColor);
        }

        Color greyColor1 = Color.Lerp(inColor, Color.grey, .25f);
        Color greyColor2 = Color.Lerp(inColor, Color.grey, .65f);
        foreach (var ps in this.smokeParticles)
        {
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(greyColor1, greyColor2);
        }
    }

    private void Delete()
    {
        this.miningToolConfig = null;
        Pool.Despawn(this.gameObject);
    }

    // private void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if (collision.gameObject != null)
    //     {
    //         Explode();
    //     }
    // }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawSphere(this.transform.position, this.EffectRadius);
    //}
}