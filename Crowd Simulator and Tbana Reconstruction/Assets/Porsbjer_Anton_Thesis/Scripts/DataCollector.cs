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
    public float signHeight;
    public float signPositionX;
    public float signPositionZ;
    public float vcaAngle;
    public float vcaDistance;
    public float signComprehensionTime;

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
    public float height;
    public float eyeHeight;
    public float timeInVCA;
    public int timesInVCA;
    public bool sawSign;

    public AgentData(int id, string agentType, int start, int goal, float agentHeight, float agentEyeHeight)
    {
        agentId = id;
        type = agentType;
        startNode = start;
        goalNode = goal;
        height = agentHeight;
        eyeHeight = agentEyeHeight;
        timeInVCA = 0f; // Initialize time in VCA
        timesInVCA = 0; // Initialize times in VCA
        sawSign = false; // Initialize as not seeing the sign
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

    // save current dataRecord to disk (callable from other scripts)
    public void SaveRun(int runIndex = -1)
    {
        try
        {
            // Ensure global counters reflect current agents before saving
            if (dataRecord != null && dataRecord.global != null)
            {
                dataRecord.global.totalAgents = dataRecord.agents != null ? dataRecord.agents.Count : 0;
                dataRecord.global.visibleSignCount = dataRecord.agents != null ? dataRecord.agents.FindAll(a => a.sawSign).Count : 0;
            }

            string json = JsonUtility.ToJson(dataRecord, true);
            string folderPath = Application.persistentDataPath;
            string timePart = dataRecord.global != null ? dataRecord.global.timestamp.ToString("yyyy-MM-dd_HH.mm.ss") : DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
            string runPart = runIndex > 0 ? $"_run{runIndex}" : "";
            string fileName = $"visibility_data_{timePart}{runPart}.json";
            string filePath = Path.Combine(folderPath, fileName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            File.WriteAllText(filePath, json);
            Debug.Log($"[DataCollector] Run saved to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DataCollector] Failed to save JSON: {ex.Message}");
        }
    }

    // prepare a fresh record for the next run (keeps scenarioId if present)
    public void ResetForNextRun()
    {
        // keep the same DataRecord instance so other scripts don't see a suddenly different object reference
        if (dataRecord == null)
            dataRecord = new DataRecord();

        string scenario = dataRecord.global != null ? dataRecord.global.scenarioId : SceneManager.GetActiveScene().name + "_scenario";

        // clear agent list and reset globals
        dataRecord.agents.Clear();
        dataRecord.global = new GlobalData { scenarioId = scenario };
  }

    private void SaveToJSON()
    {
        try
        {
            // update counters before writing (same logic as SaveRun)
            if (dataRecord != null && dataRecord.global != null)
            {
                dataRecord.global.totalAgents = dataRecord.agents != null ? dataRecord.agents.Count : 0;
                dataRecord.global.visibleSignCount = dataRecord.agents != null ? dataRecord.agents.FindAll(a => a.sawSign).Count : 0;
            }

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
        if (dataRecord == null)
            dataRecord = new DataRecord();
        if (dataRecord.global == null)
            dataRecord.global = new GlobalData();

        dataRecord.global.scenarioId = SceneManager.GetActiveScene().name + "_scenario";
    }

    private void OnApplicationQuit()
    {
        if (dataRecord != null)
        {
            dataRecord.global.totalAgents = dataRecord.agents.Count;
            dataRecord.global.visibleSignCount = dataRecord.agents.FindAll(agent => agent.sawSign).Count;
            SaveToJSON();
        }
    }
}
