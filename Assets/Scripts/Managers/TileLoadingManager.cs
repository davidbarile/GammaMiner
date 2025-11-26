using System;
using System.Collections;
using System.Collections.Generic;
using Rocks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileLoadingManager : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds2 = new(2f);
    public static TileLoadingManager IN;
    public static Vector3 PlayerPosition;
    public static Vector2Int PlayerCoords;

    public Vector3 PlayerPositionDisplay;
    public Vector2Int PlayerCoordsDisplay;

    [SerializeField, ReadOnly] private MapData currentMapData;

    public Tile[,] TileMatrix = new Tile[20, 20];

    [SerializeField] private Vector2 tileDimensions;//38.365

    [SerializeField] private TextMeshProUGUI loadTimeText;

    [SerializeField] private float tileScaleModifier = 2f;

    [Space]
    public Transform WormHoleContainer;

    [Space]
    [ShowInInspector, ReadOnly]
    private Dictionary<string, List<Tile>> loadedTilesDict = new();

    [ShowInInspector, ReadOnly]
    private Dictionary<string, Tile> loadedTileScenesDict = new();

    private DateTime startLoadTime;

    private string loadedTilesDisplay;
    private int loadingTilesCount = 0;

    private Transform playerTransform;

    private Vector3 gridPlayerOffset = Vector3.zero;

    private GameObject tilePrefabsContainer;
    private GameObject placedTilesContainer;

    private List<Tile> allTilesInLevel = new();

    public List<Rock> AllRocksInLevel => this.allRocksInLevel;
    private List<Rock> allRocksInLevel = new();
    private List<RockCluster> allRockClustersInLevel = new();

    public List<Rock> AllActiveRocksInLevel => this.allActiveRocksInLevel;
    private List<Rock> allActiveRocksInLevel = new();

    private void Awake()
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;
    }

    private void OnEnable()
    {
        TickManager.OnSecondTick += SecondTickHandler;
    }

    private void OnDisable()
    {
        TickManager.OnSecondTick -= SecondTickHandler;
    }

    public void LoadMap(MapData inData)
    {
        this.currentMapData = inData;

        UnloadPreviousLevel();

        this.loadingTilesCount = 0;

        this.gridPlayerOffset = SpaceShip.PlayerShip.transform.position;

        StartCoroutine(DelayedLoadScenes());
    }

    private void UnloadPreviousLevel()
    {
        var shouldGarbageCollect = false;

        if (this.placedTilesContainer)
        {
            Destroy(this.placedTilesContainer);
            shouldGarbageCollect = true;
        }

        if (this.tilePrefabsContainer)
        {
            Destroy(this.tilePrefabsContainer);
            shouldGarbageCollect = true;
        }

        if (shouldGarbageCollect)
            GC.Collect();

        this.placedTilesContainer = new GameObject($"L{this.currentMapData.MapNum} Tiles Container");

        this.placedTilesContainer.transform.SetParent(NavMeshManager.IN.NavSurface.transform);
        this.placedTilesContainer.transform.rotation = Quaternion.identity;

        this.tilePrefabsContainer = new GameObject($"L{this.currentMapData.MapNum} Tile Prefabs Container");
        this.tilePrefabsContainer.SetActive(false);

        this.loadedTilesDict.Clear();
        this.loadedTileScenesDict.Clear();
        this.allTilesInLevel.Clear();
        this.allRocksInLevel.Clear();
        this.allRockClustersInLevel.Clear();
    }

    public void UpdatePlayerCoords(Transform inPlayer, bool inForceLoad = false)
    {
        this.playerTransform = inPlayer;

        PlayerPosition = inPlayer.position;
        this.PlayerPositionDisplay = inPlayer.position;

        var tileSize = this.tileDimensions * this.tileScaleModifier;
        int xCoord = Mathf.RoundToInt((inPlayer.position.x - this.gridPlayerOffset.x) / tileSize.x);
        int yCoord = Mathf.RoundToInt((inPlayer.position.y - this.gridPlayerOffset.y) / tileSize.y);

        if (this.currentMapData != null)
        {
            xCoord += this.currentMapData.StartTileCoords.x;
            yCoord += this.currentMapData.StartTileCoords.y;
        }

        if (xCoord != PlayerCoords.x || yCoord != PlayerCoords.y || inForceLoad)
            StartCoroutine(LoadAdjacentTiles(xCoord, yCoord));

        PlayerCoords = new Vector2Int(xCoord, yCoord);
        this.PlayerCoordsDisplay = PlayerCoords;
    }

    private IEnumerator LoadAdjacentTiles(int inX, int inY)
    {
        //Debug.Log($"LoadAdjacentTiles({inX}, {inY})");

        if (this.loadedTilesDict.Count == 0) yield break;

        bool isSafeLeft = inX > 0;
        bool isSafeRight = inX < this.TileMatrix.GetLength(0) - 1;
        bool isSafeBottom = inY > 0;
        bool isSafeTop = inY < this.TileMatrix.GetLength(1) - 1;

        bool success = TryLoadTileAtCoords(inX, inY);
        if (success) yield return null;

        if (isSafeLeft)
        {
            success = TryLoadTileAtCoords(inX - 1, inY);
            if (success) yield return null;

            if (isSafeBottom)
            {
                success = TryLoadTileAtCoords(inX - 1, inY - 1);
                if (success) yield return null;
            }

            if (isSafeTop)
            {
                success = TryLoadTileAtCoords(inX - 1, inY + 1);
                if (success) yield return null;
            }
        }

        if (isSafeRight)
        {
            success = TryLoadTileAtCoords(inX + 1, inY);
            if (success) yield return null;

            if (isSafeBottom)
            {
                success = TryLoadTileAtCoords(inX + 1, inY - 1);
                if (success) yield return null;
            }

            if (isSafeTop)
            {
                success = TryLoadTileAtCoords(inX + 1, inY + 1);
                if (success) yield return null;
            }
        }

        if (isSafeBottom)
        {
            success = TryLoadTileAtCoords(inX, inY - 1);
            if (success) yield return null;
        }

        if (isSafeTop)
        {
            success = TryLoadTileAtCoords(inX, inY + 1);
            if (success) yield return null;
        }

        StartCoroutine(HideAdjacentTiles(inX, inY));
    }

    private IEnumerator HideAdjacentTiles(int inX, int inY)
    {
        //Debug.Log($"HideAdjacentTiles({inX}, {inY})");

        bool isSafeLeft1 = inX > 0;
        bool isSafeRight1 = inX < this.TileMatrix.GetLength(0) - 1;
        bool isSafeBottom1 = inY > 0;
        bool isSafeTop1 = inY < this.TileMatrix.GetLength(1) - 1;

        bool isSafeLeft2 = inX > 1;
        bool isSafeRight2 = inX < this.TileMatrix.GetLength(0) - 2;
        bool isSafeBottom2 = inY > 1;
        bool isSafeTop2 = inY < this.TileMatrix.GetLength(1) - 2;

        bool success = false;

        if (isSafeLeft2)
        {
            success = HideTileAtCoords(inX - 2, inY);
            if (success) yield return null;

            if (isSafeBottom1)
            {
                success = HideTileAtCoords(inX - 2, inY - 1);
                if (success) yield return null;
            }

            if (isSafeTop1)
            {
                success = HideTileAtCoords(inX - 2, inY + 1);
                if (success) yield return null;
            }

            if (isSafeBottom2)
            {
                success = HideTileAtCoords(inX - 2, inY - 2);
                if (success) yield return null;
            }

            if (isSafeTop2)
            {
                success = HideTileAtCoords(inX - 2, inY + 2);
                if (success) yield return null;
            }
        }

        if (isSafeRight2)
        {
            success = HideTileAtCoords(inX + 2, inY);
            if (success) yield return null;

            if (isSafeBottom1)
            {
                success = HideTileAtCoords(inX + 2, inY - 1);
                if (success) yield return null;
            }

            if (isSafeTop1)
            {
                success = HideTileAtCoords(inX + 2, inY + 1);
                if (success) yield return null;
            }

            if (isSafeBottom2)
            {
                success = HideTileAtCoords(inX + 2, inY - 2);
                if (success) yield return null;
            }

            if (isSafeTop2)
            {
                success = HideTileAtCoords(inX + 2, inY + 2);
                if (success) yield return null;
            }
        }

        //one space away

        if (isSafeBottom2)
        {
            success = HideTileAtCoords(inX, inY - 2);
            if (success) yield return null;

            if (isSafeLeft1)
            {
                success = HideTileAtCoords(inX - 1, inY - 2);
                if (success) yield return null;
            }

            if (isSafeRight1)
            {
                success = HideTileAtCoords(inX + 1, inY - 2);
                if (success) yield return null;
            }
        }

        if (isSafeTop2)
        {
            success = HideTileAtCoords(inX, inY + 2);
            if (success) yield return null;

            if (isSafeLeft1)
            {
                success = HideTileAtCoords(inX - 1, inY + 2);
                if (success) yield return null;
            }

            if (isSafeRight1)
            {
                success = HideTileAtCoords(inX + 1, inY + 2);
                if (success) yield return null;
            }
        }

        NavMeshManager.IN.RebuildNavMesh();
    }

    private bool TryLoadTileAtCoords(int inX, int inY)
    {
        var levelTileDataObj = this.currentMapData.GetTileDataObjectAtCoords(inX, inY);

        if (levelTileDataObj == null || levelTileDataObj.TileData == null)
        {
            return true;//maybe load empty tile here
        }

        var tile = this.TileMatrix[inX, inY];

        if (tile != null)
        {
            if (tile.IsShowing)
            {
                return false;
            }
            else
            {
                tile.Show();
                return true;
            }
        }

        float xOffset = 0;// this.TileMatrix.GetLength(0) * .5f;
        float yOffset = 0;// this.TileMatrix.GetLength(1) * .5f;

        var tilePos = new Vector3((inX - xOffset) * this.tileDimensions.x * this.tileScaleModifier, (inY - yOffset) * this.tileDimensions.y * this.tileScaleModifier, 0);
        tilePos += this.gridPlayerOffset;

        var startTileOffset = Vector3.zero;
        startTileOffset.x = this.currentMapData.StartTileCoords.x < 0 ? 0 : this.currentMapData.StartTileCoords.x;
        startTileOffset.y = this.currentMapData.StartTileCoords.y < 0 ? 0 : this.currentMapData.StartTileCoords.y;
        startTileOffset *= this.tileDimensions;
        startTileOffset *= this.tileScaleModifier;

        tilePos -= startTileOffset;

        //Debug.Log($"TryLoadTileAtCoords({inX}, {inY})   tilePos = {tilePos}");

        var key = levelTileDataObj.TileData.TileSceneName;

        //load from Dictionary with Key tileSceneName and list of tiles as Value

        var tilesList = new List<Tile>();

        if (this.loadedTilesDict.ContainsKey(key))
        {
            tilesList = this.loadedTilesDict[key];

            if (tilesList.Count > 0)
            {
                tile = tilesList[0];

                tile.transform.position = tilePos;
                tile.transform.SetParent(this.placedTilesContainer.transform);

                this.TileMatrix[inX, inY] = tile;

                tile.transform.localScale = new Vector3(this.tileScaleModifier, this.tileScaleModifier, 1);
                var rotation = Mathf.Repeat(levelTileDataObj.Rotation, 360f);
                tile.transform.rotation = Quaternion.Euler(0, 0, rotation);
                tile.gameObject.name = $"Tile [{inX},{inY}]";
                tile.Show();

                tilesList.RemoveAt(0);

                return true;
            }
            else
            {
                Debug.Log($"<color=red>TryLoadTileAtCoords({inX}, {inY}) loadedTilesDict[{key}].Count = 0!</color>");
                return false;
            }
        }
        else
        {
            Debug.Log($"<color=red>TryLoadTileAtCoords({inX}, {inY}) loadedTilesDict doesn't contain key \"{key}\"</color>");
            return false;
        }
    }

    private bool HideTileAtCoords(int inX, int inY)
    {
        if (inX < 0 || inX >= this.TileMatrix.GetLength(0) - 1)
        {
            Debug.Log($"<color=red>MapData.GetTileAtCoords({inX}, {inY}) out of range of TileMatrix.width ({this.TileMatrix.GetLength(0)})</color>");
            return false;
        }

        if (inY < 0 || inY >= this.TileMatrix.GetLength(1) - 1)
        {
            Debug.Log($"<color=red>MapData.GetTileAtCoords({inX}, {inY}) out of range of TileMatrix.height ({this.TileMatrix.GetLength(1)})</color>");
            return false;
        }

        var tile = this.TileMatrix[inX, inY];

        if (tile != null)
        {
            tile.Hide();
            return true;
        }

        return false;
    }

    // private IEnumerator DelayedLoad(string inSceneName)
    // {
    //     ProtoShape2D.IsPreBuilt = true;

    //     this.startLoadTime = DateTime.Now;

    //     this.loadTimeText.text = "0";

    //     yield return null;

    //     SceneManager.LoadSceneAsync(inSceneName, LoadSceneMode.Additive);

    //     var loadTimeSpan = DateTime.Now.Subtract(this.startLoadTime);

    //     this.loadTimeText.text = $"{loadTimeSpan.TotalMilliseconds:N2}";

    //     yield break;
    // }

    public void HandleButtonPress2()
    {
        if (this.loadingTilesCount > 0)
            return;

        StartCoroutine(DelayedLoadScenes());
    }

    private IEnumerator DelayedLoadScenes()
    {
        UiLoadingScreen.IN.Show($"Loading Map {this.currentMapData.MapNum}: <i>{this.currentMapData.Name}</i>\n{this.currentMapData.NumTilesInMap} Tiles");
        UiSplashScreen.IN.SetLoadingText($"Loading Map {this.currentMapData.MapNum}: <i>{this.currentMapData.Name}</i>");

        this.loadedTilesDisplay = "Loaded Tiles:";

        this.startLoadTime = DateTime.Now;

        this.loadTimeText.text = "0";

        yield return null;

        var numAcross = this.currentMapData.ArrayDims.x;
        var numDown = this.currentMapData.ArrayDims.y;

        for (int i = 0; i < numAcross; i++)
        {
            for (int j = 0; j < numDown; j++)
            {
                var levelTileDataObj = this.currentMapData.GetTileDataObjectAtCoords(i, j);

                if (levelTileDataObj != null && levelTileDataObj.TileData != null)
                {
                    var tileData = levelTileDataObj.TileData;
                    //if Tile Scene not already loaded, load it
                    if (!this.loadedTileScenesDict.ContainsKey(tileData.TileSceneName))
                    {
                        var loadOperation = SceneManager.LoadSceneAsync(tileData.TileSceneName, LoadSceneMode.Additive);

                        loadOperation.completed += op =>
                        {
                            Scene newScene = SceneManager.GetSceneByName(tileData.TileSceneName);
                            if (newScene.IsValid())
                            {
                                foreach (GameObject rootObj in newScene.GetRootGameObjects())
                                {
                                    if (rootObj.TryGetComponent<Tile>(out var tile))
                                    {
                                        this.loadedTileScenesDict.Add(tileData.TileSceneName, tile);
                                        RegisterLoadedTile(tile, levelTileDataObj);
                                    }
                                }

                                AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(newScene);
                            }
                        };

                        while (!loadOperation.isDone)
                        {
                            yield return null;
                        }
                    }
                    else
                    {
                        var tileToDuplicate = this.loadedTileScenesDict[tileData.TileSceneName];
                        var newTile = Instantiate(tileToDuplicate);
                        newTile.name = tileData.TileSceneName;
                        RegisterLoadedTile(newTile, levelTileDataObj);
                    }
                }
            }
        }

        UpdatePlayerCoords(this.playerTransform, true);

        var loadTimeSpan = DateTime.Now.Subtract(this.startLoadTime);

        this.loadTimeText.text = $"{loadTimeSpan.TotalMilliseconds:N2}";

        UiLoadingScreen.IN.Show($"Loading Map {this.currentMapData.MapNum}: <i>{this.currentMapData.Name}</i>\n{this.currentMapData.NumTilesInMap} Tiles\n{loadTimeSpan.TotalMilliseconds:N2} ms");
        UiSplashScreen.IN.SetLoadingText($"Loading Map {this.currentMapData.MapNum}: <i>{this.currentMapData.Name}</i>");

        this.startLoadTime = DateTime.Now;

        RefreshLevelToSavedState();

        yield return null;

        GameManager.OnLevelTilesLoaded?.Invoke();

        yield break;
    }

    private void RegisterLoadedTile(Tile inTile, MapTileDataObject inMapTileDataObj)
    {
        var key = inTile.name;
        var tilesList = new List<Tile>();

        if (this.loadedTilesDict.ContainsKey(key))
            tilesList = this.loadedTilesDict[key];
        else
            this.loadedTilesDict.Add(key, tilesList);

        tilesList.Add(inTile);
        inTile.name = $"{key} ({tilesList.Count})";
        inTile.transform.SetParent(this.tilePrefabsContainer.transform);
        var rotation = Mathf.Repeat(inMapTileDataObj.Rotation, 360f);
        inTile.transform.rotation = Quaternion.Euler(0, 0, rotation);

        this.allTilesInLevel.Add(inTile);

        ++this.loadingTilesCount;

        var loadTimeSpan = DateTime.Now.Subtract(this.startLoadTime);
        this.loadedTilesDisplay += $"\n{key}  {loadTimeSpan.TotalMilliseconds:N2} ms";

        UiLoadingScreen.IN.SetLoadedTilesText(this.loadedTilesDisplay);

        UiLoadingScreen.IN.Show($"Loading Map {this.currentMapData.MapNum}: <i>{this.currentMapData.Name}</i>\n{this.loadingTilesCount}/{this.currentMapData.NumTilesInMap} Tiles Loaded\n{loadTimeSpan.TotalMilliseconds:N2} ms");
        UiSplashScreen.IN.SetLoadingText($"Loading Map {this.currentMapData.MapNum}: <i>{this.currentMapData.Name}</i>    {this.loadingTilesCount}/{this.currentMapData.NumTilesInMap} Tiles");
    }

    private void RefreshLevelToSavedState()
    {
        foreach (var tile in this.allTilesInLevel)
        {
            tile.Init();
            this.allRocksInLevel.AddRange(tile.AllRocks);
            this.allRockClustersInLevel.AddRange(tile.AllRockClusters);
        }

        //set unique IDs for rocks
        //this is done here because GetInstanceID() is not stable across sessions
        for (int i = 0; i < this.allRocksInLevel.Count; i++)
        {
            this.allRocksInLevel[i].ID = $"Rock_{i}";
        }

        this.allActiveRocksInLevel.Clear();

        var isLevelDirty = MapProgressData.Data.CurrentMapSaveData.DirtyRocks.Count > 0;

        foreach (var rockCluster in this.allRockClustersInLevel)
        {
            rockCluster.InitRockHealths();
        }

        if (!isLevelDirty && PlayerData.Data.CurrentLevelIndex < MapProgressData.Data.MapSaveDatas.Count)
            LootManager.IN.InitRockLootDataFromProgressData(ProgressManager.IN.ProgressDatas[PlayerData.Data.CurrentLevelIndex]);//TODO: this should not be tied to level

        foreach (var rock in this.allRocksInLevel)
        {
            if (MapProgressData.Data.CurrentMapSaveData.DirtyRocks.ContainsKey(rock.ID))
            {
                var rockSaveData = MapProgressData.Data.CurrentMapSaveData.DirtyRocks[rock.ID];
                rock.ApplySaveData(rockSaveData);

                if (this.allActiveRocksInLevel.Contains(rock))
                    Debug.Log($"<color=red>allActiveRocksInLevel Already contains {rock.name}!!!</color>");

                if (rock.gameObject.activeSelf)
                    this.allActiveRocksInLevel.Add(rock);

                rock.HealthEntity.OnDie += RemoveRockFromActiveList;
            }
        }

        LootManager.IN.SetLevelLootDataFromRocks();

        //Debug.Log($"allRocksInLevel.Count = {this.allRocksInLevel.Count}    allActiveRocksInLevel = {allActiveRocksInLevel.Count}    PlayerData.Data.CurrentMapLevel = {PlayerData.Data.CurrentLevelIndex}    MapProgressData.Data.MapSaveDatas.Count = {MapProgressData.Data.MapSaveDatas.Count}  PlayerProgressData.Data.CurrentMapSaveData.DirtyRocks = {MapProgressData.Data.CurrentMapSaveData.DirtyRocks.Count}");
    }

    //called every second
    private void SecondTickHandler()
    {
        ReviveAllRocks(false);
    }

    public void DelayedReviveAllRocks()
    {
        StartCoroutine(DelayedReviveAllRocksCoroutine());
    }

    private IEnumerator DelayedReviveAllRocksCoroutine()
    {
        yield return _waitForSeconds2;
        ReviveAllRocks(true);
    }
    
    public void ReviveAllRocks(bool inForceAll)
    {
        RockSaveData saveData = null;

        var didAtLeastOneRevive = false;

        foreach (var rock in this.allRocksInLevel)
        {
            if (rock.gameObject.activeSelf)
                continue;

            if (MapProgressData.Data.CurrentMapSaveData.DirtyRocks.TryGetValue(rock.ID, out saveData))
            {
                if (saveData.TimeMined <= -1)
                    continue;

                var shouldRevive = inForceAll || Utils.NOW - saveData.TimeMined >= GameManager.IN.SecondsBeforeRockRevive;

                if (shouldRevive)
                {
                    var isOnscreen = true;//TODO: CameraManager.IN.IsObjectInView(rock.gameObject);

                    if (isOnscreen || inForceAll)
                        didAtLeastOneRevive = true;

                    HandleRockRevive(rock);
                    rock.Revive();
                }
                else if (!inForceAll)
                {
                    Debug.Log($"Not reviving rock {rock.name} yet.  Time since mined: {Utils.NOW - saveData.TimeMined} seconds");
                }
            }
        }

        if (didAtLeastOneRevive)
            NavMeshManager.IN.RebuildNavMesh();
    }

    private void RemoveRockFromActiveList(HealthEntity inHealthEntity)
    {
        if (inHealthEntity.TryGetComponent<Rock>(out var rock))
        {
            if (this.allActiveRocksInLevel.Contains(rock))
                this.allActiveRocksInLevel.Remove(rock);
        }
    }   

    private void HandleRockRevive(Rock inRock)
    {
        if (!this.allActiveRocksInLevel.Contains(inRock))
            this.allActiveRocksInLevel.Add(inRock);
    }
}