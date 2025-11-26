using UnityEngine;

public class ShieldGenerator : ShipComponent
{
    public float TotalShieldsHealth;
    public float TotalShieldsMaxHealth;

    [SerializeField] private ShieldLayer[] shieldLayers;

    public bool AreAllShieldsCharged
    {
        get { return this.TotalShieldsHealth == this.TotalShieldsMaxHealth; }
    }

    public bool AreAnyShieldsCharged
    {
        get { return this.TotalShieldsHealth > 0; }
    }

    public void InitShieldLayer(int inShieldLayerIndex, float inMaxHealth)
    {
        this.shieldLayers[inShieldLayerIndex].gameObject.SetActive(inMaxHealth > 0);

        this.TotalShieldsHealth = 0;

        if(inMaxHealth <= 0)
            return;
   
        this.TotalShieldsMaxHealth += this.shieldLayers[inShieldLayerIndex].InitShieldBricks(inMaxHealth, UpdateAllShieldsHealth);

        //print($"Shields.InitShieldLayer({inShieldLayerIndex}, {inMaxHealth})   this.TotalShieldsHealth = {this.TotalShieldsHealth}   this.TotalShieldsMaxHealth = {this.TotalShieldsMaxHealth}");
    }

    public void ChargeShields(float inEnergyAmount)
    {
        if (!this.gameObject.activeInHierarchy)
            return;
            
        while (inEnergyAmount > 0 && !this.AreAllShieldsCharged)
        {
            foreach (var shieldLayer in this.shieldLayers)
            {
                if (shieldLayer.gameObject.activeInHierarchy && !shieldLayer.AreAllShieldBricksCharged)
                {
                    float remainingEnergy = shieldLayer.ChargeShieldBricks(inEnergyAmount);

                    inEnergyAmount = remainingEnergy; //if shield layer empty, return energy to subtract from next shield layer

                    if (inEnergyAmount <= 0)
                        return;
                }
            }
        }
    }

    public void DischargeShields(float inEnergyAmount)
    {
        if (!this.gameObject.activeInHierarchy)
            return;
            
        while (inEnergyAmount > 0 && this.AreAnyShieldsCharged)
        {
            for (int i = this.shieldLayers.Length - 1; i >= 0; i--)
            {
                var shieldLayer = this.shieldLayers[i];

                if (shieldLayer.gameObject.activeInHierarchy &&shieldLayer.AreAnyShieldBricksCharged)
                {
                    float remainingEnergy = shieldLayer.DischargeShieldBricks(inEnergyAmount);

                    inEnergyAmount = remainingEnergy; //if shield layer empty, return energy to subtract from next shield layer
                }
            }
        }
    }

    private void UpdateAllShieldsHealth(float inValue)
    {
        //Debug.Log($"Shields.UpdateAllShieldsHealth({inValue})   this.TotalShieldsHealth = {this.TotalShieldsHealth}");

        this.TotalShieldsHealth += inValue;
    }
}