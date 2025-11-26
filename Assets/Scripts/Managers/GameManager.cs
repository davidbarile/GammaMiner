using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using Sirenix.OdinInspector;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds_5 = new(.5f);
    public static GameManager IN;

    public static Action OnLevelTilesLoaded;

    [Header("Safety Setting")]
    public bool IsPublishBuild;

    [Header("Debug Settings")]
    [SerializeField] private bool shouldResetDataOnLoad;
    [SerializeField] private bool shouldShowSplashScreen;

    public bool ShouldShowLoadingScreen => this.shouldShowLoadingScreen;
    [SerializeField] private bool shouldShowLoadingScreen;

    public Transform ProjectilesContainer { get; private set; }

    [Header("Player & Ship Data")]
    [ReadOnly, SerializeField] private  PlayerData playerData;
    [SerializeField] private PlayerData defaultPlayerData;
    
    [Header("MapNum & Progress Data")]
    [ReadOnly, SerializeField] private  MapProgressData mapProgressData;
    [SerializeField] private MapProgressData defaultMapProgressData;

    [Header("Ship")]
    [SerializeField] private SpaceShip playerShip;
    [SerializeField] private ShipConfig defaultShipConfig;

    [Header("Revive Rocks: 1 day = 86400, 1 hour = 3600 seconds, 5 minutes = 300 seconds")]
    public float SecondsBeforeRockRevive = 15;

    [Range(10, 300)][SerializeField] private int targetFrameRate;

    [Header("Camera")]
    [SerializeField] private CinemachineCamera cinemachineCamera;

    [Header("Singletons")]
    [SerializeField] private HUD hud;
    [SerializeField] private UI ui;
    [SerializeField] private UiLoadingScreen loadingScreen;
    [SerializeField] private UiSplashScreen splashScreen;
    [SerializeField] private Cockpit cockpit;
    [SerializeField] private MiniMap miniMap;
    [SerializeField] private UIShopPanel shopPanel;
    [SerializeField] private UiSettingsPanel settingsPanel;
    [SerializeField] private UiDebugPanel debugPanel;
    [SerializeField] private UIConfirmPanel confirmPanel;
    [SerializeField] private KaleidoscopeController kaleidoscopeController;
    [Space, SerializeField] private Pool pool;

    private void Awake()
    {
        DOTween.Init();

        DontDestroyOnLoad(this.gameObject);

        CreateSingletons();

        if (this.IsPublishBuild)
        {
            this.shouldShowSplashScreen = true;
            this.shouldResetDataOnLoad = false;
            LootManager.IN.DebugLootSpriteAlpha = 0f;
            LootManager.IN.DebugShowHealthText = false;
        }

#if !UNITY_EDITOR
        this.shouldShowSplashScreen = true;
        this.shouldResetDataOnLoad = false;
#endif

        InputManager.IsInputBlocked = true;

        GameManager.OnLevelTilesLoaded += OnLoadLevelTilesComplete;

        var go = new GameObject("Projectiles Container");
        DontDestroyOnLoad(go);
        this.ProjectilesContainer = go.transform;

        this.ui.gameObject.SetActive(true);

        if(this.shouldShowLoadingScreen)
            this.loadingScreen.Show();

        this.splashScreen.SetPlayButtonVisibility(false);

        if (this.shouldShowSplashScreen)
            this.splashScreen.Show();
        else
            this.splashScreen.Hide();

        this.shopPanel.Hide();
        this.debugPanel.Hide();
        this.settingsPanel.Hide();
        this.confirmPanel.Hide();
    }

    private void CreateSingletons()
    {
        GameManager.IN = this;

        UI.IN = this.ui;
        HUD.IN = this.hud;
        Pool.IN = this.pool;
        Cockpit.IN = this.cockpit;
        MiniMap.IN = this.miniMap;
        UiLoadingScreen.IN = this.loadingScreen;
        UiSplashScreen.IN = this.splashScreen;
        UIShopPanel.IN = this.shopPanel;
        UiSettingsPanel.IN = this.settingsPanel;
        UiDebugPanel.IN = this.debugPanel;
        UIConfirmPanel.IN = this.confirmPanel;

        DeviceManager.IN = this.GetComponent<DeviceManager>();
        MapManager.IN = this.GetComponent<MapManager>();
        TileLoadingManager.IN = this.GetComponent<TileLoadingManager>();
        NavMeshManager.IN = this.GetComponent<NavMeshManager>();
        KaleidoscopeController.IN = this.kaleidoscopeController;
        InputManager.IN = this.GetComponent<InputManager>();
        LootManager.IN = this.GetComponent<LootManager>();
        ProgressManager.IN = this.GetComponent<ProgressManager>();
        PlanetManager.IN = this.GetComponent<PlanetManager>();
        GlobalData.IN = this.GetComponent<GlobalData>();
        AudioManager.IN = this.GetComponent<AudioManager>();
        TickManager.IN = this.GetComponent<TickManager>();
        DebugManager.IN = this.GetComponent<DebugManager>();
        TooltipManager.IN = this.GetComponent<TooltipManager>();
    }

    public void DeleteCurrentLevelProgress()
    {
        if (PlayerData.Data.CurrentLevelIndex < MapProgressData.Data.MapSaveDatas.Count)
        {
            MapProgressData.Data.MapSaveDatas[PlayerData.Data.CurrentLevelIndex] = new MapSaveData()
            {
                MapNum = PlayerData.Data.CurrentLevelIndex
            };
        }
    }
    
    public void DeleteAllLevelProgress()
    {
        PlayerData.Data.CurrentLevelIndex = 0;
        MapProgressData.Data.MapSaveDatas.Clear();
    }

    [PropertySpace(10), Button(ButtonSizes.Large), GUIColor(1, 0, 0)]
    public void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    private IEnumerator Start()
    {
#if UNITY_EDITOR
        if (this.shouldResetDataOnLoad)
            DeletePlayerPrefs();
#endif

        this.defaultPlayerData.ShipData = ShipData.GetDataFromConfig(this.defaultShipConfig);

        var playerDataExists = PlayerPrefsManager.PlayerDataExists;

        if (playerDataExists)
        {
            PlayerPrefsManager.GetPlayerData();
            PlayerData.Data.SetSettings();
            PlayerPrefsManager.GetMapProgressData();
        }
        else
        {
            PlayerData.Data = this.defaultPlayerData;
            PlayerData.Data.SetStartDate();
            PlayerData.Data.InitOwnedItems(this.defaultShipConfig);

            //force to randomize
            if (this.IsPublishBuild)
                PlayerData.Data.RandomSeed = -1;

            MapProgressData.Data = this.defaultMapProgressData;
        }

        if (PlayerData.Data.RandomSeed == -1)
            PlayerData.Data.RandomSeed = UnityEngine.Random.Range(0, 1000);

        //to see serialized in Inspector
        this.playerData = PlayerData.Data;
        this.mapProgressData = MapProgressData.Data;

        AudioManager.IN.Init();

        this.playerShip.gameObject.SetActive(false);

        SpaceShip.PlayerShip = this.playerShip;
        ReplacePlayerShipPrefab(PlayerData.Data.ShipData.PrefabName);

        SerializePlayerShipToCinemachineCamera();

        yield return null;

        Application.targetFrameRate = this.targetFrameRate;

        PlanetManager.IN.Init();

        SpaceShip.PlayerShip.Init(PlayerData.Data.ShipData, true);

        print($"PlayerData.Data.CurrentMapLevel = {PlayerData.Data.CurrentLevelIndex}");

        MapManager.IN.LoadMapNum(PlayerData.Data.CurrentLevelIndex);
    }

    private void OnLoadLevelTilesComplete()
    {
        UiSplashScreen.IN.SetLoadingText(string.Empty);
        UiSplashScreen.IN.SetPlayButtonVisibility(true);

        if (UiSplashScreen.IN.IsShowing)
            UiLoadingScreen.IN.Hide();
        else
        {
            if(this.shouldShowLoadingScreen)
                UiLoadingScreen.IN.DelayedHide();
                
            EnableInput();
        }
    }

    //called from UiSplashScreen play button
    public void StartGame()
    {
        UiSplashScreen.IN.Hide();
        Invoke(nameof(EnableInput), .5f);
    }

    private void EnableInput()
    {
        InputManager.IsInputBlocked = false;
    }

    public static void ReplacePlayerShipPrefab(string inPrefabName)
    {
        var oldshipParent = SpaceShip.PlayerShip.transform.parent;
        SpaceShip.PlayerShip.transform.GetPositionAndRotation(out var oldShipPos, out var oldShipRot);
        var oldShipScale = SpaceShip.PlayerShip.transform.localScale;

        HUD.IN.UnregisterEvents();
        Destroy(SpaceShip.PlayerShip.gameObject);

        var newShipPrefabGo = Resources.Load($"Ships/{inPrefabName}") as GameObject;
        var go = Instantiate(newShipPrefabGo, oldshipParent);

        SpaceShip.PlayerShip = go.GetComponent<SpaceShip>();

        SpaceShip.PlayerShip.transform.position = oldShipPos;
        SpaceShip.PlayerShip.transform.rotation = oldShipRot;
        SpaceShip.PlayerShip.transform.localScale = oldShipScale;

        GameManager.IN.SerializePlayerShipToCinemachineCamera();
    }

    public void SpawnShip(SpaceShip inSpaceShipPrefab)
    {
        var go = Instantiate(inSpaceShipPrefab.gameObject);
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;

        go.transform.SetAsFirstSibling();

        SpaceShip.PlayerShip = go.GetComponent<SpaceShip>();
        SpaceShip.PlayerShip.Init(PlayerData.Data.ShipData, true);
    }

    public PlayerData GetDefaultPlayerData()
    {
        return this.defaultPlayerData;
    }

    public MapProgressData GetDefaultMapProgressData()
    {
        return this.defaultMapProgressData;
    }

    public static void PauseGame(bool inIsPaused)
    {
        Time.timeScale = inIsPaused ? 0 : 1;
    }

    public void OnPlayerDie(HealthEntity inHealthEntity = null)
    {
        StartCoroutine(PlayerDieRebirthCo());
    }

    private IEnumerator PlayerDieRebirthCo()
    {
        yield return new WaitForSeconds(2);

        //TODO: show some You're Dead, star log data display

        MapManager.IN.ReloadMap();

        yield return _waitForSeconds_5;

        SpaceShip.PlayerShip.Reset();
    }

    public void SerializePlayerShipToCinemachineCamera()
    {
        this.cinemachineCamera.Follow = SpaceShip.PlayerShip.transform;
    }

    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnApplicationQuit()
    {
        PlayerPrefsManager.SavePlayerData();
        PlayerPrefsManager.SaveMapProgressData();
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    private void OnApplicationPause(bool inIsPaused)
    {
        if (inIsPaused)
        {
            PlayerPrefsManager.SavePlayerData();
            PlayerPrefsManager.SaveMapProgressData();
        }
    }

    private void OnDestroy()
    {
        GameManager.OnLevelTilesLoaded -= OnLoadLevelTilesComplete;
    }
}