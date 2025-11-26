using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using TMPro;

public class UiHullDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hullNameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI percentText;

    [Space, SerializeField] private Image hullIconImage;

    [Space, SerializeField] private ProceduralImage cellPrefab;
    [SerializeField] private RectTransform cellsContainer;
    [SerializeField] private GridLayoutGroup cellsGrid;

    [Space, SerializeField] private Color emptyColor;
    [SerializeField] private Color fullColor;

    [SerializeField] private bool shouldShowColorizedCells;

    private List<ProceduralImage> cells = new();

    private bool isInitialized = false;

    private void Awake()
    {
        this.cellPrefab.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (this.isInitialized)
            return;

        StartCoroutine(InitCo());
    }

    private IEnumerator InitCo()
    {
        while (PlayerData.Data == null || PlayerData.Data.ShipData == null)
        {
            yield return null; // Wait until PlayerData and ShipData are initialized
        }

        PlayerData.OnShipDataChanged += Configure;
        Configure();

        this.isInitialized = true;
        yield break;
    }

    private void OnDestroy()
    {
        PlayerData.OnShipDataChanged -= Configure;
    }

    private void Configure()
    {
        this.hullNameText.text = $"Model: {PlayerData.Data.ShipData.Name}";

        RefreshStorageDisplay(PlayerData.Data.ShipData.NumUsedTiles, PlayerData.Data.ShipData.NumTotalTiles);

        var sb = new StringBuilder();
        sb.AppendLine($"Rotation: {PlayerData.Data.ShipData.RotationSpeed}");
        sb.AppendLine($"Deceleration: {PlayerData.Data.ShipData.DecelerationRate}");
        sb.AppendLine($"Thrusters: {PlayerData.Data.ShipData.ThrusterDatas.Count}");

        if (PlayerData.Data.ShipData.MaxTurrets > 0)
            sb.AppendLine($"Turrets: {PlayerData.Data.ShipData.TurretDatas.Count}/{PlayerData.Data.ShipData.MaxTurrets}");

        this.statsText.text = sb.ToString();
    }

    public void RefreshStorageDisplay(int inUsedTiles, int inTotalTiles)
    {
        if (inTotalTiles <= 0)
        {
            this.percentText.text = "0%";
            return;
        }

        var percent = Mathf.RoundToInt(inUsedTiles / (float)inTotalTiles * 100f);
        this.percentText.text = $"{percent}%\n({inUsedTiles}/{inTotalTiles})";

        var numCellsToCreate = inTotalTiles - this.cells.Count;

        var didClear = false;

        if (numCellsToCreate < 0)
        {
            //Remove excess cells
            for (int i = this.cells.Count - 1; i >= inTotalTiles; i--)
            {
                Destroy(this.cells[i].gameObject);
                this.cells.RemoveAt(i);
            }
            numCellsToCreate = 0;
            didClear = true;
        }

        if (numCellsToCreate != 0 || didClear)
        {
            //calculate the size of the grid cells based on the total number of tiles
            int numCellsAcross = Mathf.CeilToInt(Mathf.Sqrt(inTotalTiles));

            var cellWidth = (this.cellsContainer.rect.width - (this.cellsGrid.spacing.x * (numCellsAcross - 1))) / numCellsAcross;
            var cellHeight = (this.cellsContainer.rect.height - (this.cellsGrid.spacing.y * (numCellsAcross - 1))) / numCellsAcross;

            this.cellsGrid.cellSize = new Vector2(cellWidth, cellHeight);

            this.cellsGrid.constraintCount = Mathf.CeilToInt(numCellsAcross);
        }

        //create new cells if needed
        for (int i = 0; i < numCellsToCreate; i++)
        {
            var cell = Instantiate(this.cellPrefab, this.cellsContainer);
            cell.gameObject.SetActive(true);
            cell.name = $"Cell_{this.cells.Count}";
            this.cells.Add(cell);
        }

        //colorize cells
    
        var data = PlayerData.Data.ShipData;
        int numReactorTiles = data.ReactorData?.NumTilesRequired ?? 0;
        int numBatteryTiles = data.BatteryData?.NumTilesRequired ?? 0;
        int numShieldTiles = data.ShieldGeneratorData?.NumTilesRequired ?? 0;

        var numVaultTiles = 0;
        foreach (var vault in data.VaultDatas)
        {
            numVaultTiles += vault.NumTilesRequired;
        }

        var numThrusterTiles = 0;
        foreach (var thruster in data.ThrusterDatas)
        {
            numThrusterTiles += thruster.NumTilesRequired;
        }

        var numRailgunTiles = 0;
        foreach (var railgun in data.RailgunDatas)
        {
            numRailgunTiles += railgun.NumTilesRequired;
        }

        var numLaserCannonTiles = 0;
        foreach (var laserCannon in data.LaserCannonDatas)
        {
            numLaserCannonTiles += laserCannon.NumTilesRequired;
        }

        var numMissileLauncherTiles = 0;
        foreach (var missileLauncher in data.MissileLauncherDatas)
        {
            numMissileLauncherTiles += missileLauncher.NumTilesRequired;
        }

        // var turretTiles = 0;
        // foreach (var turret in data.TurretDatas)
        // {
        //     turretTiles += turret.NumTilesRequired;
        // }

        var categoryColors = UIShopPanel.IN.CategoryColors;//TODO: move this to GlobalData/ScriptableObject

        var tooltipText = string.Empty;

        for (int i = 0; i < this.cells.Count; i++)
        {
            var cell = this.cells[i];
            cell.BorderWidth = 0;

            if (i < numReactorTiles)
            {
                cell.color = categoryColors[1];
                tooltipText = $"Reactor ({numReactorTiles})";
            }
            else if (i < numReactorTiles + numBatteryTiles)
            {
                cell.color = categoryColors[2];
                tooltipText = $"Battery ({numBatteryTiles})";
            }
            else if (i < numReactorTiles + numBatteryTiles + numShieldTiles)
            {
                cell.color = categoryColors[3];
                tooltipText = $"Shield ({numShieldTiles})";
            }
            else if (i < numReactorTiles + numBatteryTiles + numShieldTiles + numVaultTiles)
            {
                cell.color = categoryColors[4];
                tooltipText = $"Vault ({numVaultTiles})";
                if(data.VaultDatas.Count > 1)
                    tooltipText = $"{data.VaultDatas.Count} Vaults ({numVaultTiles})";
            }
            else if (i < numReactorTiles + numBatteryTiles + numShieldTiles + numVaultTiles + numThrusterTiles)
            {
                cell.color = categoryColors[5];
                tooltipText = $"Thruster ({numThrusterTiles})";
                if(data.ThrusterDatas.Count > 1)
                    tooltipText = $"{data.ThrusterDatas.Count} Thrusters ({numThrusterTiles})";
            }
            else if (i < numReactorTiles + numBatteryTiles + numShieldTiles + numVaultTiles + numThrusterTiles + numRailgunTiles)
            {
                cell.color = categoryColors[6];
                tooltipText = $"Railgun ({numRailgunTiles})";
                if(data.RailgunDatas.Count > 1)
                    tooltipText = $"{data.RailgunDatas.Count} Railguns ({numRailgunTiles})";
            }
            else if (i < numReactorTiles + numBatteryTiles + numShieldTiles + numVaultTiles + numThrusterTiles + numRailgunTiles + numLaserCannonTiles)
            {
                cell.color = categoryColors[7];
                tooltipText = $"Laser Cannon ({numLaserCannonTiles})";
                if(data.LaserCannonDatas.Count > 1)
                    tooltipText = $"{data.LaserCannonDatas.Count} Laser Cannons ({numLaserCannonTiles})";
            }
            else if (i < numReactorTiles + numBatteryTiles + numShieldTiles + numVaultTiles + numThrusterTiles + numRailgunTiles + numLaserCannonTiles + numMissileLauncherTiles)
            {
                cell.color = categoryColors[8];
                tooltipText = $"Missile Launcher ({numMissileLauncherTiles})";
                if(data.MissileLauncherDatas.Count > 1)
                    tooltipText = $"{data.MissileLauncherDatas.Count} Missile Launchers ({numMissileLauncherTiles})";
            }
            else
            {
                cell.color = categoryColors[0];
                cell.BorderWidth = 2f;
                tooltipText = $"Empty ({PlayerData.Data.ShipData.NumTotalTiles - inUsedTiles})";
            }

            if (cell.TryGetComponent<TooltipTrigger>(out var tooltipTrigger))
            {
                tooltipTrigger.TooltipText = tooltipText;
            }

            if (!shouldShowColorizedCells)
            {
                var isFull = i < inUsedTiles;
                cell.color = isFull ? this.fullColor : this.emptyColor;
                cell.BorderWidth = isFull ? 0 : 2f;
            }
        }
    }
}