using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class UICountDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private bool shouldShowTotal = true;
    [SerializeField] private bool shouldColorTextOnDecrease;
    [ShowIf("shouldColorTextOnDecrease")]
    [SerializeField] private Color decreaseColor = Color.red;

    [ShowIf("shouldShowTotal")]
    [SerializeField] private TextMeshProUGUI totalText;
    [Space, SerializeField] private Graphic currentAmountGraphic;
    [SerializeField] private Gradient gradient;
    [Space, SerializeField] private Healthbar healthBar;

    public int Amount { get; private set; }
    private int maxAmount;
    private int lastAmount;
    private bool isDecreasing;

    public void SetMax(int inMax)
    {
        this.Amount = inMax;
        this.maxAmount = inMax;
        this.lastAmount = inMax;

        if (healthBar != null)
        {
            this.healthBar.Init(null, this.maxAmount);
        }

        RefreshDisplay();
    }

    public void SetValue(int inAmount)
    {
        this.lastAmount = this.Amount;
        this.Amount = inAmount;

        if (this.lastAmount != this.Amount)
        {
            this.isDecreasing = this.Amount < this.lastAmount;
        }
        else if (this.Amount == this.maxAmount)
        {
            this.isDecreasing = false;
        }
      
        RefreshDisplay();
        this.lastAmount = inAmount;
    }

    public void SetIconColor(Color inColor)
    {
        if (this.currentAmountGraphic)
        {
            this.currentAmountGraphic.color = inColor;
        }
    }
    
    public void SetGradient(Gradient inGradient)
    {
        this.gradient = inGradient;
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (healthBar)
        {
            this.healthBar.SetValue(this.Amount);
        }
        else if (this.totalText)
        {
            if(this.shouldColorTextOnDecrease && this.isDecreasing)
            {
                var colorHex = this.decreaseColor.ToHexString();
                this.valueText.text = $"<color=#{colorHex}>{this.Amount}</color>";
            }
            else
            {
                this.valueText.text = $"{this.Amount}";
            }
         
            this.totalText.text = $"{this.maxAmount}";
        }
        else if (this.shouldShowTotal)
        {
            if(this.shouldColorTextOnDecrease && this.isDecreasing)
            {
                var colorHex = this.decreaseColor.ToHexString();
                this.valueText.text = $"<color=#{colorHex}>{this.Amount}</color>/<size=65%>{this.maxAmount}</size>";
            }
            else
            {
                this.valueText.text = $"{this.Amount}<size=65%>/{this.maxAmount}</size>";
            }
        }
        else
        {
            if(this.shouldColorTextOnDecrease && this.isDecreasing)
            {
                var colorHex = this.decreaseColor.ToHexString();
                this.valueText.text = $"<color=#{colorHex}>{this.Amount}</color>";
            }
            else
            {
                this.valueText.text = $"{this.Amount}";
            }
        }

        if (this.currentAmountGraphic && this.maxAmount > 0)
            this.currentAmountGraphic.color = gradient.Evaluate(this.Amount / (float)this.maxAmount);
    }
}