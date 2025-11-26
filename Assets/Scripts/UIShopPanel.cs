using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CrystalData;

public class UIShopPanel : UIPanelBase
{
    public static UIShopPanel IN;
    public static Action<ShopItemConfig> OnShopItemSelected;

    [Space, SerializeField] private  ShopConfig shopConfig;
    [SerializeField] private CrystalTradePricesConfig crystalTradePricesConfig;

    [Space, SerializeField] private UiTradePanel tradePanel;

    [Space, SerializeField] private ResetScrollRectOnEnable storeItemsScrollRectResetter;

    [Space, SerializeField] private RectTransform itemsScrollerRectTrans;
    [SerializeField] private Transform itemsParent;
    [SerializeField] private Transform itemsSpacer;
    [SerializeField] private float spacerOffset = 100f;
    [SerializeField] private LayoutElement[] spacerLayoutElements;

    [Header("Buy / Sell Mode")]
    [SerializeField] private GameObject[] sellModeItemsToShow;
    [SerializeField] private GameObject[] buyModeItemsToShow;

    [Space, SerializeField] private UIShopItemButton selectedShopItemButton;

    [Space, SerializeField] private UIShopCategoryButton[] categoryButtons;

    public RectTransform SelectionAreaRect => this.selectionAreaRect;
    [Space, SerializeField] private RectTransform selectionAreaRect;
    private List<UIShopItemButton> shopItemButtons = new();
    private List<LayoutElement> shopItemDividers = new();

    public Color[] CategoryColors => this.categoryColors;
    [Space, SerializeField] private Color[] categoryColors;

    public UIShopCategoryButton SelectedCategoryButton { get; private set; }

    private ShopItemConfig selectedShopItemConfig;

    private void Start()
    {
        if(this.itemsScrollerRectTrans == null)
            return;

        foreach (var spacer in this.spacerLayoutElements)
        {
            spacer.minWidth = (this.itemsScrollerRectTrans.rect.width * 0.5f) - this.spacerOffset;
        }

        SetBuyMode(true);
    }

    public void Show(ShopConfig inShopConfig)
    {
        GameManager.PauseGame(true);

        this.shopConfig = inShopConfig;

        for (int i = 0; i < this.categoryButtons.Length; i++)
        {
            var shouldShow = i < inShopConfig.categoryDatas.Length;

            var catButton = this.categoryButtons[i];
            catButton.gameObject.SetActive(shouldShow);

            if (shouldShow)
            {
                var catData = inShopConfig.categoryDatas[i];
                catButton.Configure(catData);
            }
        }

        base.Show();

        if (this.SelectedCategoryButton == null)
            SelectCategory(this.categoryButtons[0]);

        HUD.OnCrystalsChanged?.Invoke(PlayerData.Data.ShipData);

        PlayerData.OnShipDataChanged += RefreshCurrentCategory;

        OnShopItemSelected += RefreshSelectedShopItem;

        PlayerData.OnShipDataChanged?.Invoke();
        PlayerData.OnCreditsChanged?.Invoke(PlayerData.Data.Credits);
    }

    public override void Hide()
    {
        GameManager.PauseGame(false);

        PlayerData.OnShipDataChanged -= RefreshCurrentCategory;
        OnShopItemSelected -= RefreshSelectedShopItem;

        base.Hide();
    }

    private void RefreshSelectedShopItem(ShopItemConfig inConfig)
    {
        if (inConfig == this.selectedShopItemConfig)
            return;

        this.selectedShopItemConfig = inConfig;
        this.selectedShopItemButton.Configure(inConfig);
    }

    public Color GetShopCategoryColor(EShopItemType inType)
    {
        switch (inType)
        {
            case EShopItemType.Reactor:
                return this.CategoryColors[1];
            case EShopItemType.Battery:
                return this.CategoryColors[2];
            case EShopItemType.ShieldGenerators:
                return this.CategoryColors[3];
            case EShopItemType.Vault:
                return this.CategoryColors[4];
            case EShopItemType.Thrusters:
                return this.CategoryColors[5];
            case EShopItemType.Railguns:
                return this.CategoryColors[6];
            case EShopItemType.LaserCannons:
                return this.CategoryColors[7];
            case EShopItemType.MissileLaunchers:
                return this.CategoryColors[8];
            case EShopItemType.Turrets:
                return this.CategoryColors[9];
            case EShopItemType.Armor:
                return this.CategoryColors[10];
            default:
                return Color.white;
        }
    }

    private void RefreshCurrentCategory()
    {
        if (this.SelectedCategoryButton == null)
            return;

        RefreshItemsScroller(this.SelectedCategoryButton.CategoryData);
    }

    private IEnumerator ResetScrollerPositionCo()
    {
        yield return null;
        this.storeItemsScrollRectResetter.ScrollToPosition(0f);
    }

