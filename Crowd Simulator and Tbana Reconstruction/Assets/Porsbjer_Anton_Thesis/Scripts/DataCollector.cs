using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class GlobalData
{
    public readonly DateTime timestamp;
    public int totalAgents;
    public string scenarioId;
    public int inVcaCount;
    public int visibleSignCount;

    public GlobalData()
    {
        timestamp = DateTime.Now;
        totalAgents = 0;
        scenarioId = "default_scenario";
    }
}

[Serializable]
public class AgentData
{
    public int agentId;
    public string type;
    public int startNode;
    public int goalNode;
    public float timeInVCA;
    public bool sawSign;

    public AgentData(int id, string agentType, int start, int goal)
    {
        agentId = id;
        type = agentType;
        startNode = start;
        goalNode = goal;
    }
}

[Serializable]
public class DataRecord
{
    public GlobalData global;
    public List<AgentData> agents = new();
}

public class DataCollector : MonoBehaviour
{
    public DataRecord dataRecord;

    private void SaveToJSON()
    {
        try
        {
            // Convert to JSON
            string json = JsonUtility.ToJson(dataRecord, true);

            // Prepare safe file name
            string folderPath = Application.persistentDataPath;
            string fileName = $"visibility_data_{dataRecord.global.timestamp.ToString("yyyy-MM-dd_HH.mm.ss")}.json";
            string filePath = Path.Combine(folderPath, fileName);

            // Ensure directory exists
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Write to file
            File.WriteAllText(filePath, json);
            Debug.Log($"[DataCollector] Data saved to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DataCollector] Failed to save JSON: {ex.Message}");
        }
    }

    private void Awake()
    {
        dataRecord.global.scenarioId = SceneManager.GetActiveScene().name + "_scenario";
    }

    private void OnApplicationQuit()
    {
        dataRecord.global.totalAgents = dataRecord.agents.Count;
        dataRecord.global.visibleSignCount = dataRecord.agents.FindAll(agent => agent.sawSign).Count;
        SaveToJSON();
    }
}
