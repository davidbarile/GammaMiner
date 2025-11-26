using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Data/MapData", order = 4)]
public class MapData : ScriptableObject
{
    public static readonly string EMPTY_TILE = "EmptyTile";
    public int MapNum;
    public string Name;
    [ReadOnly]
    public string FileName;
    public Vector2Int StartTileCoords = new Vector2Int(-1, -1);
    public int NumTilesInMap = 0;

    //this is a hack because 2 dimensional arrays get deleted on play
    public MapTileDataObject[] TileDataObjectsArray = new MapTileDataObject[0];

    public Vector2Int ArrayDims;

    private void OnValidate()
    {
        this.FileName = this.name;

        if (this.NumTilesInMap > 0)
            return;

        this.NumTilesInMap = 0;

        foreach (var tileDataObj in this.TileDataObjectsArray)
        {
            if (tileDataObj != null && tileDataObj.TileData != null)
                ++this.NumTilesInMap;
        }

        SetDirty();
    }

    [Button(ButtonSizes.Large), GUIColor(1, 0, 0), PropertySpace(5)]
    public void ClearTileArray()
    {
        this.TileDataObjectsArray = new MapTileDataObject[0];
        this.NumTilesInMap = 0;

        SetDirty();
    }

    public void InitGrid(int inX, int inY)
    {
        if (this.TileDataObjectsArray.Length > 0)
            return;

        this.TileDataObjectsArray = new MapTileDataObject[inX * inY];
        this.ArrayDims = new Vector2Int(inX, inY);

        SetDirty();
    }

    public MapTileDataObject GetTileDataObjectAtCoords(int inX, int inY)
    {
        if (inX < 0 || inX >= this.ArrayDims.x || inY < 0 || inY >= this.ArrayDims.y)
        {
            Debug.Log($"<color=red>MapData.GetTileDataObjectAtCoords({inX}, {inY}) out of range of ArrayDims ({this.ArrayDims})</color>");
            return null;
        }

        var index = inX + (this.ArrayDims.x * inY);
        if( index < 0 || index >= this.TileDataObjectsArray.Length)
            Debug.LogError($"MapData.GetTileDataObjectAtCoords({inX}, {inY}) calculated index {index} out of range of TileDataObjectsArray.Length ({this.TileDataObjectsArray.Length}).  this.ArrayDims = {this.ArrayDims}");


        if (this.TileDataObjectsArray[index] == null || this.TileDataObjectsArray[index].TileData == null)
            return null;

        return this.TileDataObjectsArray[index];
    }

    public void InsertTileDataObjectAtCoords(MapTileDataObject inDataObject, int inX, int inY)
    {
        var index = (this.ArrayDims.x * inY) + inX;

        if (this.TileDataObjectsArray[index] == null && inDataObject != null)
            ++this.NumTilesInMap;
        else if (this.TileDataObjectsArray[index] != null && inDataObject == null)
            --this.NumTilesInMap;
            
        this.TileDataObjectsArray[index] = inDataObject;

        SetDirty();
    }

    public new void SetDirty()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

[Serializable]
public class MapTileDataObject
{
    public MapTileData TileData;
    public float Rotation;
}