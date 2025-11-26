using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager IN;

    [ReadOnly,SerializeField] private int currentMapIndex = 0;
    [Space]
    [SerializeField] private TextMeshProUGUI mapNumText;
    [SerializeField] private TextMeshProUGUI mapNameText;
    [Space]
    private MapData[] allMapDatas;
    
    [Header("Optional - Assign to manually set levels")]
    [SerializeField] private MapData[] mapDatas;

    private void Start()
    {
        if(this.mapDatas.Length > 0)
            this.allMapDatas = this.mapDatas;
        else
            this.allMapDatas = Resources.LoadAll("Data/Maps", typeof(MapData)).Cast<MapData>().ToArray();
    }

    public void LoadMapNum(int inIndex)
    {
        if (this.currentMapIndex < 0 || this.currentMapIndex >= this.allMapDatas.Length)
            inIndex = 0;

        this.currentMapIndex = inIndex;
        PlayerData.Data.CurrentLevelIndex = inIndex;

        var levelData = this.allMapDatas[inIndex];

        this.mapNumText.text = $"{levelData.MapNum}";
        this.mapNameText.text = $"{levelData.Name}";

        TileLoadingManager.IN.LoadMap(levelData);
    }

    public void ReloadMap()
    {
        LoadMapNum(this.currentMapIndex);
    }

    public void HandlePrevLevelButtonPress()
    {
        if (this.currentMapIndex > 0)
            LoadMapNum(this.currentMapIndex - 1);
    }

    public void HandleNextLevelButtonPress()
    {
        if (this.currentMapIndex < this.allMapDatas.Length - 1)
            LoadMapNum(this.currentMapIndex + 1);
    }
}