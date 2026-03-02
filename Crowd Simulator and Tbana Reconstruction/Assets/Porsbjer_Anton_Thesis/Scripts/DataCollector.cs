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
    public int totalNodesNavigated;
    public int nodesWithDetection;
    public float rdEffective => totalNodesNavigated > 0 ? (float)nodesWithDetection / totalNodesNavigated : 0f;

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
        totalNodesNavigated = 0;
        nodesWithDetection = 0;
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
            
            // File names
            string jsonFileName = $"visibility_data_{timePart}{runPart}.json";
            string csvFileName  = $"visibility_data_{timePart}{runPart}.csv";
            
            string jsonFilePath = Path.Combine(folderPath, jsonFileName);
            string csvFilePath  = Path.Combine(folderPath, csvFileName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Save JSON
            File.WriteAllText(jsonFilePath, json);
            
            // Build and Save CSV
            System.Text.StringBuilder csv = new System.Text.StringBuilder();
            
            // Header
            csv.AppendLine("Timestamp,RunIndex,ScenarioID,TotalAgents,VisibleSignCount,SignHeight,SignPositionX,SignPositionZ,VcaAngle,VcaDistance,SignComprehensionTime,AgentID,AgentType,StartNode,GoalNode,Height,EyeHeight,TimeInVCA,TimesInVCA,SawSign,TotalNodesNavigated,NodesWithDetection,RDEffective");
            
            if (dataRecord.agents != null)
            {
                foreach (var agent in dataRecord.agents)
                {
                    csv.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22}",
                        timePart,
                        runIndex > 0 ? runIndex.ToString() : "N/A",
                        dataRecord.global.scenarioId,
                        dataRecord.global.totalAgents,
                        dataRecord.global.visibleSignCount,
                        dataRecord.global.signHeight,
                        dataRecord.global.signPositionX,
                        dataRecord.global.signPositionZ,
                        dataRecord.global.vcaAngle,
                        dataRecord.global.vcaDistance,
                        dataRecord.global.signComprehensionTime,
                        agent.agentId,
                        agent.type,
                        agent.startNode,
                        agent.goalNode,
                        agent.height,
                        agent.eyeHeight,
                        agent.timeInVCA,
                        agent.timesInVCA,
                        agent.sawSign,
                        agent.totalNodesNavigated,
                        agent.nodesWithDetection,
                        agent.rdEffective
                    ));
                }
            }
            
            File.WriteAllText(csvFilePath, csv.ToString());

            Debug.Log($"[DataCollector] Run saved to {jsonFilePath} and {csvFilePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DataCollector] Failed to save JSON/CSV: {ex.Message}");
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
    
    private void Awake()
    {
        if (dataRecord == null)
            dataRecord = new DataRecord();
        if (dataRecord.global == null)
            dataRecord.global = new GlobalData();

        dataRecord.global.scenarioId = SceneManager.GetActiveScene().name + "_scenario";
    }
}
