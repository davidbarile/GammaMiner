using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static CrystalData;

public class UiCrystalTradeItem : MonoBehaviour
{
    [SerializeField] private Image crystalIcon;
    [SerializeField] private TMP_Text crystalNameText;
    [SerializeField] private TMP_Text crystalAmountText;

    [SerializeField] private Button tradeTenButton;
    [SerializeField] private Button tradeAllButton;

    [SerializeField] private Button dumpTenButton;
    [SerializeField] private Button dumpAllButton;

    [SerializeField] private GameObject tradeButtonsContainer;
    [SerializeField] private GameObject dumpButtonsContainer;

    private ECrystalType crystalType;
    private Action<ECrystalType, int, bool> onSellOrDumpCrystals;
    private int crystalAmount;

    public void Configure(ECrystalType inCrystalType, int inAmount, Action<ECrystalType, int, bool> inOnSellOrDumpCrystals, bool isOfferedToday)
    {
        this.crystalType = inCrystalType;
        this.onSellOrDumpCrystals = inOnSellOrDumpCrystals;
        this.crystalAmount = inAmount;
        this.crystalIcon.color = GlobalData.GetCrystalColor(inCrystalType);
        this.crystalNameText.text = GlobalData.GetCrystalName(inCrystalType);
        this.crystalAmountText.text = inAmount.ToString();

        this.tradeTenButton.interactable = this.crystalAmount >= 10 && isOfferedToday;
        this.tradeAllButton.interactable = this.crystalAmount > 0 && isOfferedToday;

        this.dumpTenButton.interactable = this.crystalAmount >= 10;
        this.dumpAllButton.interactable = this.crystalAmount > 0;

        this.tradeButtonsContainer.SetActive(isOfferedToday);
        this.dumpButtonsContainer.SetActive(!isOfferedToday);
    }

    public void HandleTradeTenButtonClicked()
    {
        var amountToSell = Math.Min(10, this.crystalAmount);
        this.onSellOrDumpCrystals?.Invoke(this.crystalType, amountToSell, true);
    }

    public void HandleTradeAllButtonClicked()
    {
        this.onSellOrDumpCrystals?.Invoke(this.crystalType, this.crystalAmount, true);
    }

    public void HandleDumpTenButtonClicked()
    {
        var amountToSell = Math.Min(10, this.crystalAmount);
        this.onSellOrDumpCrystals?.Invoke(this.crystalType, amountToSell, false);
    }

    public void HandleDumpAllButtonClicked()
    {
        this.onSellOrDumpCrystals?.Invoke(this.crystalType, this.crystalAmount, false);
    }
}