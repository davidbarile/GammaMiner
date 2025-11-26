using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMapEditor : MonoBehaviour
{
    public static UIMapEditor IN;

    public static int TileRotation;

    public Vector2Int GridDims = new Vector2Int(20, 20);
    [Space]
    [SerializeField] private int editingMapIndex = 0;

    [ReadOnly, SerializeField] private MapData editingMapData;
    [Space]
    [SerializeField] private TextMeshProUGUI mapNumText;
    [SerializeField] private TextMeshProUGUI mapNameText;
    [Space] [SerializeField] private TextMeshProUGUI tileRotationText;
    [Space]
    [SerializeField] private TMP_InputField mapNameInput;
    [SerializeField] private TMP_InputField goToMapInput;

    [Space]
    [SerializeField] private Toggle[] tileGroupToggles;

    [Space]
    [SerializeField] private RectTransform grid;
    [SerializeField] private UIMapGridCell uiMapGridCellPrefab;
    [Space]
    [SerializeField] private Transform listContent;
    [SerializeField] private UITileInfoItem uiTileInfoItemPrefab;
    [SerializeField] private MapTileData emptyMapTileData;
    [SerializeField] private MapTileData[] allLevelTileDatas;

    [ReadOnly, SerializeField] private UIMapGridCell selectedCell;
    [ReadOnly, SerializeField] private UIMapGridCell startTileCell;
    [ReadOnly, SerializeField] private UITileInfoItem selectedTileInfoItem;

    private List<MapData> allMapDatas;
    private readonly string mapDataPath = "Assets/Resources/Data/Levels/";

    private UIMapGridCell[,] gridCellMatrix;
    private bool isInitialized;
    private int selectedTileGroupIndex = -1;

    private bool isRotationFlaggedForReset;

    private void Awake()
    {
        IN = this;

        this.gridCellMatrix = new UIMapGridCell[20, 20];
    }

    private void Start()
    {
        this.allMapDatas = Resources.LoadAll("Data/Maps", typeof(MapData)).Cast<MapData>().ToList();

        RefreshTilesList();

        LoadMapNum(0);
        this.isInitialized = true;
    }

    private void Update()
    {
        UIMapEditor.TileRotation = this.selectedCell != null ? (int)this.selectedCell.Rotation : 0;
        this.tileRotationText.text = $"{TileRotation}°";

        if (!this.isInitialized || this.selectedCell == null) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            IncrementSelectedCellRotation(10f);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            IncrementSelectedCellRotation(270f, true);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            IncrementSelectedCellRotation(350f);
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            IncrementSelectedCellRotation(90f, true);
    }

    private void IncrementSelectedCellRotation(float inDegrees, bool inSnapToGrid = false)
    {
        if (this.selectedCell == null) return;

        var rotation = this.selectedCell.Rotation;
        rotation += inDegrees;

        if (inSnapToGrid)
            rotation = Mathf.Round(rotation / 90f) * 90f;

        rotation = (rotation + 720) % 360f;

        this.selectedCell.SetRotation(rotation);
        SaveRotationToLevelData();
    }

    private void RefreshTilesList()
    {
        foreach (Transform child in this.listContent)
        {
            Destroy(child.gameObject);
        }

        var filteredTiles = this.selectedTileGroupIndex == -1 ? this.allLevelTileDatas.ToList() : this.allLevelTileDatas.Where(t => t.TileGroup == this.selectedTileGroupIndex).ToList();
        filteredTiles.Sort((a, b) => a.TileGroup.CompareTo(b.TileGroup));
        filteredTiles.Sort((a, b) => a.TileSceneName.CompareTo(b.TileSceneName));
        filteredTiles.Insert(0, this.emptyMapTileData);

        for (var i = 0; i < filteredTiles.Count; i++)
        {
            var data = filteredTiles[i];
            var tileItem = Instantiate(this.uiTileInfoItemPrefab, this.listContent);
            tileItem.SetTileData(data);
            tileItem.name = $"TileItem_{i}_{data.TileSceneName}";
        }
    }

    private void LoadMapNum(int inIndex)
    {
        if (this.editingMapIndex >= this.allMapDatas.Count)
            inIndex = 0;

        this.editingMapIndex = inIndex;

        this.editingMapData = this.allMapDatas[inIndex];
        RefreshLevelNameTexts();

        var offset = 3f;
        var spacer = 3f;

        //var numAcross = this.editingMapData.TileMatrix.GetLength(0);
        //var numDown = this.editingMapData.TileMatrix.GetLength(1);

        var numAcross = this.gridCellMatrix.GetLength(0);
        var numDown = this.gridCellMatrix.GetLength(1);

        var cellWidth = ((this.grid.rect.width - offset * 2) / numAcross) - spacer;

        for (int i = 0; i < numAcross; i++)
        {
            for (int j = 0; j < numDown; j++)
            {
                UIMapGridCell cell = null;

                if (!this.isInitialized)
                {
                    cell = Instantiate(this.uiMapGridCellPrefab, this.grid.transform);
                    this.gridCellMatrix[i, j] = cell;

                    var rt = cell.GetComponent<RectTransform>();
                    rt.pivot = new Vector2(0, 0);
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellWidth);
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellWidth);

                    var xPos = ((cellWidth + spacer) * i) + offset;
                    var yPos = ((cellWidth + spacer) * j) + offset;
                    rt.anchoredPosition = new Vector2(xPos, yPos);
                }
                else
                    cell = this.gridCellMatrix[i, j];

                cell.SetCoords(i, j);
                cell.SetSelected(false);

                var isStartTile = cell.Coords == this.editingMapData.StartTileCoords;
                cell.SetStartIconVisible(isStartTile);

                if (isStartTile)
                    this.startTileCell = cell;

                this.editingMapData.InitGrid(numAcross, numDown);

                var dataObj = this.editingMapData.GetTileDataObjectAtCoords(i, j);

                if (dataObj != null && dataObj.TileData != null)
                {
                    cell.SetTileSprite(dataObj.TileData);
                    cell.SetRotation(dataObj.Rotation);
                }
                else
                {
                    cell.SetTileSprite(null);
                    cell.SetRotation(0f);
                }
            }
        }

        this.selectedCell = null;
    }

    private void RefreshLevelNameTexts()
    {
        this.mapNumText.text = $"{this.editingMapData.MapNum}";
        this.mapNameText.text = $"{this.editingMapData.Name}";
        this.mapNameInput.text = this.editingMapData.Name;

        var placeHolder = this.goToMapInput.placeholder.GetComponent<TextMeshProUGUI>();
        placeHolder.color = Color.gray;
        placeHolder.text = $"Go to Lv #/{this.allMapDatas.Count}";
    }

    public void SetSelectedCell(UIMapGridCell inCell)
    {
        if (this.selectedCell != null)
            this.selectedCell.SetSelected(false);

        if (inCell == this.selectedCell)
        {
            this.selectedCell = null;
            return;
        }

        this.selectedCell = inCell;
        this.selectedCell.SetSelected(true);

        if (this.selectedTileInfoItem != null)
        {
            var data = this.selectedTileInfoItem.mapTileData;

            if (this.selectedCell != null)
            {
                if (this.isRotationFlaggedForReset)
                {
                    this.isRotationFlaggedForReset = false;
                    UIMapEditor.TileRotation = 0;
                }

                var isDataEmpty = data.TileSceneName.Equals(MapData.EMPTY_TILE);
                var tileRotation = isDataEmpty ? 0 : UIMapEditor.TileRotation;
                this.selectedCell.SetRotation(tileRotation);

                this.selectedCell.SetTileSprite(data);

                var dataObj = isDataEmpty ? new MapTileDataObject { TileData = null, Rotation = tileRotation } : new MapTileDataObject { TileData = data, Rotation = tileRotation };
                this.editingMapData.InsertTileDataObjectAtCoords(dataObj, this.selectedCell.Coords.x, this.selectedCell.Coords.y);
                SaveTileToLevelData(dataObj);
            }
        }
    }

    public void SetSelectedTileInfoItem(UITileInfoItem inItem)
    {
        if (this.selectedTileInfoItem != null)
            this.selectedTileInfoItem.SetSelected(false);

        if (inItem == this.selectedTileInfoItem)
        {
            this.selectedTileInfoItem = null;
            return;
        }

        this.isRotationFlaggedForReset = true;

        this.selectedTileInfoItem = inItem;
        this.selectedTileInfoItem.SetSelected(true);
    }

    private void SaveTileToLevelData(MapTileDataObject inDataObj)
    {
        //write to level scriptable object
        var x = this.selectedCell.Coords.x;
        var y = this.selectedCell.Coords.y;

        if (inDataObj != null && inDataObj.TileData != null)
            this.editingMapData.InsertTileDataObjectAtCoords(inDataObj, x, y);
        else
            this.editingMapData.InsertTileDataObjectAtCoords(null, x, y);
    }

    private void SaveRotationToLevelData()
    {
        //write to level scriptable object
        var x = this.selectedCell.Coords.x;
        var y = this.selectedCell.Coords.y;

        var index = (this.editingMapData.ArrayDims.x * y) + x;

        this.editingMapData.TileDataObjectsArray[index].Rotation = this.selectedCell.Rotation;
        this.editingMapData.SetDirty();
    }

    public void HandlePrevLevelButtonPress()
    {
        if (this.editingMapIndex > 0)
        {
            --this.editingMapIndex;
            LoadMapNum(this.editingMapIndex);
        }
    }

    public void HandleGoToLevelButtonPress()
    {
        if (int.TryParse(this.goToMapInput.text, out int levelNum))
        {
            if (levelNum > 0 && levelNum <= this.allMapDatas.Count)
            {
                this.goToMapInput.text = string.Empty;
                LoadMapNum(levelNum - 1);
            }
            else
            {
                this.goToMapInput.text = string.Empty;
                var placeHolder = this.goToMapInput.placeholder.GetComponent<TextMeshProUGUI>();
                placeHolder.text = $"Max: {this.allMapDatas.Count}";
                placeHolder.color = Color.red;

                Invoke(nameof(RefreshLevelNameTexts), 1.5f);
            }
        }
    }

    public void HandleNextLevelButtonPress()
    {
        if (this.editingMapIndex < this.allMapDatas.Count - 1)
        {
            ++this.editingMapIndex;
            LoadMapNum(this.editingMapIndex);
        }
    }

    public void HandleFillAllButtonPress()
    {
        if (this.selectedTileInfoItem == null) return;

        bool isDataEmpty = this.selectedTileInfoItem.mapTileData.TileSceneName.Equals(MapData.EMPTY_TILE);
        var data = isDataEmpty ? null : this.selectedTileInfoItem.mapTileData;

        var numAcross = this.gridCellMatrix.GetLength(0);
        var numDown = this.gridCellMatrix.GetLength(1);

        for (int i = 0; i < numAcross; i++)
        {
            for (int j = 0; j < numDown; j++)
            {
                var cell = this.gridCellMatrix[i, j];
                cell.SetSelected(false);
                cell.SetTileSprite(data);
                cell.SetRotation(UIMapEditor.TileRotation);

                var tileDataObj = isDataEmpty ? null : new MapTileDataObject { TileData = data, Rotation = UIMapEditor.TileRotation };
                this.editingMapData.InsertTileDataObjectAtCoords(tileDataObj, i, j);
            }
        }

        if (isDataEmpty)
            this.editingMapData.NumTilesInMap = 0;
    }

    public void HandleSetStartTileButtonPress()
    {
        if (this.selectedCell != null)
        {
            if (this.startTileCell != null)
                this.startTileCell.SetStartIconVisible(false);

            this.startTileCell = this.selectedCell;

            this.startTileCell.SetStartIconVisible(true);

            this.editingMapData.StartTileCoords = this.startTileCell.Coords;
        }
    }

    public void HandleNewLevelButtonPress()
    {
        var newLevelData = ScriptableObject.CreateInstance<MapData>();

        this.allMapDatas.Add(newLevelData);//this will not persist when editor stops

        var levelNum = this.allMapDatas.Count < 10 ? $"0{this.allMapDatas.Count}" : $"{this.allMapDatas.Count}";

        newLevelData.Name = $"Lv {this.allMapDatas.Count}";
        // if (!string.IsNullOrWhiteSpace(this.mapNameText.text))
        //     newLevelData.Name = $"{this.mapNameText.text} {this.allMapDatas.Count}";

        newLevelData.FileName = $"Lv-{levelNum}_LevelData";
        // if(!string.IsNullOrWhiteSpace(this.mapNameInput.text))
        //     newLevelData.FileName = $"Lv-{levelNum}_{this.mapNameText.text}";

        newLevelData.MapNum = this.allMapDatas.Count;

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.CreateAsset(newLevelData, $"{this.mapDataPath}{newLevelData.FileName}.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
#endif

        LoadMapNum(this.allMapDatas.Count - 1);
    }

    public void HandleRenameAssetButtonPress()
    {
#if UNITY_EDITOR
        var map = this.editingMapIndex + 1;
        var mapNum = map < 10 ? $"0{map}" : $"{map}";

        if (string.IsNullOrWhiteSpace(this.mapNameInput.text))
        {
            this.editingMapData.Name = $"Lv {map}";
            this.editingMapData.FileName = $"Lv-{mapNum}_LevelData";
        }
        else
        {
            this.editingMapData.Name = this.mapNameInput.text;
            this.editingMapData.FileName = $"Lv-{mapNum}_{this.editingMapData.Name}";
        }

        UnityEditor.EditorUtility.SetDirty(this.editingMapData);

        RefreshLevelNameTexts();

        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this.editingMapData.GetInstanceID());

        UnityEditor.AssetDatabase.RenameAsset(assetPath, this.editingMapData.FileName);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }

    public void HandleCopyLevelButtonPress()
    {
        var levelDataCopy = ScriptableObject.CreateInstance<MapData>();

        this.allMapDatas.Add(levelDataCopy);//this will not persist when editor stops

        var levelNum = this.allMapDatas.Count < 10 ? $"0{this.allMapDatas.Count}" : $"{this.allMapDatas.Count}";

        var fileName = $"Lv-{levelNum}_{this.editingMapData.Name}";
        levelDataCopy.Name = $"{this.editingMapData.Name} ({this.allMapDatas.Count})";
        levelDataCopy.MapNum = this.allMapDatas.Count; //this.editingMapData.MapNum;
        levelDataCopy.StartTileCoords = this.editingMapData.StartTileCoords;
        levelDataCopy.InitGrid(this.editingMapData.ArrayDims.x, this.editingMapData.ArrayDims.y);

        var numAcross = this.editingMapData.ArrayDims.x;
        var numDown = this.editingMapData.ArrayDims.y;

        for (int i = 0; i < numAcross; i++)
        {
            for (int j = 0; j < numDown; j++)
            {
                var levelTileDataObj = this.editingMapData.GetTileDataObjectAtCoords(i, j);
                levelDataCopy.InsertTileDataObjectAtCoords(levelTileDataObj, i, j);
            }
        }

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.CreateAsset(levelDataCopy, $"{this.mapDataPath}{fileName}.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
#endif

        LoadMapNum(this.allMapDatas.Count - 1);
    }

    public void HandleTileGroupButtonPress(int inGroupIndex)
    {
        Debug.Log($"HandleTileGroupButtonPress({inGroupIndex})");
        this.selectedTileGroupIndex = inGroupIndex;

        RefreshTilesList();
    }
}