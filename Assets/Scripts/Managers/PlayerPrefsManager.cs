using System.IO;
using Leguar.TotalJSON;
using UnityEngine;

public static class PlayerPrefsManager
{
    private static bool playerDataExists;
    private static bool MapProgressDataExists;

#if UNITY_EDITOR
    private static readonly string FILE_PATH = "Assets/PlayerData/PlayerData.json";
    private static readonly string BACKUP_FILE_PATH = "Assets/PlayerData/PlayerData_bkup.json";

    private static readonly string MAP_PROGRESS_FILE_PATH = "Assets/PlayerData/MapProgressData.json";
    private static readonly string MAP_PROGRESS_BACKUP_FILE_PATH = "Assets/PlayerData/LevelProgressData_bkup.json";
#else
    private static readonly string FILE_PATH = Application.persistentDataPath + "/PlayerData.json";
    private static readonly string BACKUP_FILE_PATH = Application.persistentDataPath + "/PlayerData_bkup.json";

    private static readonly string MAP_PROGRESS_FILE_PATH = Application.persistentDataPath + "/MapProgressData.json";
    private static readonly string MAP_PROGRESS_BACKUP_FILE_PATH = Application.persistentDataPath + "/LevelProgressData_bkup.json";
#endif

    private static readonly string PLAYER_PREFS_EXIST = "PlayerDataExists";

    public static bool GetPlayerPrefBool(string inPlayerPrefName)
    {
        return PlayerPrefs.GetString(inPlayerPrefName) == "True" ? true : false;
    }

    public static void SavePlayerPrefBool(string inPlayerPrefName, bool inValue)
    {
        //Debug.Log("SavePlayerPrefBool( " + inPlayerPrefName + ", " + inValue + " )");
        string boolString = inValue ? "True" : "False";
        PlayerPrefs.SetString(inPlayerPrefName, boolString);
        PlayerPrefs.Save();
    }

    public static bool PlayerDataExists
    {
        get
        {
            PlayerPrefsManager.playerDataExists = PlayerPrefsManager.GetPlayerPrefBool(PLAYER_PREFS_EXIST);

            if (!PlayerPrefsManager.playerDataExists) //very first load or data cleared
            {
                Debug.Log("<color=yellow>FIRST TIME or CLEARED DATA</color>");

                PlayerPrefsManager.SavePlayerPrefBool(PLAYER_PREFS_EXIST, true);

                PlayerPrefs.Save();
            }
            else ////not very first load
            {
                Debug.Log("<color=green>PLAYER DATA EXISTS</color>");
            }

            return PlayerPrefsManager.playerDataExists;
        }
    }

    public static PlayerData GetPlayerData()
    {
        DeserializeSettings deserializeSettings = new DeserializeSettings()
        {
            RequireAllFieldsArePopulated = false
        };

        bool success = false;
        try
        {
            // Load from text file
            JSON savedPlayerDataJSON = LoadTextFileToJsonObject(PlayerPrefsManager.FILE_PATH);
            PlayerData.Data = savedPlayerDataJSON.Deserialize<PlayerData>(deserializeSettings);
            success = true;
        }
        catch
        {
            try
            {
                //load backup PlayerData from file, created (hopefully) before corruption of data
                JSON backupPlayerDataJSON = LoadTextFileToJsonObject(PlayerPrefsManager.BACKUP_FILE_PATH);
                PlayerData.Data = backupPlayerDataJSON.Deserialize<PlayerData>(deserializeSettings);

                Debug.Log("<color=red>FAILED TO LOAD PLAYERDATA, using backup data instead</color>");
            }
            catch
            {
                PlayerData.Data = GameManager.IN.GetDefaultPlayerData();
                PlayerData.Data.SetStartDate();

                Debug.Log("<color=red>FAILED TO LOAD BACKUP_PLAYERDATA, loading default PlayerData instead</color>");
            }
        }

        //save backup PlayerData to file, from freshly loaded, uncorrupted data
        if (success)
            SaveBackupPlayerData();

        return PlayerData.Data;
    }

