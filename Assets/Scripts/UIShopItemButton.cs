using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using static CrystalData;

public class UIShopItemButton : MonoBehaviour
{
    private static WaitForSecondsRealtime _waitForSecondsRealtime3 = new(3f);
    [SerializeField] private bool isDetailsView;
    [Space, SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI subtitleText;

    [ShowIf("isDetailsView")]
    [SerializeField] private TextMeshProUGUI quantityText, spaceText, buttonText, numberOwnedText, descriptionText, outputText;
    [ShowIf("isDetailsView")]
    [SerializeField] private Image icon;
    [SerializeField] private Image iconShadow;

    [ShowIf("isDetailsView")]
    [Space, SerializeField] private GameObject buyButton, equipButton, disabledButton, removeButton, outputDisplay;
    [SerializeField] private GameObject activeBadge;

    [SerializeField] private ResetScrollRectOnEnable scrollRectResetter;

    [ShowIf("isDetailsView")]
    [Space, SerializeField] private GameObject notEnoughCreditsIcon;
    [ShowIf("isDetailsView")]
    [SerializeField] private GameObject notEnoughSpaceIcon;
    [ShowIf("isDetailsView")]
    [Space, SerializeField] private Image spaceColorRect;

    [HideIf("isDetailsView")]
    [Space, SerializeField] private Graphic[] selectedHighlightGraphics;
    [HideIf("isDetailsView")]
    [SerializeField] private Color[] selectedHighlightColors;

    private ShopItemConfig itemConfig;

    private string outputMessage;

    private void OnEnable()
    {
        PlayerData.OnCreditsChanged += HandleCreditsChanged;
        PlayerData.OnShipDataChanged += ConfigureButton;
    }

    private void OnDisable()
    {
        PlayerData.OnCreditsChanged -= HandleCreditsChanged;
        PlayerData.OnShipDataChanged -= ConfigureButton;
    }

    private void Update()
    {
        if (this.isDetailsView) return;

        if(UIShopPanel.IN == null) return;

        UIShopPanel.IN.SelectionAreaRect.rect.Contains(this.transform.position);
        var isSelected = this.transform.position.x > (Screen.width * 0.5f - UIShopPanel.IN.SelectionAreaRect.rect.width * 0.5f) &&
                         this.transform.position.x < (Screen.width * 0.5f + UIShopPanel.IN.SelectionAreaRect.rect.width * 0.5f);

        UIShopPanel.IN.SelectionAreaRect.rect.Contains(this.transform.position);
        SetSelected(isSelected);
    }

    private void SetSelected(bool inIsSelected)
    {
        foreach (var graphic in this.selectedHighlightGraphics)
        {
            graphic.color = inIsSelected ? this.selectedHighlightColors[1] : this.selectedHighlightColors[0];
        }

        if (inIsSelected)
            UIShopPanel.OnShopItemSelected?.Invoke(this.itemConfig);
    }

    public void Configure(ShopItemConfig inData)
    {
        this.itemConfig = inData;
        this.nameText.text = inData.Name;
        this.subtitleText.text = inData.SubTitle;
        this.subtitleText.gameObject.SetActive(!string.IsNullOrWhiteSpace(inData.SubTitle));
        this.icon.sprite = inData.Sprite;
        this.icon.color = inData.Color;
        if (this.iconShadow != null)
            this.iconShadow.sprite = inData.Sprite;

        ConfigureButton();

        if (!this.isDetailsView) return;

        this.quantityText.text = inData.Quantity > 1 ? $"x{inData.Quantity}" : string.Empty;

        SetDescriptionText(inData);

        this.spaceText.text = $"{inData.NumTilesRequired}";
        this.spaceColorRect.transform.gameObject.SetActive(inData.NumTilesRequired > 0);
        this.spaceColorRect.color = UIShopPanel.IN.GetShopCategoryColor(inData.ShopItemType);

        this.buttonText.text = $"BUY <sprite name=\"Credits\">{inData.Price}";
        
        StartCoroutine(HideOutputDisplay());
    }
    
    private void SetDescriptionText(ShopItemConfig inConfig)
    {
        this.scrollRectResetter.ResetPosition();

        if (inConfig.OverwriteTypeDescription)
        {
            this.descriptionText.text = inConfig.Description;
            return;
        }

        var sb = new StringBuilder();

        var description = string.IsNullOrWhiteSpace(inConfig.Description) ? string.Empty : $"\n\n{inConfig.Description}";
        sb.Append(description);

        var tinySpacer = "<size=12> </size>";

        switch (inConfig.ShopItemType)
        {
            case EShopItemType.Ship:
                var shipConfig = inConfig as ShipConfig;
                sb.AppendLine($"<b>Hull Tiles</b>: {shipConfig.NumTiles}");
                sb.AppendLine(tinySpacer);
                sb.AppendLine($"<b>Reactor</b>: {shipConfig.ReactorData.SubTitle}");
                sb.AppendLine(tinySpacer);
                sb.AppendLine($"<b>Battery</b>: {shipConfig.BatteryData.SubTitle}");
                sb.AppendLine(tinySpacer);

                // Thrusters
                if (shipConfig.ThrusterDatas.Length > 0)
                {
                    var thrusterGroups = shipConfig.ThrusterDatas
                        .GroupBy(t => t.SubTitle)
                        .ToList();
                    sb.AppendLine($"<b>Thrusters</b>: {shipConfig.ThrusterDatas.Length}");
                    foreach (var group in thrusterGroups)
                    {
                        var first = group.First();
                        sb.AppendLine($"• {group.Key} ({group.Count()})\n  Accel: {first.AccelerationRate}, Discharge: {first.DischargeRate}");
                    }
                    sb.AppendLine(tinySpacer);
                }

                // Vaults
                if (shipConfig.VaultDatas.Length > 0)
                {
                    var vaultGroups = shipConfig.VaultDatas
                        .GroupBy(v => v.SubTitle)
                        .ToList();
                    sb.AppendLine($"<b>Vaults</b>: {shipConfig.VaultDatas.Length}");
                    foreach (var group in vaultGroups)
                    {
                        var first = group.First();
                        sb.AppendLine($"• {group.Key} ({group.Count()})  Capacity: {first.Capacity}");
                    }
                    sb.AppendLine(tinySpacer);
                }

                // Shield Generator
                if (shipConfig.ShieldGeneratorData != null)
                {
                    var sg = shipConfig.ShieldGeneratorData;
                    sb.AppendLine($"<b>Shield Generator</b>:\n{sg.SubTitle}");
                    sb.AppendLine("<size=8> </size>");
                    sb.AppendLine($"Charge: {sg.ChargeRate}/s  Discharge: {sg.DischargeRate}/s  Delay: {sg.DelayToDischarge}s");
                    sb.AppendLine("<size=8> </size>");
                    for (int i = 0; i < sg.MaxShieldRings; i++)
                    {
                        sb.AppendLine($"• Ring {i + 1} Health: { (i == 0 ? sg.InnerShieldBrickHealth : i == 1 ? sg.MiddleShieldBrickHealth : sg.OuterShieldBrickHealth)}");
                    }
                    sb.AppendLine(tinySpacer);
                }

                // Railguns
                if (shipConfig.RailGunDatas.Length > 0)
                {
                    var railgunGroups = shipConfig.RailGunDatas
                        .GroupBy(r => r.SubTitle)
                        .ToList();
                    sb.AppendLine($"<b>Railguns</b>: {shipConfig.RailGunDatas.Length}");
                    foreach (var group in railgunGroups)
                    {
                        var first = group.First();
                        sb.AppendLine($"• {group.Key} ({group.Count()})  RPS: {first.RoundsPerSecond}\n  Ammo:  L:{first.MaxRoundsLight} M:{first.MaxRoundsMedium}  H:{first.MaxRoundsHeavy}");
                    }
                    sb.AppendLine(tinySpacer);
                }

                // Missile Launchers
                if (shipConfig.MissileLauncherDatas.Length > 0)
                {
                    var mlGroups = shipConfig.MissileLauncherDatas
                        .GroupBy(m => m.SubTitle)
                        .ToList();
                    sb.AppendLine($"<b>Missile Launchers</b>: {shipConfig.MissileLauncherDatas.Length}");
                    foreach (var group in mlGroups)
                    {
                        var first = group.First();
                        sb.AppendLine($"• {group.Key} ({group.Count()})  # Missiles: {first.MaxMissiles}");
                    }
                    sb.AppendLine(tinySpacer);
                }

                // Laser Cannons
                if (shipConfig.LaserCannonDatas.Length > 0)
                {
                    var lcGroups = shipConfig.LaserCannonDatas
                        .GroupBy(l => l.SubTitle)
                        .ToList();
                    sb.AppendLine($"<b>Laser Cannons</b>: {shipConfig.LaserCannonDatas.Length}");
                    foreach (var group in lcGroups)
                    {
                        var first = group.First();
                        if (first.IsMiningLaser)
                            sb.AppendLine($"• {group.Key} ({group.Count()})  Mining Laser\n  ChargeRate: {first.ChargeRate}");
                        else
                            sb.AppendLine($"• {group.Key} ({group.Count()})\n  ChargeRate: {first.ChargeRate}  Damage: {first.MaxCharge}");
                    }
                    sb.AppendLine(tinySpacer);
                }

                // Turrets
                if (shipConfig.TurretDatas.Length > 0)
                {
                    var turretGroups = shipConfig.TurretDatas
                        .GroupBy(t => t.SubTitle)
                        .ToList();
                    sb.AppendLine($"<b>Turrets</b>: {shipConfig.TurretDatas.Length}");
                    foreach (var group in turretGroups)
                    {
                        var first = group.First();
                        sb.AppendLine($"• {group.Key} ({group.Count()})\n  Rot Speed: {first.RotationSpeed}  # Mounts: {first.NumMounts}");
                    }
                    sb.AppendLine(tinySpacer);
                }
                break;
            case EShopItemType.Reactor:
                var reactorConfig = inConfig as ReactorConfig;
                sb.AppendLine($"Recharge rate: {reactorConfig.EnergyRechargeRate}/s");
                this.descriptionText.text = sb.ToString();
                break;
            case EShopItemType.Battery:
                var batteryConfig = inConfig as BatteryConfig;
                sb.AppendLine($"Energy Capacity: {batteryConfig.MaxEnergyCharge}");
                this.descriptionText.text = sb.ToString();
                break;
            case EShopItemType.ShieldGenerators:
                var shieldConfig = inConfig as ShieldGeneratorConfig;
                sb.AppendLine($"Charge Rate: {shieldConfig.ChargeRate}/s  Discharge Rate: {shieldConfig.DischargeRate}/s  Delay: {shieldConfig.DelayToDischarge}s");
                for (int i = 0; i < shieldConfig.MaxShieldRings; i++)
                {
                    sb.AppendLine($"• Ring {i + 1}  Health: { (i == 0 ? shieldConfig.InnerShieldBrickHealth : i == 1 ? shieldConfig.MiddleShieldBrickHealth : shieldConfig.OuterShieldBrickHealth)}");
                }
                break;
            case EShopItemType.Vault:
                var vaultConfig = inConfig as VaultConfig;
                sb.AppendLine($"Capacity: {vaultConfig.Capacity}");
                sb.AppendLine(tinySpacer);
                sb.AppendLine($"Allowed Crystals:");
                foreach (ECrystalType crystalType in Enum.GetValues(vaultConfig.AllowedCrystalTypes.GetType()))
                {
                    int intValue = (int)crystalType;
                    if (intValue != 0 && vaultConfig.AllowedCrystalTypes.HasFlag(crystalType))
                    {
                        var crystalHexColor = GlobalData.GetCrystalColor(crystalType).ToHexString();
                        var crystalSpriteName = $"<sprite name=\"Crystals_Fill\" color=#{crystalHexColor}><sprite name=\"Crystals_Outline\">";

                        sb.Append($"{crystalSpriteName}  ");
                    }
                }
                sb.AppendLine(tinySpacer);
                sb.AppendLine($"\nRadiation Shielding: {Mathf.RoundToInt(vaultConfig.RadiationShielding * 100)}%");
                sb.AppendLine($"Max Rads: {vaultConfig.MaxRadiationLevel}   Flushes: {vaultConfig.MaxRadiationFlushes}");
                break;
            case EShopItemType.Railguns:
                var railgunConfig = inConfig as RailGunConfig;
                sb.AppendLine($"Rounds Per Second: {railgunConfig.RoundsPerSecond}");
                sb.AppendLine(tinySpacer);
                sb.AppendLine($"Ammo Capacity:  Light:{railgunConfig.MaxRoundsLight}  Medium:{railgunConfig.MaxRoundsMedium}  Heavy:{railgunConfig.MaxRoundsHeavy}");
                break;
            case EShopItemType.Missiles:
                var missileConfig = inConfig as MissileLauncherConfig;
                sb.AppendLine($"Max Missiles: {missileConfig.MaxMissiles}");
                break;
            case EShopItemType.LaserCannons:
                var laserConfig = inConfig as LaserCannonConfig;
                if (laserConfig.IsMiningLaser)
                    sb.AppendLine($"Mining Laser - ChargeRate: {laserConfig.ChargeRate}, Type: {laserConfig.MiningToolType}%");
                else
                    sb.AppendLine($"ChargeRate: {laserConfig.ChargeRate}, Damage: {laserConfig.MaxCharge}");
                break;
            case EShopItemType.Thrusters:
                var thrusterConfig = inConfig as ThrusterConfig;
                sb.AppendLine($"Acceleration Rate: {thrusterConfig.AccelerationRate}");
                sb.AppendLine(tinySpacer);
                sb.AppendLine($"Discharge Rate: {thrusterConfig.DischargeRate}");
                sb.AppendLine(tinySpacer);
                sb.AppendLine($"Max Accel: {thrusterConfig.MaxAcceleration}");
                break;
            case EShopItemType.Armor:
                sb.AppendLine($"Provides additional hull protection.{description}");
                break;
            case EShopItemType.RailRounds:
                sb.AppendLine($"Ammunition for railgun weapon systems.{description}");
                break;
            case EShopItemType.MissileLaunchers:
                var mlConfig = inConfig as MissileLauncherConfig;
                sb.AppendLine($"Missile launching system with a capacity of {mlConfig.MaxMissiles} missiles.{description}");
                break;
            case EShopItemType.Turrets:
                var turretConfig = inConfig as TurretConfig;
                sb.AppendLine($"Automated weapon system with {turretConfig.NumMounts} mounts.{description}");
                break;
            case EShopItemType.Repairs:
                sb.AppendLine($"Restores your ship's hull or systems to full functionality.{description}");
                break;
            case EShopItemType.Sensors:
                sb.AppendLine($"Enhances detection and scanning capabilities.{description}");
                break;
            case EShopItemType.Tools:
                sb.AppendLine($"Various utility items to assist in your missions.{description}");
                break;
            default:
                sb.AppendLine(inConfig.Description);
                break;
        }
        
        description = sb.ToString();
        this.descriptionText.text = $"{description}";
    }

    private void HandleCreditsChanged(int inAmount)
    {
        ConfigureButton();
    }

    private void ConfigureButton()
    {
        if (this.itemConfig == null) return;

        var isOwnedItem = PlayerData.Data.OwnedItems.ContainsKey(this.itemConfig.Id) && PlayerData.Data.OwnedItems[this.itemConfig.Id] > 0;

        var isActiveShip = this.itemConfig.ShopItemType.Equals(EShopItemType.Ship) && this.itemConfig.Id.Equals(PlayerData.Data.ShipData.Id);
        var isActiveReactor = this.itemConfig.ShopItemType.Equals(EShopItemType.Reactor) && this.itemConfig.Id.Equals(PlayerData.Data.ShipData.ReactorData.Id);
        var isActiveBattery = this.itemConfig.ShopItemType.Equals(EShopItemType.Battery) && this.itemConfig.Id.Equals(PlayerData.Data.ShipData.BatteryData.Id);
        var isActiveShieldGenerator = this.itemConfig.ShopItemType.Equals(EShopItemType.ShieldGenerators) && PlayerData.Data.ShipData.ShieldGeneratorData != null && this.itemConfig.Id.Equals(PlayerData.Data.ShipData.ShieldGeneratorData.Id);

        var isActiveItem = isActiveShip || isActiveReactor || isActiveBattery || isActiveShieldGenerator;
        this.activeBadge.SetActive(isActiveItem);

        if (!this.isDetailsView) return;

        //below here only for details view --------------------------------------------

        var hasEnoughCredits = PlayerData.Data.Credits >= this.itemConfig.Price;

        var currentSpaceOccupiedByItem = 0;

        switch (this.itemConfig.ShopItemType)
        {
            case EShopItemType.Reactor:
                currentSpaceOccupiedByItem = PlayerData.Data.ShipData.ReactorData.NumTilesRequired;
                break;
            case EShopItemType.Battery:
                currentSpaceOccupiedByItem = PlayerData.Data.ShipData.BatteryData.NumTilesRequired;
                break;
            case EShopItemType.ShieldGenerators:
                if (PlayerData.Data.ShipData.ShieldGeneratorData != null)
                    currentSpaceOccupiedByItem = PlayerData.Data.ShipData.ShieldGeneratorData.NumTilesRequired;
                break;
        }

        var numAvailableTilesInShip = PlayerData.Data.ShipData.GetNumAvailableTiles();
        var spaceRequired = this.itemConfig.Quantity * this.itemConfig.NumTilesRequired;
        var hasEnoughSpace = numAvailableTilesInShip + currentSpaceOccupiedByItem >= spaceRequired;

        this.notEnoughCreditsIcon.SetActive(!hasEnoughCredits);
        this.notEnoughSpaceIcon.SetActive(!hasEnoughSpace);

        //print($"Configuring button for {this.itemData.Name} - Credits: {PlayerData.Data.Credits}, Price: {this.itemData.Price}, Space: {numAvailableTilesInShip}/{spaceRequired}");

        if (!hasEnoughCredits)
            this.outputMessage = $"Not enough credits!\nNeed {this.itemConfig.Price - PlayerData.Data.Credits} more";
        else if (!hasEnoughSpace)
            this.outputMessage = $"Not enough space in ship!\nHave: {numAvailableTilesInShip + currentSpaceOccupiedByItem} Need: {spaceRequired}";

        this.disabledButton.SetActive((!hasEnoughCredits || !hasEnoughSpace) && !isActiveItem);

        var shouldShowBuyButton = !isActiveItem && !isOwnedItem;
        var shouldShowEquipButton = !shouldShowBuyButton;

        var canHaveMultiples = this.itemConfig.ShopItemType switch
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

        if (!canHaveMultiples)
        {
            this.buyButton.SetActive(shouldShowBuyButton);
            this.equipButton.SetActive(shouldShowEquipButton);

            this.removeButton.SetActive(false);
            this.numberOwnedText.text = string.Empty;

            this.removeButton.SetActive(isActiveShieldGenerator);
            return;
        }

        //can have multiples --------------------------------------------
        shouldShowBuyButton = false;
        shouldShowEquipButton = false;

        var totalActive = PlayerData.Data.NumActiveItems(this.itemConfig.Id);
        var totalOwned = PlayerData.Data.NumItemsOwned(this.itemConfig.Id);

        this.removeButton.SetActive(totalActive > 0);

        if (totalOwned > 0)
        {
            var denominatorSize = this.numberOwnedText.fontSize * 0.8f;
            this.numberOwnedText.text = $"{totalActive}<size={denominatorSize}>/{totalOwned}</size>";

            shouldShowEquipButton = totalActive < totalOwned;
            shouldShowBuyButton = !shouldShowEquipButton || totalOwned == 0;
        }
        else
        {
            this.numberOwnedText.text = string.Empty;
            shouldShowBuyButton = true;
        }

        this.buyButton.SetActive(shouldShowBuyButton);
        this.equipButton.SetActive(shouldShowEquipButton);
    }

    public void HandleBuyButtonClick()
    {
        UIShopPanel.IN.TryBuyItem(this.itemConfig);
    }

    public void HandleEquipButtonClick()
    {
        UIShopPanel.IN.TryBuyItem(this.itemConfig, false);
    }

    public void HandleDisabledButtonClick()
    {
        this.outputText.text = this.outputMessage;
        this.outputDisplay.SetActive(true);

        StartCoroutine(HideOutputDisplay());
    }

    public void HandleRemoveButtonClick()
    {
        UIShopPanel.IN.TryDeactivateItem(this.itemConfig);            
    }

    private IEnumerator HideOutputDisplay()
    {
        yield return _waitForSecondsRealtime3;
        this.outputDisplay.SetActive(false);
    }
}