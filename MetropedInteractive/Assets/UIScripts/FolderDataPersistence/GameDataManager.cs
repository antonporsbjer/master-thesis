using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{

    public string saveFile;
    public string idFile;
    public List<GameData> gameDataList = new List<GameData>();
    private int currentPlayerId;

    void Awake()
    {
        saveFile = Path.Combine(Application.persistentDataPath + "gamedata.json");
        idFile = Path.Combine(Application.persistentDataPath + "lastPlayerId.txt");

        readFile();
        assignNewPlayerId();
    }

    private void assignNewPlayerId()
    {
        if (File.Exists(idFile))
        {
            string idString = File.ReadAllText(idFile);
            if (int.TryParse(idString, out int lastId))
            {
                currentPlayerId = lastId + 1;
            }
            else
            {
                Debug.LogWarning("Invalid lastPlayerId format, starting from new id: 1" + idString);
                currentPlayerId = 1;
            }
        }
        else{
            currentPlayerId = 1;
        }
        saveLastPlayerId();
        Debug.Log("PlayerId written to " + idFile);

    }

    private void saveLastPlayerId()
    {
        File.WriteAllText(idFile, currentPlayerId.ToString("D5"));
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
        newGameData.playerId = currentPlayerId.ToString("D5");
        gameDataList.Add(newGameData);
        writeFile();
    }

    [System.Serializable]
    public class GameDataList
    {
        public List<GameData> gameDataList;
    }


}