    public void SelectCategory(UIShopCategoryButton inCategoryButton)
    {
        if (inCategoryButton == this.SelectedCategoryButton) return;

        if (this.SelectedCategoryButton != null)
            this.SelectedCategoryButton.SetSelected(false);

        this.SelectedCategoryButton = inCategoryButton;

        this.SelectedCategoryButton.SetSelected(true);

        RefreshItemsScroller(inCategoryButton.CategoryData);

        StartCoroutine(ResetScrollerPositionCo());
    }

    private void RefreshItemsScroller(ShopCategoryData inData)
    {
        foreach (var itemButton in this.shopItemButtons)
        {
            Pool.Despawn(itemButton.gameObject);
        }
        this.shopItemButtons.Clear();

        foreach (var divider in this.shopItemDividers)
        {
            Pool.Despawn(divider.gameObject);
        }
        this.shopItemDividers.Clear();
        
        for (int i = 0; i < inData.ItemDatas.Length; i++)
        {
            var itemData = inData.ItemDatas[i];

            if (itemData.ShopItemType == EShopItemType.None)
            {
                var divider = Pool.Spawn<LayoutElement>("Divider", this.itemsParent, Vector3.zero, Quaternion.identity);
                this.shopItemDividers.Add(divider);
            }
            else
            {
                var item = Pool.Spawn<UIShopItemButton>("Shop Item Button", this.itemsParent, Vector3.zero, Quaternion.identity);

                item.Configure(itemData);
                var itemName = string.IsNullOrWhiteSpace(itemData.SubTitle) ? itemData.Name : $"{itemData.Name} - {itemData.SubTitle}";
                item.name = $"Item_{i} ({itemName})";
                this.shopItemButtons.Add(item);
            }
        }

        this.itemsSpacer.SetAsLastSibling();
    }

    public void TryBuyItem(ShopItemConfig inConfig, bool inIsPurchase = true)
    {
        if (inConfig.ShopItemType == EShopItemType.Ship)
        {
            var shipConfig = inConfig as ShipConfig;
            var isNewShipSmaller = shipConfig.NumTiles < PlayerData.Data.ShipData.NumUsedTiles;

            if (isNewShipSmaller)
            {
                UIConfirmPanel.IN.Show("Downgrade Ship?", $"The new ship is smaller and will downgrade components.\n\nUsed Space: {PlayerData.Data.ShipData.NumUsedTiles}\nNew Ship Space: {shipConfig.NumTiles}\n\nDo you want to proceed?", () =>
                {
                    // User confirmed, proceed with the downgrade
                    BuyItem(inConfig, inIsPurchase);
                }, () =>
                {
                    // User cancelled, revert changes
                    return;
                });
            }
            else
                BuyItem(inConfig, inIsPurchase);
        }
        else
            BuyItem(inConfig, inIsPurchase);
    }

