using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;

[ExecuteInEditMode]
public class TileEditorTool : MonoBehaviour
{
    public static TileEditorTool IN = null;
    public static bool IsEditing => IN != null;

    public DrawingToolData DrawingData;

    public enum EDrawMode
    {
        Off,
        Draw,
        Color,
        Preset
    }

    // [PropertySpace(10), ShowInInspector, HideLabel, DisplayAsString(EnableRichText = true, FontSize = 13, Overflow = false)]
    // private string keyPressesNote07 = "<color=cyan>Hold <b>Shift</b> to Erase.\nPress <b>0-9</b> on keyboard to select Presets.\n<b>Esc</b> = Off, <b>D</b> = Draw, <b>C</b> = Color</color>";

    [Space(5)]
    [EnumToggleButtons, GUIColor(0, 1, 1)]
    public EDrawMode DrawMode = EDrawMode.Preset;

    [Space(5)]
    public bool ShouldDeleteHiddenRocks;

    [Space(5)]
    public Color TextureColor = Color.red;

    private bool isShiftPressed;
    private bool isOptionPressed;
    private bool isAltPressed;

    private RockCluster currentRockCluster;

    private string mapTilesDataPath = "Assets/Data/Map Tiles/";

    private MapTileData newMapTileData;

    private bool isSceneDirty;

    public static void SetDirty(Object inObject)
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(inObject);
        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(inObject);//praise Jesus!
#endif
    }

    [Title("Do Not Use These buttons in Play Mode", TitleAlignment = TitleAlignments.Centered)]
    [Button(ButtonSizes.Large), GUIColor(1, 0, 0), PropertyOrder(10), PropertySpace(11)]
    private void DeleteHiddenRocks()
    {
        if (this.DrawingData != null)
        {
            if (this.rocksInScene.Length == 0)
                this.rocksInScene = Object.FindObjectsByType<Rock>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var rock in this.rocksInScene)
            {
                var id = rock.GetInstanceID();

                if (this.DrawingData.RocksToModify.ContainsKey(id))
                {
                    var data = this.DrawingData.RocksToModify[id];

                    if (!data.IsVisible)
                    {
                        SpawnDeleteMeContainer();
                        rock.transform.SetParent(RockClusterEditor.DeleteMe.transform);
                    }
                }
            }

            this.rocksInScene = new Rock[0];
        }

        var rockClusters = FindObjectsByType<RockCluster>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var cluster in rockClusters)
        {
            cluster.RefreshRocksArray();
        }
    }

    [Button(ButtonSizes.Large), GUIColor(1, 0, 0), PropertyOrder(11), PropertySpace(10)]
    private void GatherAndDeleteHiddenRocks()
    {
        var allRocksInScene = Object.FindObjectsByType<Rock>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        RockClusterEditor.DeleteMe = null;

        foreach (var rock in allRocksInScene)
        {
            if (!rock.gameObject.activeSelf && rock.transform.parent.gameObject.activeSelf)
            {
                SpawnDeleteMeContainer();
                rock.transform.SetParent(RockClusterEditor.DeleteMe.transform);
            }
        }

        this.rocksInScene = new Rock[0];

        var rockClusters = FindObjectsByType<RockCluster>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var cluster in rockClusters)
        {
            cluster.RefreshRocksArray();
        }
    }

    [Button(ButtonSizes.Large), GUIColor(1, 1, .3f), PropertyOrder(11), PropertySpace(10)]
    private void RenameAllRocks()
    {
        var rockClusters = FindObjectsByType<RockCluster>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        var allRocksInScene = new List<Rock>();

        foreach (var cluster in rockClusters)
        {
            cluster.RefreshRocksArray(true);
            foreach (var rock in cluster.Rocks)
            {
                allRocksInScene.Add(rock);
            }
        }

        this.rocksInScene = allRocksInScene.ToArray();
    }

    [Button(ButtonSizes.Large), GUIColor(0, 1f, .3f), PropertyOrder(11), PropertySpace(10)]
    private void ExportLevel()
    {
#if UNITY_EDITOR

        if (Application.isPlaying)
        {
            Debug.Log("<color=red>ExportLevel() - Cannot export level while in Play Mode.</color>");
            return;
        }

        var screenshotTool = this.GetComponent<SR_RenderCamera>();
        screenshotTool.CaptureImage();

        var scene = SceneManager.GetActiveScene();
        this.newMapTileData = ScriptableObject.CreateInstance<MapTileData>();
        this.newMapTileData.TileSceneName = $"{scene.name}";

        var newThumbnailFilePath = $"{screenshotTool.ScreenshotPath}";

        var reader = new StreamReader(newThumbnailFilePath);
        var data = reader.ReadToEnd();
        reader.Close();

        // byte[] imageData = File.ReadAllBytes(newThumbnailFilePath);
        // Texture2D myTexture = new Texture2D(256, 256); // Create a new texture
        // ImageConversion.LoadImage(myTexture, imageData); // Load image data into the texture

        // Rect rect = new Rect(0, 0, myTexture.width, myTexture.height);
        // Vector2 pivot = new Vector2(0.5f, 0.5f); // Center pivot
        // Sprite newSprite = Sprite.Create(myTexture, rect, pivot);

        // newTileData.TileThumbnail = newSprite;

        UnityEditor.EditorUtility.SetDirty(this.newMapTileData);

        UnityEditor.AssetDatabase.CreateAsset(this.newMapTileData, $"{this.mapTilesDataPath}{scene.name}.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();

        //LoadAssetInEditor(newThumbnailFilePath);

        Debug.Log($"ExportLevel() - Screenshot Captured for Scene: {scene.name}");
        
#endif
    }

//     public void LoadAssetInEditor(string assetAddress)
//     {
// #if UNITY_EDITOR
//         var opHandle = Addressables.LoadAssetAsync<Texture2D>(assetAddress);

//         opHandle.Completed += handle =>
//         {
//             if (handle.Status == AsyncOperationStatus.Succeeded)
//             {
//                 this.loadedThumbnailTexture = handle.Result;
//                 this.newMapTileData.ThumbnailTexture = this.loadedThumbnailTexture;
//                 UnityEditor.EditorUtility.SetDirty(this.newMapTileData);
//             }
//             else
//             {
//                 Debug.LogError($"Failed to load Addressable asset: {assetAddress}");
//             }
//             Addressables.Release(handle); // Release the handle when done
//         };
// #endif
//     }

    [PropertyOrder(13), PropertySpace(10)]
    public RockData SelectedPreset;

    [Space(15), PropertyOrder(14)]
    [SerializeField] private Rock[] rocksInScene;

    [TableList(ShowIndexLabels = true), PropertyOrder(15), PropertySpace(10)]
    public List<RockData> Presets;

    private void Awake()
    {
        //ExportLevel();

        if (!this.enabled) return;

        if (GameManager.IN != null)
        {
            IN = null;
            Destroy(this.gameObject);
            return;
        }
        else
        {
            if (IN == null) IN = this;
        }

        this.DrawingData.RocksToModify.Clear();
        this.rocksInScene = new Rock[0];

        //TODO: save in DrawToolData
        if (this.Presets.Count > 0)
            SelectRockPreset(this.Presets[0]);
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!this.isSceneDirty && TileEditorTool.IsEditing)
        {
            this.isSceneDirty = true;
            UnityEditor.EditorUtility.SetDirty(this.gameObject); // Mark the GameObject dirty to trigger scene save
        }
    }
