using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiReactorAndBatteryDisplay : MonoBehaviour
{
    [SerializeField] private UICountDisplay[] batteryDisplays;
    [SerializeField] private Image reactorGlow;
    [SerializeField] private Image[] reactorPips;
    [SerializeField] private TMP_Text energyLevelText;
    [SerializeField] private Image energyTextBackground;

    public void SetMaxBatteryLevel(int inMax)
    {
        foreach (var batteryDisplay in this.batteryDisplays)
        {
            batteryDisplay.SetMax(inMax);
        }
    }

    public void SetBatteryLevel(int inValue)
    {
        foreach (var batteryDisplay in this.batteryDisplays)
        {
            batteryDisplay.SetValue(inValue);
        }
    }

    public void SetGradient(Gradient inGradient)
    {
        foreach (var batteryDisplay in this.batteryDisplays)
        {
            batteryDisplay.GetComponent<Healthbar>().SetGradient(inGradient);
        }
    }

    public void SetColors(Color inColor)
    {
        var lightColor = Color.Lerp(inColor, Color.white, 0.5f);
        var darkColor = Color.Lerp(inColor, Color.black, 0.5f);
        this.reactorGlow.color = lightColor;
        foreach (var pip in this.reactorPips)
        {
            pip.color = lightColor;
        }

        this.energyLevelText.color = Color.Lerp(inColor, Color.white, 0.95f);
        this.energyTextBackground.color = darkColor;
    }

    //TODO: reactor particles
}