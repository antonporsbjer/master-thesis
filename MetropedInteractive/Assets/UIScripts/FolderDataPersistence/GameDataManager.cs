using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{

    public string saveFile;
    public GameData gameData = new GameData();

    void Awake()
    {
        saveFile = Path.Combine(Application.persistentDataPath + "gamedata.json");
    }

    public void readFile()
    {
        if (File.Exists(saveFile))
        {
            string fileContents = File.ReadAllText(saveFile);

            gameData = JsonUtility.FromJson<GameData>(fileContents);
        }
        else
        {
            Debug.LogWarning("Save file not found at " + saveFile); 
        }
    }

    public void writeFile()
    {
        string jsonString = JsonUtility.ToJson(gameData, true);
        
        File.WriteAllText(saveFile, jsonString);
        Debug.Log("Data written to " + saveFile);
    }






}
