using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{

    public string saveFile;
    public List<GameData> gameDataList = new List<GameData>();

    void Awake()
    {
        saveFile = Path.Combine(Application.persistentDataPath + "gamedata.json");
    }

    public void readFile()
    {
        if (File.Exists(saveFile))
        {
            string fileContents = File.ReadAllText(saveFile);

            gameDataList = JsonUtility.FromJson<GameDataList>(fileContents)?.gameDataList ?? new List<GameData>();
        }
        else
        {
            Debug.LogWarning("Save file not found at " + saveFile); 
        }
    }

    public void writeFile()
    {
        GameDataList gameDataListWrapper = new GameDataList { gameDataList = this.gameDataList };
        string jsonString = JsonUtility.ToJson(gameDataListWrapper, true);
        
        File.WriteAllText(saveFile, jsonString);
        Debug.Log("Data written to " + saveFile);
    }

    public void SaveGameData(GameData newGameData)
    {
        gameDataList.Add(newGameData);
        writeFile();
    }

    [System.Serializable]
    public class GameDataList
    {
        public List<GameData> gameDataList;
    }


}
