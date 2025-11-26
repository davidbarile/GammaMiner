using System;
using UnityEngine;

/// <summary>
/// Use for lasers and zones that cause damage
/// </summary>
public class DamageEntity_Trigger : DamageEntityBase
{
    public HealthEntity hitHealthEntity;

    private Rock rock;

    protected override void Awake()
    {
        base.Awake();
        this.collider2D.isTrigger = true;
        this.rock = this.GetComponent<Rock>();
    }

    private void OnTriggerEnter2D(Collider2D inCollider)
    {
        this.damageOverTime = 0;

        if (inCollider.gameObject.TryGetComponent(out this.hitHealthEntity))
        {
            if (this.Damage == 0) return;//this.hitHealthEntity has been set, but no damage to deal

            DamageHealthEntities(this.Damage, this.OnDamage);
        }
    }

    private void OnTriggerStay2D(Collider2D inCollider)
    {
        if(this.hitHealthEntity == null)
            inCollider.gameObject.TryGetComponent(out this.hitHealthEntity);
    }

    //if target is not moving, OnTriggerStay may not be called every frame, so we use Update - doesn't work. LOL
    protected override void Tick()
    {
        if (this.DamagePerSecond == 0 || this.hitHealthEntity == null) return;

        //if rock, apply mining tool multiplier
        var miningToolMultiplier = 1f;
        if (this.miningToolConfig != null && this.rock != null && this.rock.RockData != null)
        {
            miningToolMultiplier = this.miningToolConfig.GetDamageMultiplier(this.rock.RockData.RockType);
            //print($"miningToolMultiplier = {miningToolMultiplier}   this.rock.RockData.RockType = {this.rock.RockData.RockType} ");
        }

        this.damageOverTime += this.DamagePerSecond * miningToolMultiplier;
        int damageThisFrame = Mathf.FloorToInt(this.damageOverTime);
        this.damageOverTime -= damageThisFrame;//passes remainder to next frame

        DamageHealthEntities(damageThisFrame, this.OnStayDamage);
    }

    private void OnTriggerExit2D(Collider2D inCollider)
    {
        this.miningToolConfig = null;//not sure
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

        //pass the miningToolConfig to RockLootSpawner so it can know how to mofify loot
        if (this.hitHealthEntity.TryGetComponent<RockLootSpawner>(out var rockLootSpawner))
        {
            rockLootSpawner.SetMiningToolData(this.miningToolConfig);
        }

        var factor = GetMiningToolDamageMultiplier(this.hitHealthEntity);
        var damage = Mathf.RoundToInt(inDamage * factor);

        this.hitHealthEntity.TakeDamage(damage);
        inOnDamage?.Invoke();
    }
}