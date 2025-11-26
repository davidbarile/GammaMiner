using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class PlayerData
{
    public static PlayerData Data;

    public static Action<int> OnCreditsChanged;
    public static Action OnShipDataChanged;

    public int RandomSeed = -1;
    public string CaptainName;
    public int Credits;
    public int CurrentLevelIndex;

    public float MusicVolume = 0.5f;
    public float EffectsVolume = 0.7f;

    [ReadOnly] public ShipData ShipData;

    public List<string> FavoritePlanetSeeds = new();

    public Dictionary<string, int> ActiveItems = new();
    public Dictionary<string, int> OwnedItems = new();

    [HideInInspector] public int StartDay;
    [HideInInspector] public int StartMonth;
    [HideInInspector] public int StartYear;

    [HideInInspector] public int PriceDay;
    [HideInInspector] public int PriceDayOfWeek;
    [HideInInspector] public int PriceRandomSeed;

    public void AddCredits(int inAmount)
    {
        this.Credits += inAmount;

        if (this.Credits < 0)
            this.Credits = 0;

        OnCreditsChanged?.Invoke(this.Credits);
    }

    public void SetCredits(int inAmount)
    {
        this.Credits = inAmount;

        OnCreditsChanged?.Invoke(this.Credits);
    }

    public void AddFavoritePlanetSeed(string inSeed)
    {
        if (!this.FavoritePlanetSeeds.Contains(inSeed))
            this.FavoritePlanetSeeds.Add(inSeed);
    }

    public void RemoveFavoritePlanetSeed(string inSeed)
    {
        if (this.FavoritePlanetSeeds.Contains(inSeed))
            this.FavoritePlanetSeeds.Remove(inSeed);
    }

    public void AddOwnedItem(string inItemId, int inQuantity = 1, bool inShouldActivate = false, bool inShouldFireEvent = true)
    {
        if (string.IsNullOrEmpty(inItemId))
            return;
        if (this.OwnedItems.ContainsKey(inItemId))
            this.OwnedItems[inItemId] += inQuantity;
        else
            this.OwnedItems[inItemId] = inQuantity;

        if (inShouldActivate)
            this.AddActiveItem(inItemId, inQuantity, false, inShouldFireEvent);
        else if(inShouldFireEvent)
            OnShipDataChanged?.Invoke();
    }

    public void RemoveOwnedItem(string inItemId, int inQuantity = 1, bool inShouldFireEvent = true)
    {
        if (string.IsNullOrEmpty(inItemId))
            return;

        if (this.OwnedItems.ContainsKey(inItemId))
        {
            this.OwnedItems[inItemId] -= inQuantity;

            if (this.OwnedItems[inItemId] <= 0)
                this.OwnedItems.Remove(inItemId);

            if(inShouldFireEvent)
                OnShipDataChanged?.Invoke();
        }
    }

    public int NumItemsOwned(string inItemId)
    {
        if (string.IsNullOrEmpty(inItemId))
            return 0;

        if (this.OwnedItems.ContainsKey(inItemId))
            return this.OwnedItems[inItemId];

        return 0;
    }

    public void AddActiveItem(string inItemId, int inQuantity = 1, bool inShouldBuy = false, bool inShouldFireEvent = true)
    {
        if (string.IsNullOrEmpty(inItemId))
            return;

        if (this.ActiveItems.ContainsKey(inItemId))
            this.ActiveItems[inItemId] += inQuantity;
        else
            this.ActiveItems[inItemId] = inQuantity;

        if (inShouldBuy)
            AddOwnedItem(inItemId, inQuantity, false, inShouldFireEvent);
        else if(inShouldFireEvent)
            OnShipDataChanged?.Invoke();
    }

    public void RemoveActiveItem(string inItemId, int inQuantity = 1, bool inShouldFireEvent = true)
    {
        if (string.IsNullOrEmpty(inItemId))
            return;
            
        if (this.ActiveItems.ContainsKey(inItemId))
        {
            this.ActiveItems[inItemId] -= inQuantity;

            if (this.ActiveItems[inItemId] <= 0)
                this.ActiveItems.Remove(inItemId);

            if(inShouldFireEvent)
                OnShipDataChanged?.Invoke();
        }
    }

    public void ReplaceActiveItem(string inOldItemId, string inNewItemId, int inQuantity = 1)
    {
        if (!string.IsNullOrEmpty(inOldItemId) && this.ActiveItems.ContainsKey(inOldItemId))
        {
            this.ActiveItems[inOldItemId] -= inQuantity;

            if (this.ActiveItems[inOldItemId] <= 0)
                this.ActiveItems.Remove(inOldItemId);
        }

        AddActiveItem(inNewItemId, inQuantity);
    }

    public int NumActiveItems(string inItemId)
    {
        if (this.ActiveItems.ContainsKey(inItemId))
            return this.ActiveItems[inItemId];

        return 0;
    }

    public void SaveSettings()
    {
        //this.ShouldShowDebugOutput = DebugPanel.IN.ShouldShowDebugOutput;
    }

    public void SetSettings()
    {
        //DebugPanel.IN.ShouldShowDebugOutput = this.ShouldShowDebugOutput;
    }

    public void SetStartDate()
    {
        DateTime startDate = DateTime.UtcNow;

        PlayerData.Data.StartYear = startDate.Year;
        PlayerData.Data.StartMonth = startDate.Month;
        PlayerData.Data.StartDay = startDate.Day;

        Debug.Log("SetStartDate()");
    }

    public void InitOwnedItems(ShipConfig config)
    {
        PlayerData.Data.AddOwnedItem(config.Id, config.Quantity, false, false);

        if (config.ReactorData != null)
            PlayerData.Data.AddOwnedItem(config.ReactorData.Id, config.ReactorData.Quantity, false, false);
            
        if (config.BatteryData != null)
            PlayerData.Data.AddOwnedItem(config.BatteryData.Id, config.BatteryData.Quantity, false, false);
            
        if (config.ShieldGeneratorData != null)
            PlayerData.Data.AddOwnedItem(config.ShieldGeneratorData.Id, config.ShieldGeneratorData.Quantity, false, false);

        foreach (var vaultData in config.VaultDatas)
        {
            PlayerData.Data.AddOwnedItem(vaultData.Id, vaultData.Quantity, true, false);
        }

        foreach (var thrusterData in config.ThrusterDatas)
        {
            PlayerData.Data.AddOwnedItem(thrusterData.Id, thrusterData.Quantity, true, false);
        }

        foreach (var railGunData in config.RailGunDatas)
        {
            PlayerData.Data.AddOwnedItem(railGunData.Id, railGunData.Quantity, true, false);
        }

        foreach (var missileLauncherData in config.MissileLauncherDatas)
        {
            PlayerData.Data.AddOwnedItem(missileLauncherData.Id, missileLauncherData.Quantity, true, false);
        }

        foreach (var laserCannonData in config.LaserCannonDatas)
        {
            PlayerData.Data.AddOwnedItem(laserCannonData.Id, laserCannonData.Quantity, true, false);
        }

        foreach (var turretData in config.TurretDatas)
        {
            PlayerData.Data.AddOwnedItem(turretData.Id, turretData.Quantity, true, false);
        }

        //fire event once after all items have been added
        OnShipDataChanged?.Invoke();
    }
}