using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static CrystalData;
using static LootData;
using static LootManager;

public class HUD : MonoBehaviour
{
    public static HUD IN;

    public static Action<int> OnRailRoundsChanged;
    public static Action<int> OnMissilesChanged;
    public static Action<int> OnShipHealthChanged;
    public static Action<int> OnEnergyChanged;
    public static Action<ShipData> OnCrystalsChanged;//Maybe change to Action<ListVaultData>>

    public static Action<ELootType,LootInMapData, Dictionary<ELootType, LootInMapData>> OnLootInLevelDataChanged;
    public static Action<ECrystalType,LootInMapData, Dictionary<ECrystalType, LootInMapData>> OnCrystalLootInLevelDataChanged;
    
    [Header("Count Displays")]
    [SerializeField] private TMP_Text mapLootDisplay;

    [SerializeField] private UICountDisplay[] crystalCountDisplays;
    [SerializeField] private UICountDisplay railRoundsCountDisplay;
    [SerializeField] private UICountDisplay missilesCountDisplay;
    [SerializeField] private UICountDisplay healthCountDisplay;
    [SerializeField] private UiReactorAndBatteryDisplay reactorAndBatteryDisplay;

    [Header("Buttons")]
    [SerializeField] private Button railgunButton;
    [SerializeField] private Button missilesButton;
    [SerializeField] private Button lasersButton;
    [SerializeField] private Button shieldsButton;
    [SerializeField] private Button dashButton;
    [Space]
    [SerializeField] private Image railgunButtonIcon;
    [SerializeField] private Image missilesButtonIcon;
    [SerializeField] private Image lasersButtonIcon;
    [SerializeField] private Image shieldsButtonIcon;
    [SerializeField] private Image dashButtonIcon;

    [Header("Animators")]
    [SerializeField] private Animator comsMonitorAnimator;
    [SerializeField] private Animator statsMonitorAnimator;

    public bool IsComsPanelOpen => this.isComsPanelOpen;
    public bool IsStatsPanelOpen => this.isStatsPanelOpen;

    [Header("Planet Display")]
    [SerializeField] private UiPlanetDisplay planetDisplay;

    [Header("Full Screen Viewer")]
    [SerializeField] private UiFullScreenViewer fullScreenViewer;
    public UiFullScreenViewer FullScreenViewer => this.fullScreenViewer;

    private bool isComsPanelOpen;
    private bool isStatsPanelOpen;

    private StringBuilder lootInMapSb = new();
    private StringBuilder crystalLootInMapSb = new();
    private StringBuilder combinedTextSb = new();

    public void Init(SpaceShip inPlayerSpaceShip)
    {
        RegisterEvents();

        this.planetDisplay.Init();

        this.railRoundsCountDisplay.SetMax(inPlayerSpaceShip.MaxRailRounds);
        SetRailRounds(inPlayerSpaceShip.NumRailRounds);

        this.missilesCountDisplay.SetMax(inPlayerSpaceShip.MaxMissiles);
        SetMissiles(inPlayerSpaceShip.NumMissiles);

        this.healthCountDisplay.SetMax(inPlayerSpaceShip.HullHealth.MaxHealth);
        SetHealth(inPlayerSpaceShip.HullHealth.Health);

        this.reactorAndBatteryDisplay.SetMaxBatteryLevel((int)inPlayerSpaceShip.EnergyLevel);
        this.reactorAndBatteryDisplay.SetGradient(GlobalData.IN.BatteryMeterGradients[(int)inPlayerSpaceShip.ShipData.ReactorData.SpriteIndex]);
        this.reactorAndBatteryDisplay.SetColors(inPlayerSpaceShip.ShipData.ReactorData.ReactorColor);

        SetEnergy((int)inPlayerSpaceShip.EnergyLevel);

        RefreshCrystals(inPlayerSpaceShip.ShipData);

        SetMissilesButtonActive(inPlayerSpaceShip.MaxMissiles > 0);
        SetShieldsButtonActive(inPlayerSpaceShip.ShipData.ShieldGeneratorData != null && inPlayerSpaceShip.ShipData.ShieldGeneratorData.MaxShieldRings > 0);
        SetLasersButtonActive(inPlayerSpaceShip.ShipData.LaserCannonDatas.Count > 0);
        SetRailgunButtonActive(inPlayerSpaceShip.ShipData.RailgunDatas.Count > 0);
    }