    private void BuyItem(ShopItemConfig inConfig, bool inIsPurchase = true)
    {
        Debug.Log($"BuyItem({inConfig.Name} {inConfig.Quantity} for {inConfig.Price})");

        switch (inConfig.ShopItemType)
        {
            case EShopItemType.Ship:
                var shipConfig = inConfig as ShipConfig;

                if (SpaceShip.PlayerShip != null && SpaceShip.PlayerShip.ShipData != null &&
                    !SpaceShip.PlayerShip.ShipData.PrefabName.Equals(shipConfig.PrefabName))
                {
                    GameManager.ReplacePlayerShipPrefab(shipConfig.PrefabName);
                }

                var newShipData = ShipData.GetDataFromConfig(shipConfig);//this is the new one.
                //If we want to keep old components, we replace them with the pre-existing ones in PlayerData.Data.ShipData

                var isNewShipBigger = newShipData.NumTotalTiles > PlayerData.Data.ShipData.NumTotalTiles;
                var hasSpaceForComponent = true;

                if (inIsPurchase)
                {
                    //go thru all ship components and if they don't exist, add new ones
                    //also replace component if the new component is better than the current one

                    //REACTOR
                    if (!PlayerData.Data.OwnedItems.ContainsKey(newShipData.ReactorData.Id))
                    {
                        PlayerData.Data.AddOwnedItem(newShipData.ReactorData.Id, 1, false, false);
                    }

                    hasSpaceForComponent = isNewShipBigger ||
                        PlayerData.Data.ShipData.ReactorData.NumTilesRequired - newShipData.ReactorData.NumTilesRequired <= newShipData.GetNumAvailableTiles();

                    if (hasSpaceForComponent && PlayerData.Data.ShipData.ReactorData.Price > newShipData.ReactorData.Price)
                    {
                        //only replace reactor if the new one is better
                        newShipData.ReactorData = PlayerData.Data.ShipData.ReactorData;//keep old
                    }

                    //BATTERY
                    if (!PlayerData.Data.OwnedItems.ContainsKey(newShipData.BatteryData.Id))
                    {
                        PlayerData.Data.AddOwnedItem(newShipData.BatteryData.Id, 1, false, false);
                    }

                    hasSpaceForComponent = isNewShipBigger ||
                        PlayerData.Data.ShipData.BatteryData.NumTilesRequired - newShipData.BatteryData.NumTilesRequired <= newShipData.GetNumAvailableTiles();

                    if (hasSpaceForComponent && PlayerData.Data.ShipData.BatteryData.Price > newShipData.BatteryData.Price)
                    {
                        newShipData.BatteryData = PlayerData.Data.ShipData.BatteryData;//keep old
                    }

                    var hasShieldGenerator = newShipData.ShieldGeneratorData != null && !string.IsNullOrEmpty(newShipData.ShieldGeneratorData.Id);

                    //SHIELD GENERATOR
                    if (hasShieldGenerator && !PlayerData.Data.OwnedItems.ContainsKey(newShipData.ShieldGeneratorData.Id))
                    {
                        PlayerData.Data.AddOwnedItem(newShipData.ShieldGeneratorData.Id, 1, false, false);
                    }

                    if (PlayerData.Data.ShipData.ShieldGeneratorData != null)
                    {
                        var newShipShieldTiles = 0;

                        if (newShipData.ShieldGeneratorData != null)
                            newShipShieldTiles = newShipData.ShieldGeneratorData.NumTilesRequired;

                        hasSpaceForComponent = isNewShipBigger ||
                            PlayerData.Data.ShipData.ShieldGeneratorData.NumTilesRequired - newShipShieldTiles <= newShipData.GetNumAvailableTiles();

                        if (hasSpaceForComponent && PlayerData.Data.ShipData.ShieldGeneratorData.Price > newShipData.ShieldGeneratorData.Price)
                        {
                            newShipData.ShieldGeneratorData = PlayerData.Data.ShipData.ShieldGeneratorData;//keep old
                        }
                    }

                    //TODO: add hasSpaceForComponent logic to the rest of the components

                    //VAULTS
                    var existingVaults = new List<VaultData>(PlayerData.Data.ShipData.VaultDatas);
                    var vaultsToAddOrReplace = new List<VaultData>();

                    foreach (var newVault in newShipData.VaultDatas)
                    {
                        int replaceIndex = -1;
                        for (int i = 0; i < existingVaults.Count; i++)
                        {
                            var existingVault = existingVaults[i];
                            // If the new vault is higher priced, mark for replacement
                            if (existingVault.Price > newVault.Price)
                            {
                                replaceIndex = i;
                                break;
                            }
                        }

                        if (replaceIndex != -1)
                        {
                            var betterExistingVault = existingVaults[replaceIndex];
                            existingVaults.RemoveAt(replaceIndex);
                            vaultsToAddOrReplace.Add(betterExistingVault);
                        }
                        else
                        {
                            vaultsToAddOrReplace.Add(newVault);
                        }
                    }

                    vaultsToAddOrReplace.Sort((a, b) => a.Price.CompareTo(b.Price));

                    var activeVaultIds = new List<string>();

                    //deactivate all active vaults first
                    for (int i = 0; i < PlayerData.Data.ShipData.VaultDatas.Count; i++)
                    {
                        var oldVaultData = PlayerData.Data.ShipData.VaultDatas[i];
                        activeVaultIds.Add(oldVaultData.Id);
                        PlayerData.Data.RemoveActiveItem(oldVaultData.Id, 1, false);
                    }

                    var vaultsCount = Mathf.Max(newShipData.VaultDatas.Count, PlayerData.Data.ShipData.VaultDatas.Count);

                    for (int i = 0; i < vaultsCount; i++)
                    {
                        VaultData vaultData = null;

                        if (i < vaultsToAddOrReplace.Count)
                            vaultData = vaultsToAddOrReplace[i];
                        else if (i < PlayerData.Data.ShipData.VaultDatas.Count)
                            vaultData = PlayerData.Data.ShipData.VaultDatas[i];

                        if (activeVaultIds.Contains(vaultData.Id))
                        {
                            PlayerData.Data.AddActiveItem(vaultData.Id, 1, false, false);
                            activeVaultIds.Remove(vaultData.Id);
                        }
                        else
                            PlayerData.Data.AddOwnedItem(vaultData.Id, 1, true, false);
                        if (i < newShipData.VaultDatas.Count)
                            newShipData.VaultDatas[i] = vaultData;
                        else
                            newShipData.VaultDatas.Add(vaultData);
                    }

                    //THRUSTERS
                    var existingThrusters = new List<ThrusterData>(PlayerData.Data.ShipData.ThrusterDatas);
                    var thrustersToAddOrReplace = new List<ThrusterData>();

                    foreach (var newThruster in newShipData.ThrusterDatas)
                    {
                        int replaceIndex = -1;
                        for (int i = 0; i < existingThrusters.Count; i++)
                        {
                            var existingThruster = existingThrusters[i];
                            // If the new thruster is higher priced, mark for replacement
                            if (existingThruster.Price > newThruster.Price)
                            {
                                replaceIndex = i;
                                break;
                            }
                        }

                        if (replaceIndex != -1)
                        {
                            var betterExistingThruster = existingThrusters[replaceIndex];
                            existingThrusters.RemoveAt(replaceIndex);
                            thrustersToAddOrReplace.Add(betterExistingThruster);
                        }
                        else
                        {
                            thrustersToAddOrReplace.Add(newThruster);
                        }
                    }

                    thrustersToAddOrReplace.Sort((a, b) => a.Price.CompareTo(b.Price));

                    var activeThrusterIds = new List<string>();

                    //deactivate all active thrusters first
                    for (int i = 0; i < PlayerData.Data.ShipData.ThrusterDatas.Count; i++)
                    {
                        var oldThrusterData = PlayerData.Data.ShipData.ThrusterDatas[i];
                        activeThrusterIds.Add(oldThrusterData.Id);
                        PlayerData.Data.RemoveActiveItem(oldThrusterData.Id, 1, false);
                    }

                    var thrustersCount = Mathf.Max(newShipData.ThrusterDatas.Count, PlayerData.Data.ShipData.ThrusterDatas.Count);

                    for (int i = 0; i < thrustersCount; i++)
                    {
                        ThrusterData thrusterData = null;

                        if (i < thrustersToAddOrReplace.Count)
                            thrusterData = thrustersToAddOrReplace[i];
                        else if (i < PlayerData.Data.ShipData.ThrusterDatas.Count)
                            thrusterData = PlayerData.Data.ShipData.ThrusterDatas[i];

                        if (activeThrusterIds.Contains(thrusterData.Id))
                        {
                            PlayerData.Data.AddActiveItem(thrusterData.Id, 1, false, false);
                            activeThrusterIds.Remove(thrusterData.Id);
                        }
                        else
                            PlayerData.Data.AddOwnedItem(thrusterData.Id, 1, true, false);
                        if (i < newShipData.ThrusterDatas.Count)
                            newShipData.ThrusterDatas[i] = thrusterData;
                        else
                            newShipData.ThrusterDatas.Add(thrusterData);
                    }

                    //RAILGUNS
                    var existingRailguns = new List<RailGunData>(PlayerData.Data.ShipData.RailgunDatas);
                    var railgunsToAddOrReplace = new List<RailGunData>();

                    foreach (var newRailgun in newShipData.RailgunDatas)
                    {
                        int replaceIndex = -1;
                        for (int i = 0; i < existingRailguns.Count; i++)
                        {
                            var existingRailgun = existingRailguns[i];
                            // If the new railgun is higher priced, mark for replacement
                            if (existingRailgun.Price > newRailgun.Price)
                            {
                                replaceIndex = i;
                                break;
                            }
                        }

                        if (replaceIndex != -1)
                        {
                            var betterExistingRailgun = existingRailguns[replaceIndex];
                            existingRailguns.RemoveAt(replaceIndex);
                            railgunsToAddOrReplace.Add(betterExistingRailgun);
                        }
                        else
                        {
                            railgunsToAddOrReplace.Add(newRailgun);
                        }
                    }

                    railgunsToAddOrReplace.Sort((a, b) => a.Price.CompareTo(b.Price));

                    var activeRailgunIds = new List<string>();

                    //deactivate all active railguns first
                    for (int i = 0; i < PlayerData.Data.ShipData.RailgunDatas.Count; i++)
                    {
                        var oldRailgunData = PlayerData.Data.ShipData.RailgunDatas[i];
                        activeRailgunIds.Add(oldRailgunData.Id);
                        PlayerData.Data.RemoveActiveItem(oldRailgunData.Id, 1, false);
                    }

                    var railgunsCount = Mathf.Max(newShipData.RailgunDatas.Count, PlayerData.Data.ShipData.RailgunDatas.Count);

                    for (int i = 0; i < railgunsCount; i++)
                    {
                        RailGunData railgunData = null;

                        if (i < railgunsToAddOrReplace.Count)
                            railgunData = railgunsToAddOrReplace[i];
                        else if (i < PlayerData.Data.ShipData.RailgunDatas.Count)
                            railgunData = PlayerData.Data.ShipData.RailgunDatas[i];

                        if (activeRailgunIds.Contains(railgunData.Id))
                        {
                            PlayerData.Data.AddActiveItem(railgunData.Id, 1, false, false);
                            activeRailgunIds.Remove(railgunData.Id);
                        }
                        else
                            PlayerData.Data.AddOwnedItem(railgunData.Id, 1, true, false);
                        if (i < newShipData.RailgunDatas.Count)
                            newShipData.RailgunDatas[i] = railgunData;
                        else
                            newShipData.RailgunDatas.Add(railgunData);
                    }

                    //MISSILE LAUNCHERS
                    var existingMissileLaunchers = new List<MissileLauncherData>(PlayerData.Data.ShipData.MissileLauncherDatas);
                    var missileLaunchersToAddOrReplace = new List<MissileLauncherData>();

                    foreach (var newMissileLauncher in newShipData.MissileLauncherDatas)
                    {
                        int replaceIndex = -1;
                        for (int i = 0; i < existingMissileLaunchers.Count; i++)
                        {
                            var existingMissileLauncher = existingMissileLaunchers[i];
                            // If the new missile launcher is higher priced, mark for replacement
                            if (existingMissileLauncher.Price > newMissileLauncher.Price)
                            {
                                replaceIndex = i;
                                break;
                            }
                        }

                        if (replaceIndex != -1)
                        {
                            var betterExistingMissileLauncher = existingMissileLaunchers[replaceIndex];
                            existingMissileLaunchers.RemoveAt(replaceIndex);
                            missileLaunchersToAddOrReplace.Add(betterExistingMissileLauncher);
                        }
                        else
                        {
                            missileLaunchersToAddOrReplace.Add(newMissileLauncher);
                        }
                    }

                    missileLaunchersToAddOrReplace.Sort((a, b) => a.Price.CompareTo(b.Price));

                    var activeMissileLauncherIds = new List<string>();

                    //deactivate all active missile launchers first
                    foreach (var activeMissileLauncherId in activeMissileLauncherIds)
                    {
                        PlayerData.Data.RemoveActiveItem(activeMissileLauncherId, 1, false);
                    }

                    foreach (var missileLauncherData in missileLaunchersToAddOrReplace)
                    {
                        PlayerData.Data.AddOwnedItem(missileLauncherData.Id, 1, true, false);
                    }

                    var missileLaunchersCount = Mathf.Max(newShipData.MissileLauncherDatas.Count, PlayerData.Data.ShipData.MissileLauncherDatas.Count);

                    for (int i = 0; i < missileLaunchersCount; i++)
                    {
                        MissileLauncherData missileLauncherData = null;

                        if (i < missileLaunchersToAddOrReplace.Count)
                            missileLauncherData = missileLaunchersToAddOrReplace[i];
                        else if (i < PlayerData.Data.ShipData.MissileLauncherDatas.Count)
                            missileLauncherData = PlayerData.Data.ShipData.MissileLauncherDatas[i];

                        if (activeMissileLauncherIds.Contains(missileLauncherData.Id))
                        {
                            PlayerData.Data.AddActiveItem(missileLauncherData.Id, 1, false, false);
                            activeMissileLauncherIds.Remove(missileLauncherData.Id);
                        }
                        else
                            PlayerData.Data.AddOwnedItem(missileLauncherData.Id, 1, true, false);
                        if (i < newShipData.MissileLauncherDatas.Count)
                            newShipData.MissileLauncherDatas[i] = missileLauncherData;
                        else
                            newShipData.MissileLauncherDatas.Add(missileLauncherData);
                    }

                    //LASER CANNONS
                    var existingLaserCannons = new List<LaserCannonData>(PlayerData.Data.ShipData.LaserCannonDatas);
                    var laserCannonsToAddOrReplace = new List<LaserCannonData>();

                    foreach (var newLaserCannon in newShipData.LaserCannonDatas)
                    {
                        int replaceIndex = -1;
                        for (int i = 0; i < existingLaserCannons.Count; i++)
                        {
                            var existingLaserCannon = existingLaserCannons[i];
                            // If the new laser cannon is higher priced, mark for replacement
                            if (existingLaserCannon.Price > newLaserCannon.Price)
                            {
                                replaceIndex = i;
                                break;
                            }
                        }

                        if (replaceIndex != -1)
                        {
                            var betterExistingLaserCannon = existingLaserCannons[replaceIndex];
                            existingLaserCannons.RemoveAt(replaceIndex);
                            laserCannonsToAddOrReplace.Add(betterExistingLaserCannon);
                        }
                        else
                        {
                            laserCannonsToAddOrReplace.Add(newLaserCannon);
                        }
                    }

                    laserCannonsToAddOrReplace.Sort((a, b) => a.Price.CompareTo(b.Price));

                    var activeLaserCannonIds = new List<string>();

                    //deactivate all active laser cannons first
                    for (int i = 0; i < PlayerData.Data.ShipData.LaserCannonDatas.Count; i++)
                    {
                        var oldLaserCannonData = PlayerData.Data.ShipData.LaserCannonDatas[i];
                        activeLaserCannonIds.Add(oldLaserCannonData.Id);
                        PlayerData.Data.RemoveActiveItem(oldLaserCannonData.Id, 1, false);
                    }

                    var laserCannonsCount = Mathf.Max(newShipData.LaserCannonDatas.Count, PlayerData.Data.ShipData.LaserCannonDatas.Count);

                    for (int i = 0; i < laserCannonsCount; i++)
                    {
                        LaserCannonData laserCannonData = null;

                        if (i < laserCannonsToAddOrReplace.Count)
                            laserCannonData = laserCannonsToAddOrReplace[i];
                        else if (i < PlayerData.Data.ShipData.LaserCannonDatas.Count)
                            laserCannonData = PlayerData.Data.ShipData.LaserCannonDatas[i];

                        if (activeLaserCannonIds.Contains(laserCannonData.Id))
                        {
                            PlayerData.Data.AddActiveItem(laserCannonData.Id, 1, false, false);
                            activeLaserCannonIds.Remove(laserCannonData.Id);
                        }
                        else
                            PlayerData.Data.AddOwnedItem(laserCannonData.Id, 1, true, false);
                        if (i < newShipData.LaserCannonDatas.Count)
                            newShipData.LaserCannonDatas[i] = laserCannonData;
                        else
                            newShipData.LaserCannonDatas.Add(laserCannonData);
                    }
                }

                PlayerData.Data.ShipData = newShipData;
                break;

            case EShopItemType.Reactor:
                var reactorConfig = inConfig as ReactorConfig;
                SpaceShip.PlayerShip.ShipData.ReactorData = ReactorData.GetDataFromConfig(reactorConfig);
                break;

            case EShopItemType.Battery:
                var batteryConfig = inConfig as BatteryConfig;
                SpaceShip.PlayerShip.ShipData.BatteryData = BatteryData.GetDataFromConfig(batteryConfig);
                break;

            case EShopItemType.Vault:
                var vaultConfig = inConfig as VaultConfig;
                SpaceShip.PlayerShip.ShipData.VaultDatas.Add(VaultData.GetDataFromConfig(vaultConfig));
                SpaceShip.PlayerShip.ShipData.VaultDatas.Sort((a, b) => a.Price.CompareTo(b.Price));//TODO: add to include shielding
                break;

            case EShopItemType.Thrusters:
                var thrusterConfig = inConfig as ThrusterConfig;
                SpaceShip.PlayerShip.ShipData.ThrusterDatas.Add(ThrusterData.GetDataFromConfig(thrusterConfig));
                break;

            case EShopItemType.LaserCannons:
                var laserConfig = inConfig as LaserCannonConfig;
                SpaceShip.PlayerShip.ShipData.LaserCannonDatas.Add(LaserCannonData.GetDataFromConfig(laserConfig));
                break;

            case EShopItemType.Railguns:
                var railgunConfig = inConfig as RailGunConfig;
                SpaceShip.PlayerShip.ShipData.RailgunDatas.Add(RailGunData.GetDataFromConfig(railgunConfig));
                break;

            case EShopItemType.MissileLaunchers:
                var missileConfig = inConfig as MissileLauncherConfig;
                SpaceShip.PlayerShip.ShipData.MissileLauncherDatas.Add(MissileLauncherData.GetDataFromConfig(missileConfig));
                break;

            case EShopItemType.ShieldGenerators:
                var shieldConfig = inConfig as ShieldGeneratorConfig;
                SpaceShip.PlayerShip.ShipData.ShieldGeneratorData = ShieldGeneratorData.GetDataFromConfig(shieldConfig);
                break;

            case EShopItemType.RailRounds:
                SpaceShip.PlayerShip.AddRailRounds(inConfig.Quantity);
                break;

            case EShopItemType.Missiles:
                SpaceShip.PlayerShip.AddMissiles(inConfig.Quantity);
                break;

            case EShopItemType.Repairs:
                //SpaceShip.PlayerShip.HullHealth.AddHealth(SpaceShip.PlayerShip.ShipData.HealthDatas[0].Health);
                foreach (var healthEntity in SpaceShip.PlayerShip.HealthEntities)
                {
                    healthEntity.AddHealth(healthEntity.MaxHealth);//TODO: change this to use the ship data
                }
                break;
        }

        var canHaveMultiples = inConfig.ShopItemType switch
        {
            EShopItemType.Vault
            or EShopItemType.Railguns
            or EShopItemType.Missiles
            or EShopItemType.LaserCannons
            or EShopItemType.Thrusters
            or EShopItemType.Armor
            or EShopItemType.RailRounds
            or EShopItemType.Sensors
            or EShopItemType.MissileLaunchers
            or EShopItemType.Turrets => true,
            _ => false
        };

        if (canHaveMultiples)
            PlayerData.Data.AddActiveItem(inConfig.Id, inConfig.Quantity, false, false);

        if (inIsPurchase)
        {
            PlayerData.Data.AddCredits(-inConfig.Price);
            PlayerData.Data.AddOwnedItem(inConfig.Id, inConfig.Quantity);
        }

        //re-init the whole ship
        SpaceShip.PlayerShip.Init(PlayerData.Data.ShipData, true);

        PlayerData.OnShipDataChanged?.Invoke();
    }