    public static MapProgressData GetLevelProgressData()
    {
        DeserializeSettings deserializeSettings = new DeserializeSettings()
        {
            RequireAllFieldsArePopulated = false
        };

        bool success = false;
        try
        {
            // Load from text file
            JSON savedLevelProgressDataJSON = LoadTextFileToJsonObject(PlayerPrefsManager.MAP_PROGRESS_FILE_PATH);
            MapProgressData.Data = savedLevelProgressDataJSON.Deserialize<MapProgressData>(deserializeSettings);
            success = true;
        }
        catch
        {
            try
            {
                //load backup MapProgressData from file, created (hopefully) before corruption of data
                JSON backupLevelProgressDataJSON = LoadTextFileToJsonObject(PlayerPrefsManager.MAP_PROGRESS_BACKUP_FILE_PATH);
                MapProgressData.Data = backupLevelProgressDataJSON.Deserialize<MapProgressData>(deserializeSettings);

                Debug.Log("<color=red>FAILED TO LOAD MapProgressData, using backup data instead</color>");
            }
            catch
            {
                MapProgressData.Data = GameManager.IN.GetDefaultLevelProgressData();

                Debug.Log("<color=red>FAILED TO LOAD BACKUP_LevelProgressData, loading default MapProgressData instead</color>");
            }
        }

        //save backup MapProgressData to file, from freshly loaded, uncorrupted data
        if (success)
            SaveBackupLevelProgressData();

        return MapProgressData.Data;
    }

    public static void SavePlayerData()
    {
        PlayerData.Data.SaveSettings();

        // Serialize PlayerData to JSON object
        JSON playerDataJSON = JSON.Serialize(PlayerData.Data);

#if UNITY_EDITOR
        string jsonAsString = playerDataJSON.CreatePrettyString();
#else
        string jsonAsString = playerDataJSON.CreateString();
#endif

        var writer = new StreamWriter(PlayerPrefsManager.FILE_PATH);
        writer.WriteLine(jsonAsString);
        writer.Close();
    }

    public static void SaveBackupPlayerData()
    {
        JSON backupPlayerDataJSON = JSON.Serialize(PlayerData.Data);

#if UNITY_EDITOR
        string jsonAsString = backupPlayerDataJSON.CreatePrettyString();
#else
        string jsonAsString = backupPlayerDataJSON.CreateString();
#endif

        StreamWriter writer = new StreamWriter(PlayerPrefsManager.BACKUP_FILE_PATH);
        writer.WriteLine(jsonAsString);
        writer.Close();
    }

    public static void SaveLevelProgressData()
    {
        // Serialize MapProgressData to JSON object
        JSON LevelProgressDataJSON = JSON.Serialize(MapProgressData.Data);
        
#if UNITY_EDITOR
        string jsonAsString = LevelProgressDataJSON.CreatePrettyString();
#else
        string jsonAsString = LevelProgressDataJSON.CreateString();
#endif

        var writer = new StreamWriter(PlayerPrefsManager.MAP_PROGRESS_FILE_PATH);
        writer.WriteLine(jsonAsString);
        writer.Close();
    }

    public static void SaveBackupLevelProgressData()
    {
        JSON backupLevelProgressDataJSON = JSON.Serialize(MapProgressData.Data);

#if UNITY_EDITOR
        string jsonAsString = backupLevelProgressDataJSON.CreatePrettyString();
#else
        string jsonAsString = backupLevelProgressDataJSON.CreateString();
#endif

        var writer = new StreamWriter(PlayerPrefsManager.MAP_PROGRESS_BACKUP_FILE_PATH);
        writer.WriteLine(jsonAsString);
        writer.Close();
    }

    public static void DeletePlayerData()
    {
        var writer = new StreamWriter(PlayerPrefsManager.FILE_PATH);
        writer.WriteLine(string.Empty);
        writer.Close();

        var backupWriter = new StreamWriter(PlayerPrefsManager.BACKUP_FILE_PATH);
        backupWriter.WriteLine(string.Empty);
        backupWriter.Close();
    }

    public static void DeleteLevelProgressData()
    {
        var writer = new StreamWriter(PlayerPrefsManager.MAP_PROGRESS_FILE_PATH);
        writer.WriteLine(string.Empty);
        writer.Close();

        var backupWriter = new StreamWriter(PlayerPrefsManager.MAP_PROGRESS_BACKUP_FILE_PATH);
        backupWriter.WriteLine(string.Empty);
        backupWriter.Close();
    }

    private static JSON LoadTextFileToJsonObject(string inFilePath)
    {
        var reader = new StreamReader(inFilePath);
        string jsonAsString = reader.ReadToEnd();
        reader.Close();
        JSON jsonObject = JSON.ParseString(jsonAsString);
        return jsonObject;
    }
}