using UnityEngine;

[CreateAssetMenu(fileName = "MapTileData", menuName = "Data/MapTileData", order = 5)]
public class MapTileData : ScriptableObject
{
    public string TileSceneName;
    public string FolderName;
    [Header("Setting this will rename the asset")]
    public Sprite TileThumbnail;

    [Header("For Map Editor")]
    public int TileGroup;
    
    [SerializeField] private bool shouldValidate = true;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!this.shouldValidate || this.TileThumbnail == null || this.TileSceneName == this.TileThumbnail.name)
            return;

        this.TileSceneName = this.TileThumbnail.name;

        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this.GetInstanceID());

        UnityEditor.AssetDatabase.RenameAsset(assetPath, this.TileSceneName);
        UnityEditor.AssetDatabase.SaveAssets();
    }
#endif
}