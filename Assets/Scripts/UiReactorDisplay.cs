using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiReactorDisplay : ShipComponentDisplayBase
{
    [SerializeField] private TextMeshProUGUI reactorNameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI percentText;

    [Space, SerializeField] private Image reactorIconImage;

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

        this.reactorNameText.text = data.ReactorData.SubTitle;

        var sb = new StringBuilder();
        sb.AppendLine($"Recharge Rate: {data.ReactorData.EnergyRechargeRate}/sec");
        sb.AppendLine($"Tiles: {data.ReactorData.NumTilesRequired}");
        this.statsText.text = sb.ToString();

        this.reactorIconImage.sprite = GlobalData.GetReactorIconSprite(data.ReactorData.SpriteIndex).MonotoneSprite;
    }
}