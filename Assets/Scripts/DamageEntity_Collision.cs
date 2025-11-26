using System;
using UnityEngine;

/// <summary>
/// Use for Collisions that deliver force
/// In most cases, DamagePerSecond should be 0
/// </summary>
public class DamageEntity_Collision : DamageEntityBase
{
    private HealthEntity hitHealthEntity;

    protected override void Awake()
    {
        base.Awake();
        this.collider2D.isTrigger = false;
    }

    private void OnCollisionEnter2D(Collision2D inCollision)
    {
        this.damageOverTime = 0;

        if (inCollision.gameObject.TryGetComponent(out this.hitHealthEntity))
        {
            if (this.Damage == 0) return;//this.hitHealthEntity has been set, but no damage to deal

            DamageHealthEntities(this.Damage, this.OnDamage);
        }
    }

    private void OnCollisionStay2D(Collision2D inCollision)
    {
        if(this.hitHealthEntity == null)
            inCollision.gameObject.TryGetComponent(out this.hitHealthEntity);
    }

    //if target is not moving, OnCollisionStay may not be called every frame, so we use Update
    protected override void Tick()
    {
        if (this.DamagePerSecond == 0 || this.hitHealthEntity == null) return;

        this.damageOverTime += this.DamagePerSecond;
        int damageThisFrame = Mathf.FloorToInt(this.damageOverTime);
        this.damageOverTime -= damageThisFrame;//passes remainder to next frame

        DamageHealthEntities(damageThisFrame, this.OnStayDamage);
    }

    private void OnCollisionExit2D(Collision2D inCollision)
    {
        this.hitHealthEntity = null;
    }
    
    private void DamageHealthEntities(float inDamage, Action inOnDamage)
    {
        if (this.hitHealthEntity is HealthEntityParent parentHealthEntity)
        {
            if (parentHealthEntity.IsDead)
                return;

            //only do damage to parent entity if it has no children remaining
            foreach (var child in parentHealthEntity.ChildHealthEntities)
            {
                if (child != null && !child.IsDead)
                    return;
            }
        }
        else
        {
            //child entity
            if (this.hitHealthEntity.ParentHealthEntity != null && !this.hitHealthEntity.ParentHealthEntity.IsDead)
            {
                foreach (var child in this.hitHealthEntity.ParentHealthEntity.ChildHealthEntities)
                {
                    if (child != null && !child.IsDead)
                    {
                        child.TakeDamage(inDamage);
                        inOnDamage?.Invoke();
                    }
                }

                return;
            }
        }

        var factor = GetMiningToolDamageMultiplier(this.hitHealthEntity);
        var damage = inDamage * factor;

        //print($"DamageEntity_Collision.DamageHealthEntities()   damage = {damage}/{inDamage}   factor = {factor}    miningToolConfig = {this.miningToolConfig?.MiningToolType}");

        this.hitHealthEntity.TakeDamage(damage);
        inOnDamage?.Invoke();
    }
}