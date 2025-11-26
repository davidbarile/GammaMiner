using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiCrystalPriceItem : MonoBehaviour
{
    [SerializeField] private Image crystalIcon;
    [SerializeField] private TMP_Text crystalNameText;
    [SerializeField] private TMP_Text crystalPriceText;
    [SerializeField] private TMP_Text crystalBasePriceText;
    [SerializeField] private TMP_Text percentText;

    [SerializeField] private Color underValueColor;
    [SerializeField] private Color overValueColor;

    public void Configure(CrystalData inCrystalData, int inBasePrice, int inPercent)
    {
        this.crystalIcon.color = GlobalData.GetCrystalColor(inCrystalData.CrystalType);
        this.crystalNameText.text = GlobalData.GetCrystalName(inCrystalData.CrystalType);
            
        this.crystalPriceText.text = $"<sprite name=\"Credits\">{inBasePrice + inBasePrice * (inPercent / 100f):N1}";

        this.crystalBasePriceText.text = $"Base Price: {inBasePrice}";
        this.crystalBasePriceText.gameObject.SetActive(inPercent != 0);

        this.percentText.text = inPercent < 0 ? $"{inPercent}%" : $"+{inPercent}%";
        this.percentText.color = inPercent < 0 ? underValueColor : overValueColor;
        this.percentText.gameObject.SetActive(inPercent != 0);
    }
}