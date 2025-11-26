using System;
using System.Collections.Generic;

[Serializable]
public class MapProgressData
{
    public static MapProgressData Data;
    
    public List<MapSaveData> MapSaveDatas = new();

    public MapSaveData CurrentMapSaveData
    {
        get
        {
            while (this.MapSaveDatas.Count <= PlayerData.Data.CurrentLevelIndex)
            {
                var newMapSaveData = new MapSaveData
                {
                    MapNum = this.MapSaveDatas.Count
                };

                this.MapSaveDatas.Add(newMapSaveData);
            }

            return this.MapSaveDatas[PlayerData.Data.CurrentLevelIndex];
        }
    }
}