#endif

    private void OnValidate()
    {
        if (!Application.isPlaying)
            ApplyChanges();
    }

    private void OnApplicationQuit()
    {
        if (this.DrawingData != null)
            this.DrawingData.DidApplicationQuit = true;
    }

    private void ApplyChanges()
    {
        if (this.enabled && this.DrawingData != null && this.DrawingData.DidApplicationQuit)
        {
            this.rocksInScene = Object.FindObjectsByType<Rock>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            bool didDelete = false;

            // Debug.Log($"ApplyChanges()   this.rocksInScene = {this.rocksInScene.Length}   this.DrawingData.RocksToModify = {this.DrawingData.RocksToModify.Count}");

            foreach (var rock in this.rocksInScene)
            {
                var id = rock.GetInstanceID();

                if (this.DrawingData.RocksToModify.ContainsKey(id))
                {
                    var data = this.DrawingData.RocksToModify[id];

                    if (data.RockData != null)
                    {
                        var fillColor = data.RockData.IsAnimatedTexture ? data.RockData.RockAnimColors[0] : data.RockData.FillColor;
                        rock.SetFillColor(fillColor);
                        var outlineColor = data.RockData.IsAnimatedTexture ? data.RockData.RockOutlineColors[0] : data.RockData.OutlineColor;
                        rock.SetOutlineColor(outlineColor);
                        rock.BaseRockHealthOverride = data.RockData.BaseHealth;
                        rock.ApplyRockData(data.RockData);
                    }
                    else
                    {
                        if (data.Color.a > 0)
                        {
                            rock.SetFillColor(data.Color);
                        }

                        if (data.OutlineColor.a > 0)
                        {
                            rock.SetOutlineColor(data.OutlineColor);
                        }

                        if (!rock.BaseRockHealthOverride.Equals(data.BaseRockHealthOverride))
                            rock.BaseRockHealthOverride = data.BaseRockHealthOverride;
                    }

                    //flag hidden rocks
                    rock.IsVisible = data.IsVisible;

                    if (!data.IsVisible && RockClusterEditor.ShouldDeleteHiddenRocks)
                    {
                        SpawnDeleteMeContainer();
                        rock.transform.SetParent(RockClusterEditor.DeleteMe.transform);
                        didDelete = true;
                    }
                }

                if (!rock.IsVisible)
                    rock.SetAlpha(1);

                rock.gameObject.SetActive(rock.IsVisible);

                TileEditorTool.SetDirty(rock.gameObject);
                TileEditorTool.SetDirty(rock);
            }

            if (didDelete)
                this.rocksInScene = new Rock[0];

            this.DrawingData.DidApplicationQuit = false;
        }

        this.rocksInScene = new Rock[0];
    }

    private static void SpawnDeleteMeContainer()
    {
        if (RockClusterEditor.DeleteMe == null)
        {
            RockClusterEditor.DeleteMe = new GameObject();
            RockClusterEditor.DeleteMe.name = "DELETE ME";
            RockClusterEditor.DeleteMe.SetActive(false);
        }
    }

    public static void InitRocks(RockCluster inRockCluster)
    {
        foreach (var rock in inRockCluster.Rocks)
        {
            if (rock.transform.childCount == 0)
            {
                if (rock.Fill && inRockCluster.FillColor.a > 0)
                {
                    rock.Fill.color = inRockCluster.FillColor;
                    TileEditorTool.SetDirty(rock.Fill);
                }

                if (rock.Outline && inRockCluster.OutlineColor.a > 0)
                {
                    rock.Outline.color = inRockCluster.OutlineColor;
                    TileEditorTool.SetDirty(rock.Outline);
                }
            }
        }
    }

    private List<Rock> changedRocks = new List<Rock>();

    private void Update()
    {
        HandleKeyboardInput();

        if (this.DrawMode == EDrawMode.Off || this.DrawingData == null) return;

        RockClusterEditor.ShouldDeleteHiddenRocks = this.ShouldDeleteHiddenRocks;

        if (Input.GetMouseButtonUp(0))
        {
            foreach (var rock in this.changedRocks)
            {
                rock.DrawingChangedThisFrame = false;
            }

            this.changedRocks.Clear();
            return;
        }

        if (!Input.GetMouseButton(0)) return;

        var drawingData = TileEditorTool.IN.DrawingData;

        var hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        foreach (var hit in hits)
        {
            if (hit.collider != null)
            {
                if (hit.collider.TryGetComponent<Rock>(out var rock))
                {
                    this.currentRockCluster = rock.GetComponentInParent<RockCluster>();
                    //print($"{this.currentRockCluster.name}/{rock.name}");
                }

                if (rock != null && !rock.DrawingChangedThisFrame)
                {
                    int id = rock.GetInstanceID();
                    bool isRockVisible = false;

                    if (rock.Fill)
                        isRockVisible = rock.Fill.color.a >= 1;

                    if (this.isShiftPressed && this.DrawMode != EDrawMode.Off)
                    {
                        rock.RockColorAnim.enabled = false;

                        rock.SetAlpha(0);

                        if (drawingData.RocksToModify.ContainsKey(id))
                        {
                            var data = this.DrawingData.RocksToModify[id];
                            data.IsVisible = false;
                        }
                        else
                        {
                            var color = Color.clear;

                            if (rock.Fill)
                                color = rock.Fill.color;

                            var rockEditData = new RockEditData
                            {
                                Id = id,
                                Color = color,
                                OutlineColor = rock.Outline.color,
                                IsVisible = false
                            };

                            drawingData.RocksToModify.Add(id, rockEditData);
                        }
                    }
                    else if (this.DrawMode == EDrawMode.Draw)
                    {
                        rock.IsVisible = true;
                        rock.SetAlpha(1);

                        if (this.DrawingData.RocksToModify.ContainsKey(id))
                        {
                            var data = this.DrawingData.RocksToModify[id];
                            if (rock.Fill)
                                data.Color = rock.Fill.color;
                            if (rock.Outline)
                                data.OutlineColor = rock.Outline.color;
                            data.IsVisible = true;
                        }
                        else
                        {
                            var rockEditData = new RockEditData
                            {
                                Id = id,
                                IsVisible = true
                            };

                            if (rock.Fill)
                                rockEditData.Color = rock.Fill.color;
                            if (rock.Outline)
                                rockEditData.OutlineColor = rock.Outline.color;

                            drawingData.RocksToModify.Add(id, rockEditData);
                        }
                    }
                    else if (isRockVisible && this.DrawMode == EDrawMode.Color)
                    {
                        rock.RockColorAnim.enabled = false;

                        rock.SetFillColor(this.TextureColor);

                        var outlineColor = rock.Outline != null ? rock.Outline.color : Color.black;

                        rock.SetOutlineColor(outlineColor);

                        if (!drawingData.RocksToModify.ContainsKey(id))
                        {

                            var rockEditData = new RockEditData
                            {
                                Id = id,
                                Color = this.TextureColor,
                                OutlineColor = outlineColor,
                                IsVisible = true
                            };

                            drawingData.RocksToModify.Add(id, rockEditData);
                        }
                        else
                        {
                            var thisRock = drawingData.RocksToModify[id];
                            thisRock.Color = this.TextureColor;
                            thisRock.OutlineColor = outlineColor;

                            if (thisRock.RockData != null)
                                thisRock.RockData.IsAnimatedTexture = false;
                        }
                    }
                    else if (this.DrawMode == EDrawMode.Preset && this.SelectedPreset != null)
                    {
                        ApplyPresetToRock(rock, this.SelectedPreset);
                    }

                    rock.DrawingChangedThisFrame = true;
                    this.changedRocks.Add(rock);
                }
            }
        }
    }

    private void ApplyPresetToRock(Rock inRock, RockData inPreset)
    {
        int id = inRock.GetInstanceID();
        var drawingData = TileEditorTool.IN.DrawingData;

        inRock.RockColorAnim.enabled = inPreset.IsAnimatedTexture;

        inRock.SetAlpha(1);
        inRock.IsVisible = true;

        inRock.SetFillColor(inPreset.FillColor);
        inRock.SetOutlineColor(inPreset.OutlineColor);

        if (inPreset.IsAnimatedTexture)
        {
            inRock.ApplyRockData(inPreset);
        }

        if (!drawingData.RocksToModify.ContainsKey(id))
        {
            var rockEditData = new RockEditData
            {
                Id = id,
                Color = inPreset.FillColor,
                OutlineColor = inPreset.OutlineColor,
                BaseRockHealthOverride = inPreset.BaseHealth,
                RockData = inPreset,
                IsVisible = true
            };

            drawingData.RocksToModify.Add(id, rockEditData);
        }
        else
        {
            var data = this.DrawingData.RocksToModify[id];

            data.Color = inPreset.FillColor;
            data.OutlineColor = inPreset.OutlineColor;
            data.BaseRockHealthOverride = inPreset.BaseHealth;
            data.RockData = inPreset;
            data.IsVisible = true;
        }
    }

    public void SelectRockPreset(RockData inRockData)
    {
        this.SelectedPreset = inRockData;
    }

    private void HandleKeyboardInput()
    {
        this.isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
        this.isOptionPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt);
        this.isAltPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt);

        var presetIndex = -1;

        if (Input.GetKeyDown(KeyCode.Alpha0))
            presetIndex = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha1))
            presetIndex = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            presetIndex = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            presetIndex = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            presetIndex = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            presetIndex = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha6))
            presetIndex = 6;
        else if (Input.GetKeyDown(KeyCode.Alpha7))
            presetIndex = 7;
        else if (Input.GetKeyDown(KeyCode.Alpha8))
            presetIndex = 8;
        else if (Input.GetKeyDown(KeyCode.Alpha9))
            presetIndex = 9;

        if (presetIndex != -1)
            SelectPresetAtIndex(presetIndex);

        if (this.isOptionPressed)
        {
            if (this.isShiftPressed)
            {
                if (presetIndex != -1)
                {
                    this.DrawingData?.RocksToModify?.Clear();//not sure about this

                    if (this.rocksInScene.Length == 0)
                        this.rocksInScene = Object.FindObjectsByType<Rock>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                    foreach (var rock in this.rocksInScene)
                    {
                        ApplyPresetToRock(rock, this.SelectedPreset);
                    }
                }
            }
            else
            {
                if (presetIndex != -1)
                {
                    if (this.currentRockCluster != null)
                    {
                        foreach (var rock in this.currentRockCluster.Rocks)
                        {
                            ApplyPresetToRock(rock, this.SelectedPreset);
                        }
                    }
                }
            }
        }

        if (!this.isShiftPressed && !this.isOptionPressed && !this.isAltPressed)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                this.DrawMode = EDrawMode.Off;
            else if (Input.GetKeyDown(KeyCode.D))
                this.DrawMode = EDrawMode.Draw;
            else if (Input.GetKeyDown(KeyCode.C))
                this.DrawMode = EDrawMode.Color;
        }
    }

    private void SelectPresetAtIndex(int inIndex)
    {
        this.DrawMode = EDrawMode.Preset;

        if (inIndex > 0 && inIndex < this.Presets.Count)
        {
            var preset = this.Presets[inIndex];
            SelectRockPreset(preset);
            return;
        }

        this.SelectedPreset = null;
    }
}