    public void TryDeactivateItem(ShopItemConfig inConfig)
    {
        var isActiveShieldGenerator = inConfig.ShopItemType.Equals(EShopItemType.ShieldGenerators) && inConfig.Id.Equals(PlayerData.Data.ShipData.ShieldGeneratorData.Id);
        var isVault = inConfig.ShopItemType.Equals(EShopItemType.Vault);
        if (isActiveShieldGenerator)
        {
            PlayerData.Data.ShipData.ShieldGeneratorData = null;
            PlayerData.OnShipDataChanged?.Invoke();
        }
        else if (isVault)
        {
            if (PlayerData.Data.ShipData.VaultDatas == null || PlayerData.Data.ShipData.VaultDatas.Count == 0)
            {
                Debug.Log($"<color=red>No vaults found in ship data.</color>");
                return;
            }

            //TODO: copy vault datas, try to distribute crystals to other vaults, if not possible, show confirmation dialog
            //on if all crystals fit, remove vault and replace vaults with copies

            //for now, do this...
            var vaultAmounts = new List<int>();

            foreach (var vaultData in PlayerData.Data.ShipData.VaultDatas)
            {
                if (vaultData.Id.Equals(inConfig.Id))
                    vaultAmounts.Add(vaultData.GetUsedStorage());
                else
                    vaultAmounts.Add(-1);//ignore other vaults
            }

            var indexToRemove = -1;
            var minCrystalsFound = 9999;

            for (int i = 0; i < vaultAmounts.Count; i++)
            {
                if (vaultAmounts[i] == 0)
                {
                    indexToRemove = i;
                    break;
                }
                else if (vaultAmounts[i] > -1)
                {
                    if (vaultAmounts[i] < minCrystalsFound)
                    {
                        minCrystalsFound = vaultAmounts[i];
                        indexToRemove = i;
                    }
                }
            }

            if (PlayerData.Data.ShipData.VaultDatas.Count == 1 && minCrystalsFound > 0 && minCrystalsFound != 9999)
            {
                //only vault has crystals, show confirmation dialog
                UIConfirmPanel.IN.Show("Deactivate Vault?", $"This vault contains {minCrystalsFound} crystals. Deactivating it will lose all crystals.\nAre you sure?", () =>
                {
                    SpaceShip.PlayerShip.ShipData.VaultDatas.RemoveAt(indexToRemove);
                    SpaceShip.PlayerShip.RecalculateCrystalsInVaults();
                    PlayerData.Data.RemoveActiveItem(inConfig.Id, inConfig.Quantity);
                    PlayerData.OnShipDataChanged?.Invoke();
                });
            }
            else if (minCrystalsFound == 0 || minCrystalsFound == 9999)
            {
                //found an empty vault, just remove it
                SpaceShip.PlayerShip.ShipData.VaultDatas.RemoveAt(indexToRemove);
                SpaceShip.PlayerShip.RecalculateCrystalsInVaults();
                PlayerData.Data.RemoveActiveItem(inConfig.Id, inConfig.Quantity);
                PlayerData.OnShipDataChanged?.Invoke();
            }
            else if (minCrystalsFound > 0)
            {
                //vault has crystals, show confirmation dialog
                UIConfirmPanel.IN.Show("Deactivate Vault?", $"This vault contains crystals. Deactivating it may lose some crystals.\nAre you sure?", () =>
                {
                    var crystalsToTransfer = PlayerData.Data.ShipData.VaultDatas[indexToRemove].StoredCrystalsDict;

                    //sort these backwards so that the most expensive crystals are transferred first
                    var sortedCrystals = new List<KeyValuePair<string, int>>(crystalsToTransfer);

                    foreach (var kvp in crystalsToTransfer)
                    {
                        sortedCrystals.Add(kvp);
                    }

                    sortedCrystals.Reverse();

                    foreach (var kvp in sortedCrystals)
                    {
                        var crystalType = kvp.Key;
                        var amount = kvp.Value;

                        //TODO: fix bug here when deleting larger vault and wrong crystals are transferred

                        // Try to distribute crystals to other vaults
                        for (int i = 0; i < PlayerData.Data.ShipData.VaultDatas.Count; i++)
                        {
                            if (i == indexToRemove) continue; // Skip the vault being deactivated

                            var vault = PlayerData.Data.ShipData.VaultDatas[i];
                            if (vault.AllowedCrystalTypes.HasFlag((ECrystalType)Enum.Parse(typeof(ECrystalType), crystalType)))
                            {
                                var availableSpace = vault.Capacity - vault.GetUsedStorage();
                                var inAmountToAdd = Mathf.Min(amount, availableSpace);
                                vault.StoredCrystalsDict[crystalType] += inAmountToAdd;
                                amount -= inAmountToAdd;
                                if (amount == 0)
                                    break; // All crystals of this type have been transferred
                            }
                        }
                    }

                    SpaceShip.PlayerShip.ShipData.VaultDatas.RemoveAt(indexToRemove);
                    SpaceShip.PlayerShip.RecalculateCrystalsInVaults();
                    PlayerData.Data.RemoveActiveItem(inConfig.Id, inConfig.Quantity);
                    PlayerData.OnShipDataChanged?.Invoke();
                });
            }
        }
        else
            DeactivateItem(inConfig);

        SpaceShip.PlayerShip.Init(PlayerData.Data.ShipData, true);
    }
    
