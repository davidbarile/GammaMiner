using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static CrystalData;

public class UiTradePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text dayOfWeekText;
    [SerializeField] private UiVaultDisplay vaultDisplay;
    [SerializeField] private Button tradeAllInVaultButton;
    [SerializeField] private GameObject noCrystalsMessage;
    [SerializeField] private UiCrystalTradeItem[] crystalTradeItems;
    [SerializeField] private UiCrystalPriceItem[] crystalPriceItems;

    private CrystalTradePricesConfig crystalTradePricesConfig;
    private Dictionary<ECrystalType, float> crystalSellPricesDict = new();

    private List<CrystalTradePriceData> todaysPrices = new();

    private string[] dayNames = new string[7] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

    private void OnEnable()
    {
        if (PlayerData.Data == null || PlayerData.Data.ShipData == null)
            return;
            
        HUD.OnCrystalsChanged += RefreshCrystals;
        RefreshCrystals(PlayerData.Data.ShipData);
        this.vaultDisplay.OnVaultIndexChanged += SetTradeAllInVaultButtonInteractability;
    }

    private void OnDisable()
    {
        HUD.OnCrystalsChanged -= RefreshCrystals;
        this.vaultDisplay.OnVaultIndexChanged -= SetTradeAllInVaultButtonInteractability;
    }

    public void Init(CrystalTradePricesConfig inCrystalTradePricesConfig)
    {
        this.crystalTradePricesConfig = inCrystalTradePricesConfig;

        var today = DateTime.Now;
        if (PlayerData.Data.PriceDay != today.Day && PlayerData.Data.PriceDayOfWeek != (int)today.DayOfWeek)
        {
            //generate new prices for new day
            PlayerData.Data.PriceDay = today.Day;
            PlayerData.Data.PriceDayOfWeek = (int)today.DayOfWeek;
            PlayerData.Data.PriceRandomSeed = UnityEngine.Random.Range(0, 99999);
        }

        this.dayOfWeekText.text = $"{dayNames[PlayerData.Data.PriceDayOfWeek]}'s Offers";
        
        if(this.todaysPrices.Count == 0)
            this.todaysPrices = this.crystalTradePricesConfig.GenerateNewPrices();

        RefreshCrystalPrices();
            
        HUD.OnCrystalsChanged += RefreshCrystals;
        RefreshCrystals(PlayerData.Data.ShipData);
        this.vaultDisplay.OnVaultIndexChanged += SetTradeAllInVaultButtonInteractability;
    }

    // Setup crystal trade items
    public void RefreshCrystals(ShipData inShipData)
    {
        this.vaultDisplay.Initialize();
        SetTradeAllInVaultButtonInteractability();

        foreach (var display in this.crystalTradeItems)
        {
            display.gameObject.SetActive(false);
        }

        var totalSpace = 0;
        var totalCrystals = 0;

        // Dictionary to hold the quantity of each crystal type
        var crystalCountsDict = new Dictionary<ECrystalType, int>();

        for (var i = 0; i < inShipData.VaultDatas.Count; i++)
        {
            var vault = inShipData.VaultDatas[i];
            var storedCrystalsDict = vault.StoredCrystalsDict;

            if (storedCrystalsDict == null || storedCrystalsDict.Count == 0) continue;

            totalCrystals += vault.GetUsedStorage();
            totalSpace += vault.Capacity;

            foreach (var kvp in storedCrystalsDict)
            {
                var crystalTypeString = kvp.Key;
                var crystalType = (ECrystalType)Enum.Parse(typeof(ECrystalType), crystalTypeString);
                var crystalCount = kvp.Value;

                if (crystalCountsDict.ContainsKey(crystalType))
                {
                    crystalCountsDict[crystalType] += crystalCount;
                }
                else
                {
                    crystalCountsDict.Add(crystalType, crystalCount);
                }
            }
        }

        this.noCrystalsMessage.SetActive(true);

        var index = 0;
        foreach (var kvp in crystalCountsDict)
        {
            var crystalsDisplay = this.crystalTradeItems[index];
            var shouldShow = kvp.Value > 0;
            crystalsDisplay.gameObject.SetActive(shouldShow);
            if (shouldShow)
            {
                var isCrystalOfferedToday = DoesTodaysPricesContainCrystal(kvp.Key);
                crystalsDisplay.Configure(kvp.Key, kvp.Value, SellOrDumpCrystals, isCrystalOfferedToday);
                this.noCrystalsMessage.SetActive(false);
            }
            ++index;
        }
    }

    private void SetTradeAllInVaultButtonInteractability()
    {
        if (PlayerData.Data == null || PlayerData.Data.ShipData == null || PlayerData.Data.ShipData.VaultDatas.Count == 0)
        {
            this.tradeAllInVaultButton.interactable = false;
            return;
        }

        var vault = PlayerData.Data.ShipData.VaultDatas[this.vaultDisplay.CurrentIndex];
        var storedCrystalsDict = vault.StoredCrystalsDict;

        if (storedCrystalsDict == null || storedCrystalsDict.Count == 0)
            this.tradeAllInVaultButton.interactable = false;

        foreach (var kvp in storedCrystalsDict)
        {
            var crystalCount = kvp.Value;

            if (crystalCount > 0)
            {
                var isCrystalOfferedToday = DoesTodaysPricesContainCrystal((ECrystalType)Enum.Parse(typeof(ECrystalType), kvp.Key));

                if(isCrystalOfferedToday)
                {
                    this.tradeAllInVaultButton.interactable = true;
                    return;
                }
            }
        }

        this.tradeAllInVaultButton.interactable = false;
    }
    
    private bool DoesTodaysPricesContainCrystal(ECrystalType inCrystalType)
    {
        foreach (var offer in this.todaysPrices)
        {
            if (offer.CrystalType == inCrystalType)
            {
                return true;
            }
        }
        return false;
    }

    public void SellOrDumpCrystals(ECrystalType inCrystalType, int inAmount, bool isSelling)
    {
        //loop backwards to prioritize removing crystals from the last vault first
        for (var i = PlayerData.Data.ShipData.VaultDatas.Count - 1; i >= 0; i--)
        {
            var vault = PlayerData.Data.ShipData.VaultDatas[i];
            var storedCrystalsDict = vault.StoredCrystalsDict;

            if (storedCrystalsDict == null || storedCrystalsDict.Count == 0) continue;

            foreach (var crystalTypeString in new List<string>(storedCrystalsDict.Keys))
            {
                var crystalType = (ECrystalType)Enum.Parse(typeof(ECrystalType), crystalTypeString);
                var crystalCount = storedCrystalsDict[crystalTypeString];

                if (crystalType != inCrystalType || crystalCount <= 0 || (!this.crystalSellPricesDict.ContainsKey(crystalType) && isSelling))
                    continue;

                var crystalsToRemove = Math.Min(inAmount, crystalCount);
                storedCrystalsDict[crystalTypeString] -= crystalsToRemove;
                inAmount -= crystalsToRemove;

                if(isSelling)
                {
                    var creditsGained = Mathf.RoundToInt(crystalsToRemove * GetCrystalSellPrice(crystalType));
                    PlayerData.Data.AddCredits(creditsGained);
                }
                
                if (inAmount <= 0) break;
            }
            if (inAmount <= 0) break;
        }

        HUD.OnCrystalsChanged?.Invoke(PlayerData.Data.ShipData);
    }

    private void RefreshCrystalPrices()
    {
        this.crystalSellPricesDict.Clear();

        for (var i = 0; i < this.crystalPriceItems.Length; i++)
        {
            var priceItem = this.crystalPriceItems[i];
            var shouldShow = i < this.todaysPrices.Count;

            priceItem.gameObject.SetActive(shouldShow);            

            if (shouldShow)
            {
                var priceData = this.todaysPrices[i];
                var crystalData = GlobalData.GetCrystalData(priceData.CrystalType);
            
                priceItem.Configure(crystalData, priceData.BasePrice, priceData.PercentModifier);

                var percentSellPrice = priceData.BasePrice + priceData.BasePrice * (priceData.PercentModifier / 100f);

                if (!this.crystalSellPricesDict.ContainsKey(priceData.CrystalType))
                {
                    this.crystalSellPricesDict.Add(priceData.CrystalType, percentSellPrice);
                }
            }
        }
    }

    private float GetCrystalSellPrice(ECrystalType inCrystalType)
    {
        if (this.crystalSellPricesDict.TryGetValue(inCrystalType, out var sellPrice))
        {
            return sellPrice;
        }
        //Debug.LogError($"<color=red>No sell price found for CrystalType {inCrystalType}</color>");
        return 0;
    }

    public void HandleTradeAllInVaultButtonPress()
    {
        var crystalCountsDict = new Dictionary<ECrystalType, int>();

        // First, gather all crystals in the vault
        var vault = PlayerData.Data.ShipData.VaultDatas[this.vaultDisplay.CurrentIndex];
        var storedCrystalsDict = vault.StoredCrystalsDict;

        if (storedCrystalsDict == null || storedCrystalsDict.Count == 0)
            return;

        foreach (var kvp in storedCrystalsDict)
        {
            var crystalTypeString = kvp.Key;
            var crystalType = (ECrystalType)Enum.Parse(typeof(ECrystalType), crystalTypeString);
            var crystalCount = kvp.Value;

            if(!this.crystalSellPricesDict.ContainsKey(crystalType))
                continue;

            if (crystalCountsDict.ContainsKey(crystalType))
            {
                crystalCountsDict[crystalType] += crystalCount;
            }
            else
            {
                crystalCountsDict.Add(crystalType, crystalCount);
            }
        }

        // Now, sell all gathered crystals
        foreach (var kvp in crystalCountsDict)
        {
            SellOrDumpCrystals(kvp.Key, kvp.Value, true);
        }
    }
}