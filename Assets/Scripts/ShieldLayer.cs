using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class ShieldLayer : MonoBehaviour
{
    public float ShieldLayerHealth;
    public float ShieldLayerMaxHealth;

    [Space]
    [SerializeField] private string shieldLayerName;
    [SerializeField] private bool shouldRandomizeBricksShowHide = true;

    [Space]
    [SerializeField] private ShieldBrick[] shieldBricks;
    [SerializeField] private Gradient brickHealthGradient;

    private List<ShieldBrick> randomizedChargeBricks = new();
    private List<ShieldBrick> randomizedDischargeBricks = new();

    private Action<float> onBrickHealthChanged;

    [Button(ButtonSizes.Large)]
    private void NameBricks()
    {
        for (int i = 0; i < this.shieldBricks.Length; i++)
        {
            var brick = this.shieldBricks[i];
            brick.name = $"{this.shieldLayerName}_{i}";
        }
    }

    [Button(ButtonSizes.Large)]
    private void RefreshBricksArray()
    {
        this.shieldBricks = this.GetComponentsInChildren<ShieldBrick>(true);
    }

    public float InitShieldBricks(float inMaxHealth, Action<float> inOnBrickHealthChanged)
    {
        this.ShieldLayerHealth = 0;
        this.ShieldLayerMaxHealth = 0;

        this.onBrickHealthChanged = inOnBrickHealthChanged;

        foreach (var shieldBrick in this.shieldBricks)
        {
            shieldBrick.HealthGradient = this.brickHealthGradient;
            this.ShieldLayerMaxHealth += inMaxHealth;
            shieldBrick.MaxHealth = inMaxHealth;
            shieldBrick.Health = 0;
            shieldBrick.OnBrickHealthChanged = UpdateShieldLayerHealth;
        }

        return this.ShieldLayerMaxHealth;
    }

    public float ChargeShieldBricks(float inEnergyAmount)
    {
        if (this.randomizedChargeBricks.Count == 0)
        {
            this.randomizedChargeBricks = this.shieldBricks.ToList();
            this.randomizedDischargeBricks = this.shieldBricks.ToList();

            if(this.shouldRandomizeBricksShowHide)
            {
                this.randomizedChargeBricks.RandomizeList();
                this.randomizedDischargeBricks.RandomizeList();
            }
        }

        while (inEnergyAmount > 0 && !this.AreAllShieldBricksCharged)
        {
            foreach (var shieldBrick in this.randomizedChargeBricks)
            {
                if (shieldBrick.Health < shieldBrick.MaxHealth)
                {
                    shieldBrick.Health += inEnergyAmount;

                    if (shieldBrick.Health > shieldBrick.MaxHealth)
                    {
                        var over = shieldBrick.Health - shieldBrick.MaxHealth;
                        inEnergyAmount = over;

                        shieldBrick.Health = shieldBrick.MaxHealth;
                    }
                    else
                        return 0;
                }
            }
        }

        return inEnergyAmount;//send back any leftover energy
    }

    public float DischargeShieldBricks(float inEnergyAmount)
    {
        this.randomizedChargeBricks.Clear();

        while (inEnergyAmount > 0 && this.AreAnyShieldBricksCharged)
        {
            foreach (var shieldBrick in this.randomizedDischargeBricks)
            {
                if (shieldBrick.Health > 0)
                {
                    shieldBrick.Health -= inEnergyAmount;

                    if (shieldBrick.Health < 0)
                    {
                        var under = shieldBrick.Health;
                        inEnergyAmount = under;

                        shieldBrick.Health = 0;
                    }
                    else
                        return 0;
                }
            }
        }

        return inEnergyAmount;//send back any leftover energy
    }

    private void UpdateShieldLayerHealth(float inValue)
    {
        this.ShieldLayerHealth += inValue;

        //Debug.Log($"ShieldLayer.UpdateShieldLayerHealth({inValue})   this.shieldLayerHealth = { this.shieldLayerHealth}");

        this.onBrickHealthChanged?.Invoke(inValue);
    }

    public bool AreAllShieldBricksCharged
    {
        get { return this.ShieldLayerHealth == this.ShieldLayerMaxHealth; }
    }

    public bool AreAnyShieldBricksCharged
    {
        get { return this.ShieldLayerHealth > 0; }
    }
}
