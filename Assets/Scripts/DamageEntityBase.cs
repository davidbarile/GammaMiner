using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public abstract class DamageEntityBase : MonoBehaviour
{
    new protected Collider2D collider2D;

    public int Damage;
    public float DamagePerSecond;

    public Action OnDamage;
    public Action OnStayDamage;

    protected float damageOverTime;

    protected MiningToolConfig miningToolConfig;

    protected virtual void Awake()
    {
        this.collider2D = this.GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        TickManager.OnTick += Tick;
    }

    private void OnDisable()
    {
        TickManager.OnTick -= Tick;
    }
    protected virtual void Tick()
    {
        //override in derived classes
    }
    
    public void SetMiningToolConfig(MiningToolConfig inConfig)
    {
        this.miningToolConfig = inConfig;
    }

    protected float GetMiningToolDamageMultiplier(HealthEntity hitHealthEntity)
    {
        if(hitHealthEntity.TryGetComponent<Rock>(out var hitRock))
        {
            if (this.miningToolConfig != null && hitRock != null && hitRock.RockData != null)
            {
                return this.miningToolConfig.GetDamageMultiplier(hitRock.RockData.RockType);
            }
        }
        return 1f;
    }
}