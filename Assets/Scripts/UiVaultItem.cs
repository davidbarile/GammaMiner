using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static CrystalData;

public class UiVaultItem : MonoBehaviour
{
    [SerializeField] private int numCrystalTypesPerLine = 3;
    [SerializeField] private TextMeshProUGUI vaultNameText;
    [SerializeField] private TextMeshProUGUI vaultDataText;
    [SerializeField] private TextMeshProUGUI percentText;

    [Space, SerializeField] private Image vaultGraphic;
    [SerializeField] private Image[] crystalAmounts;
    [SerializeField] private RectTransform crystalAmountsContainer;

    public void Configure(VaultData inVaultData)
    {
        this.vaultNameText.text = inVaultData.SubTitle;

        this.vaultGraphic.sprite = GlobalData.GetVaultIconSprite(inVaultData.SpriteIndex).MonotoneSprite;

        var sb = new StringBuilder();
        sb.AppendLine($"Capacity: {inVaultData.Capacity}");

        foreach (var crystalAmount in this.crystalAmounts)
        {
            crystalAmount.gameObject.SetActive(false);
        }

        var totalHeight = crystalAmountsContainer.rect.height;
        var hasCrystalsCounter = 0;
        var allCounter = 0;

        sb.AppendLine($"Payload:");

        foreach (var crystalsQuantity in inVaultData.StoredCrystalsDict)
        {
            var crystalTypeString = crystalsQuantity.Key;
            var crystalType = (ECrystalType)Enum.Parse(typeof(ECrystalType), crystalTypeString);
            var amount = crystalsQuantity.Value;

            var crystalHexColor = GlobalData.GetCrystalColor(crystalType).ToHexString();
            var crystalSpriteName = $"<sprite name=\"Crystals_Fill\" color=#{crystalHexColor}><sprite name=\"Crystals_Outline\">";

            sb.Append($" {crystalSpriteName}{amount}  ");

            if (allCounter % numCrystalTypesPerLine == numCrystalTypesPerLine - 1)
                sb.Append("\n");

            ++allCounter;

            if (amount <= 0) continue;

            var crystalAmountDisplay = this.crystalAmounts[hasCrystalsCounter];
            crystalAmountDisplay.gameObject.SetActive(true);
            crystalAmountDisplay.color = GlobalData.GetCrystalColor(crystalType);

            var rt = crystalAmountDisplay.GetComponent<RectTransform>();
            var crystalTypeHeight = amount / (float)inVaultData.Capacity * totalHeight;

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, crystalTypeHeight);

            ++hasCrystalsCounter;
        }

        // Print the true values of the enum flags inVaultData.AllowedCrystalTypes
        // var allowedTypes = inVaultData.AllowedCrystalTypes;
        // var allowedNames = Enum.GetValues(allowedTypes.GetType());
        // foreach (var value in allowedNames)
        // {
        //     int intValue = (int)value;
        //     if (intValue != 0 && allowedTypes.HasFlag((Enum)value))
        //     {
        //         sb.Append($"{value}, ");
        //     }
        // }

        // sb.Remove(sb.Length - 2, 2); // Remove the last comma and space

        // sb.AppendLine($"\nRadiation Shielding: {inVaultData.RadiationShielding * 100}%");
        // sb.AppendLine($"Radiation MapNum: {inVaultData.RadiationLevel}/{inVaultData.MaxRadiationLevel}");

        this.vaultDataText.text = sb.ToString();

        var percentUsed = inVaultData.GetUsedStorage() / (float)inVaultData.Capacity;
        var roundedPercent = Mathf.RoundToInt(percentUsed * 100);
        this.percentText.text = $"{roundedPercent}%";
    }
}