    private void RegisterEvents()
    {
        UnregisterEvents();

        OnRailRoundsChanged += SetRailRounds;
        OnMissilesChanged += SetMissiles;
        OnShipHealthChanged += SetHealth;
        OnEnergyChanged += SetEnergy;
        OnCrystalsChanged += RefreshCrystals;

        OnLootInLevelDataChanged += HandleLootInLevelDataChanged;
        OnCrystalLootInLevelDataChanged += HandleCrystalLootInLevelDataChanged;
    }

    private void HandleLootInLevelDataChanged(ELootType inLootType, LootInMapData inLootInMapData, Dictionary<ELootType, LootInMapData> inAllLootsInLevelData)
    {
        this.lootInMapSb.Clear();

        foreach (var kvp in inAllLootsInLevelData)
        {
            if (kvp.Value.TotalLoots == 0 || kvp.Value.TotalRocksWithLoot == 0) continue;
            var lootSpriteName = $"<sprite name=\"{kvp.Key}\">";
            this.lootInMapSb.Append($"{lootSpriteName}{kvp.Value.TotalLoots - kvp.Value.NumLoots}<color=#BBB>/{kvp.Value.TotalLoots}   <sprite name=\"Rock\" color=#AAAAAA>{kvp.Value.NumRocksWithLoot}/{kvp.Value.TotalRocksWithLoot}</color>\n");
        }

        RefreshLootInLevelDisplay();
    }

    private void HandleCrystalLootInLevelDataChanged(ECrystalType inCrystalType, LootInMapData inLootInMapData, Dictionary<ECrystalType, LootInMapData> inAllCrystalLootsInLevelData)
    {
        this.crystalLootInMapSb.Clear();

        foreach (var kvp in inAllCrystalLootsInLevelData)
        {
            if (kvp.Value.TotalLoots == 0 || kvp.Value.TotalRocksWithLoot == 0) continue;
            var crystalHexColor = GlobalData.GetCrystalColor(kvp.Key).ToHexString();
            var crystalSpriteName = $"<sprite name=\"Crystals_Fill\" color=#{crystalHexColor}><sprite name=\"Crystals_Outline\">";
            this.crystalLootInMapSb.Append($"{crystalSpriteName}{kvp.Value.TotalLoots - kvp.Value.NumLoots}<color=#BBB>/{kvp.Value.TotalLoots}   <sprite name=\"Rock\" color=#AAAAAA>{kvp.Value.NumRocksWithLoot}/{kvp.Value.TotalRocksWithLoot}</color>\n");
        }

        RefreshLootInLevelDisplay();
    }

    //TODO: this is temp.  Later will be added to Coms Monitor panel with glyph icons, etc.
    private void RefreshLootInLevelDisplay()
    {
        this.combinedTextSb.Clear();
        this.combinedTextSb.Append($"<color=yellow>Rocks In Map:</color>  {TileLoadingManager.IN.AllActiveRocksInLevel.Count}<color=#BBB>(Loot)/</color>{TileLoadingManager.IN.AllRocksInLevel.Count}<color=#BBB> (Total)</color>\n<size=10> </size>\n");
        this.combinedTextSb.Append("<color=yellow>Loot In Map:</color>\n");
        this.combinedTextSb.Append(this.lootInMapSb);
        this.combinedTextSb.Append("<size=15> </size>\n");
        this.combinedTextSb.Append(this.crystalLootInMapSb);
        this.mapLootDisplay.text = this.combinedTextSb.ToString();
    }

    public void SetRailRounds(int inValue)
    {
        this.railRoundsCountDisplay.SetValue(inValue);
    }