    private void DeactivateItem(ShopItemConfig inConfig)
    {
        switch (inConfig.ShopItemType)
        {
            // case EShopItemType.Vault:
            //     var firstVaultIndex = SpaceShip.PlayerShip.ShipData.VaultDatas.FindIndex(v => v.Id.Equals(inConfig.Id));
            //     if (firstVaultIndex >= 0)
            //     {
            //         //TODO: get crystals and see if they can be distributed to other vaults, if not, show confirmation dialog
            //         if (SpaceShip.PlayerShip.ShipData.VaultDatas[firstVaultIndex].StoredCrystalsDict.Count > 0)
            //         {
            //             SpaceShip.PlayerShip.ShipData.VaultDatas.RemoveAt(firstVaultIndex);
            //         }
            //     }
            //     else
            //         Debug.Log($"<color=red>Vault with ID {inConfig.Id} not found in ship data.</color>");
            //     break;

            case EShopItemType.Thrusters:
                var firstThrusterIndex = SpaceShip.PlayerShip.ShipData.ThrusterDatas.FindIndex(t => t.Id.Equals(inConfig.Id));
                if (firstThrusterIndex >= 0)
                    SpaceShip.PlayerShip.ShipData.ThrusterDatas.RemoveAt(firstThrusterIndex);
                else
                    Debug.Log($"<color=red>Thruster with ID {inConfig.Id} not found in ship data.</color>");
                break;

            case EShopItemType.LaserCannons:
                var firstLaserCannonIndex = SpaceShip.PlayerShip.ShipData.LaserCannonDatas.FindIndex(l => l.Id.Equals(inConfig.Id));
                if (firstLaserCannonIndex >= 0)
                    SpaceShip.PlayerShip.ShipData.LaserCannonDatas.RemoveAt(firstLaserCannonIndex);
                else
                    Debug.Log($"<color=red>Laser Cannon with ID {inConfig.Id} not found in ship data.</color>");
                break;

            case EShopItemType.Railguns:
                var firstRailgunIndex = SpaceShip.PlayerShip.ShipData.RailgunDatas.FindIndex(r => r.Id.Equals(inConfig.Id));
                if (firstRailgunIndex >= 0)
                    SpaceShip.PlayerShip.ShipData.RailgunDatas.RemoveAt(firstRailgunIndex);
                else
                    Debug.Log($"<color=red>Railgun with ID {inConfig.Id} not found in ship data.</color>");
                break;

            case EShopItemType.MissileLaunchers:
                var firstMissileLauncherIndex = SpaceShip.PlayerShip.ShipData.MissileLauncherDatas.FindIndex(m => m.Id.Equals(inConfig.Id));
                if (firstMissileLauncherIndex >= 0)
                    SpaceShip.PlayerShip.ShipData.MissileLauncherDatas.RemoveAt(firstMissileLauncherIndex);
                else
                    Debug.Log($"<color=red>Missile Launcher with ID {inConfig.Id} not found in ship data.</color>");
                break;

                // Handle other item types as needed
        }
        PlayerData.Data.RemoveActiveItem(inConfig.Id, inConfig.Quantity, true);//calls PlayerData.OnShipDataChanged?.Invoke();
    }

    public void HandleShowButtonPress()
    {
        Show(this.shopConfig);
    }

    //called by buy/sell mode buttons
    public void SetBuyMode(bool inIsBuyMode)
    {            
        foreach (var go in sellModeItemsToShow)
        {
            go.SetActive(!inIsBuyMode);
        }

        foreach (var go in buyModeItemsToShow)
        {
            go.SetActive(inIsBuyMode);
        }

        if (!inIsBuyMode)
            this.tradePanel.Init(this.crystalTradePricesConfig);
    }
}