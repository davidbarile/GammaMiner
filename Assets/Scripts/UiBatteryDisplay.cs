using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiBatteryDisplay : ShipComponentDisplayBase
{
    [SerializeField] private TextMeshProUGUI batteryNameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI percentText;

    [Space, SerializeField] private Image batteryIconImage;

     protected override IEnumerator InitCo()
    {
        while (PlayerData.Data == null || PlayerData.Data.ShipData == null)
        {
            yield return null; // Wait until PlayerData and ShipData are initialized
        }

        HUD.OnCrystalsChanged += OnCrystalsChanged;
        PlayerData.OnShipDataChanged += OnShipDataChanged;

        OnCrystalsChanged(PlayerData.Data.ShipData);
        OnShipDataChanged();

        this.isInitialized = true;
        
        yield break;
    }

    protected override void OnShipDataChanged()
    {
        base.OnShipDataChanged();

        var data = PlayerData.Data.ShipData;

        this.batteryNameText.text = data.BatteryData.SubTitle;

        var sb = new StringBuilder();
        sb.AppendLine($"Max Charge: {data.BatteryData.MaxEnergyCharge}");
        sb.AppendLine($"Tiles: {data.BatteryData.NumTilesRequired}");
        this.statsText.text = sb.ToString();

        this.batteryIconImage.sprite = GlobalData.GetBatteryIconSprite(data.BatteryData.SpriteIndex).MonotoneSprite;
    }
}