    public void SetMissiles(int inValue)
    {
        this.missilesCountDisplay.SetValue(inValue);
    }

    public void SetHealth(int inValue)
    {
        this.healthCountDisplay.SetValue(inValue);
    }

    public void SetEnergy(int inValue)
    {
        this.reactorAndBatteryDisplay.SetBatteryLevel(inValue);
    }

    public void RefreshCrystals(ShipData inShipData)
    {
        foreach (var display in this.crystalCountDisplays)
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

        var index = 0;
        foreach (var kvp in crystalCountsDict)
        {
            var crystalsDisplay = this.crystalCountDisplays[index];
            crystalsDisplay.gameObject.SetActive(kvp.Value > 0);
            crystalsDisplay.SetValue(kvp.Value);
            crystalsDisplay.SetIconColor(GlobalData.GetCrystalColor(kvp.Key));
            //crystalsDisplay.SetIconSprite(CrystalData.GetCrystalSprite(kvp.Key));
            ++index;
        }
    }

    public void SetRailgunButtonActive(bool inIsActive)
    {
        var canvasGroup = this.railgunButtonIcon.GetComponent<CanvasGroup>();
        canvasGroup.alpha = inIsActive ? 1 : .2f;

        this.railgunButton.interactable = inIsActive;
    }

    public void SetMissilesButtonActive(bool inIsActive)
    {
        var canvasGroup = this.missilesButtonIcon.GetComponent<CanvasGroup>();
        canvasGroup.alpha = inIsActive ? 1 : .2f;

        this.missilesButton.interactable = inIsActive;
    }

    public void SetLasersButtonActive(bool inIsActive)
    {
        var canvasGroup = this.lasersButtonIcon.GetComponent<CanvasGroup>();
        canvasGroup.alpha = inIsActive ? 1 : .2f;

        this.lasersButton.interactable = inIsActive;
    }

    public void SetShieldsButtonActive(bool inIsActive)
    {
        var canvasGroup = this.shieldsButtonIcon.GetComponent<CanvasGroup>();
        canvasGroup.alpha = inIsActive ? 1 : .2f;

        this.shieldsButton.interactable = inIsActive;
    }

    public void SetDashButtonActive(bool inIsActive)
    {
        var canvasGroup = this.dashButtonIcon.GetComponent<CanvasGroup>();
        canvasGroup.alpha = inIsActive ? 1 : .2f;

        this.dashButton.interactable = inIsActive;
    }

    public void ShowComsMonitor()
    {
        if (this.isComsPanelOpen) return;

        InputManager.OnEscapePress += HideComsMonitor;

        this.isComsPanelOpen = true;

        this.comsMonitorAnimator.SetTrigger("Show");
    }

    public void HideComsMonitor()
    {
        if (!this.isComsPanelOpen) return;

        InputManager.OnEscapePress -= HideComsMonitor;

        this.isComsPanelOpen = false;

        this.comsMonitorAnimator.SetTrigger("Hide");
    }

    public void ShowStatsMonitor()
    {
        if (this.isStatsPanelOpen) return;

        InputManager.OnEscapePress += HideStatsMonitor;

        this.isStatsPanelOpen = true;

        this.statsMonitorAnimator.SetTrigger("Show");
    }

    public void HideStatsMonitor()
    {
        if (!this.isStatsPanelOpen) return;

        InputManager.OnEscapePress -= HideStatsMonitor;

        this.isStatsPanelOpen = false;

        this.statsMonitorAnimator.SetTrigger("Hide");
    }

    private void OnDestroy()
    {
        if (SpaceShip.PlayerShip != null)
            UnregisterEvents();
    }

    public void UnregisterEvents()
    {
        OnRailRoundsChanged -= SetRailRounds;
        OnMissilesChanged -= SetMissiles;
        OnShipHealthChanged -= SetHealth;
        OnEnergyChanged -= SetEnergy;
        OnCrystalsChanged -= RefreshCrystals;
    }
}