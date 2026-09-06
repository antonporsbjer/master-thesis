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
    public float crowdDensityAlpha;
    public int inVcaCount;
    public int visibleSignCount;
    public float signHeight;
    public float signPositionX;
    public float signPositionZ;
    public float signOrientation;
    public float vcaAngle;
    public float vcaDistance;
    public float signComprehensionTime;

    public GlobalData()
    {
        timestamp = DateTime.Now;
        totalAgents = 0;
        scenarioId = "default_scenario";
        crowdDensityAlpha = 1.0f;
    }
}

public class AgentSignData
{
    public string signName = "Sign";
    public bool isTargetAudience = true;
    public float signPositionX;
    public float signPositionZ;
    public float timeInVCA;
    public int timesInVCA;
    public bool sawSign;
    public bool canSeeSign;
    public float continuousExposureTime;
    public bool isInVCA;
    public float timeStampEnteredVCA;
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
    public int totalNodesNavigated;
    public int nodesWithDetection;
    public float rdEffective => totalNodesNavigated > 0 ? (float)nodesWithDetection / totalNodesNavigated : 0f;

    public Dictionary<VisibilityVolume, AgentSignData> signTracking = new Dictionary<VisibilityVolume, AgentSignData>();

    public AgentData(int id, string agentType, int start, int goal, float agentHeight, float agentEyeHeight)
    {
        agentId = id;
        type = agentType;
        startNode = start;
        goalNode = goal;
        height = agentHeight;
        eyeHeight = agentEyeHeight;
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

    [Header("Save Settings")]
    [Tooltip("If true, automatically saves the collected run data when stopping play mode / quitting the application.")]
    public bool autoSaveOnStop = true;

    [Tooltip("Optional custom directory path. Leave empty to use Application.persistentDataPath.")]
    public string customOutputDirectory = "";

    private bool hasSavedCurrentRun = false;

    public string GetOutputDirectory()
    {
        if (!string.IsNullOrEmpty(customOutputDirectory))
        {
            try
            {
                if (!Directory.Exists(customOutputDirectory))
                    Directory.CreateDirectory(customOutputDirectory);
                return customOutputDirectory;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DataCollector] Custom output directory '{customOutputDirectory}' error: {ex.Message}. Using persistentDataPath.");
            }
        }
        return Application.persistentDataPath;
    }

    [ContextMenu("Save Run Now")]
    public void SaveRunMenu()
    {
        SaveRun();
    }

    [ContextMenu("Open Save Directory")]
    public void OpenSaveDirectory()
    {
        string path = GetOutputDirectory();
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.RevealInFinder(path);
#else
        System.Diagnostics.Process.Start(path);
#endif
    }

    public void SaveRun(int runIndex = -1)
    {
        try
        {
            if (dataRecord == null)
            {
                Debug.LogWarning("[DataCollector] No dataRecord to save.");
                return;
            }

            // Ensure global counters and sign properties reflect current state
            if (dataRecord.global != null)
            {
                dataRecord.global.totalAgents = dataRecord.agents != null ? dataRecord.agents.Count : 0;

                if (SimulationGrid.instance != null)
                {
                    dataRecord.global.crowdDensityAlpha = SimulationGrid.instance.alpha;
                }

                // If global sign properties are not set, pull from scene's VisibilityVolume
                VisibilityVolume fallbackVca = FindObjectOfType<VisibilityVolume>();
                if (fallbackVca != null)
                {
                    if (dataRecord.global.signHeight == 0f) dataRecord.global.signHeight = fallbackVca.transform.position.y;
                    if (dataRecord.global.signPositionX == 0f) dataRecord.global.signPositionX = fallbackVca.transform.position.x;
                    if (dataRecord.global.signPositionZ == 0f) dataRecord.global.signPositionZ = fallbackVca.transform.position.z;
                    if (dataRecord.global.signOrientation == 0f) dataRecord.global.signOrientation = fallbackVca.transform.rotation.eulerAngles.y;
                    if (dataRecord.global.vcaDistance == 0f) dataRecord.global.vcaDistance = fallbackVca.ViewingDistance;
                    if (dataRecord.global.vcaAngle == 0f) dataRecord.global.vcaAngle = fallbackVca.ThetaDegrees;
                    if (dataRecord.global.signComprehensionTime == 0f) dataRecord.global.signComprehensionTime = fallbackVca.comprehensionTime;
                }
            }

            // Finalize any in-progress VCA exposure if simulation is ending
            float stopTime = Time.time;
            if (dataRecord.agents != null)
            {
                foreach (var agent in dataRecord.agents)
                {
                    if (agent.signTracking != null)
                    {
                        foreach (var signEntry in agent.signTracking)
                        {
                            var sData = signEntry.Value;
                            if (sData.isInVCA)
                            {
                                sData.timeInVCA += (stopTime - sData.timeStampEnteredVCA);
                                sData.isInVCA = false;
                                sData.timesInVCA++;
                            }
                        }
                    }
                }
            }

            string folderPath = GetOutputDirectory();
            string timePart = dataRecord.global != null ? dataRecord.global.timestamp.ToString("yyyy-MM-dd_HH.mm.ss") : DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
            string runPart = runIndex > 0 ? $"_run{runIndex}" : "";
            
            // File names
            string csvFileName  = $"visibility_data_{timePart}{runPart}.csv";
            string csvFilePath  = Path.Combine(folderPath, csvFileName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            using (StreamWriter writer = new StreamWriter(csvFilePath))
            {
                // Header
                writer.WriteLine("Timestamp,RunIndex,ScenarioID,CrowdDensityAlpha,TotalAgents,SignName,IsTargetAudience,SignHeight,SignPositionX,SignPositionZ,SignOrientation,VcaAngle,VcaDistance,SignComprehensionTime,AgentID,AgentType,StartNode,GoalNode,Height,EyeHeight,TimeInVCA,TimesInVCA,SawSign,TotalNodesNavigated,NodesWithDetection,RDEffective");
                
                if (dataRecord.agents != null)
                {
                    foreach (var agent in dataRecord.agents)
                    {
                        if (agent.signTracking != null && agent.signTracking.Count > 0)
                        {
                            foreach (var signEntry in agent.signTracking)
                            {
                                var sData = signEntry.Value;
                                var signVca = signEntry.Key;

                                string sName = !string.IsNullOrEmpty(sData.signName) 
                                    ? sData.signName 
                                    : (signVca != null ? signVca.signName : "Sign");
                                float sHeight = signVca != null ? signVca.transform.position.y : (dataRecord.global != null ? dataRecord.global.signHeight : 0f);
                                float sRotY = signVca != null ? signVca.transform.rotation.eulerAngles.y : (dataRecord.global != null ? dataRecord.global.signOrientation : 0f);
                                float sAngle = signVca != null ? signVca.ThetaDegrees : (dataRecord.global != null ? dataRecord.global.vcaAngle : 90f);
                                float sDist = signVca != null ? signVca.ViewingDistance : (dataRecord.global != null ? dataRecord.global.vcaDistance : 15f);
                                float sComp = signVca != null ? signVca.comprehensionTime : (dataRecord.global != null ? dataRecord.global.signComprehensionTime : 1f);

                                writer.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                    "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25}",
                                    timePart,
                                    runIndex > 0 ? runIndex.ToString() : "N/A",
                                    dataRecord.global != null ? dataRecord.global.scenarioId : "default_scenario",
                                    dataRecord.global != null ? dataRecord.global.crowdDensityAlpha : 1.0f,
                                    dataRecord.global != null ? dataRecord.global.totalAgents : dataRecord.agents.Count,
                                    sName,
                                    sData.isTargetAudience,
                                    sHeight,
                                    sData.signPositionX,
                                    sData.signPositionZ,
                                    sRotY,
                                    sAngle,
                                    sDist,
                                    sComp,
                                    agent.agentId,
                                    agent.type,
                                    agent.startNode,
                                    agent.goalNode,
                                    agent.height,
                                    agent.eyeHeight,
                                    sData.timeInVCA,
                                    sData.timesInVCA,
                                    sData.sawSign,
                                    agent.totalNodesNavigated,
                                    agent.nodesWithDetection,
                                    agent.rdEffective
                                ));
                            }
                        }
                    }
                }
            }

            hasSavedCurrentRun = true;
            Debug.Log($"[DataCollector] Run saved successfully to: {csvFilePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DataCollector] Failed to save CSV: {ex.Message}");
        }
    }

    // prepare a fresh record for the next run (keeps scenarioId if present)
    public void ResetForNextRun()
    {
        hasSavedCurrentRun = false;

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

    private void OnApplicationQuit()
    {
        if (autoSaveOnStop && !hasSavedCurrentRun && dataRecord != null && dataRecord.agents != null && dataRecord.agents.Count > 0)
        {
            Debug.Log("[DataCollector] Auto-saving run data on application quit/stop.");
            SaveRun();
        }
    }

    private void OnDestroy()
    {
        if (autoSaveOnStop && !hasSavedCurrentRun && dataRecord != null && dataRecord.agents != null && dataRecord.agents.Count > 0)
        {
            Debug.Log("[DataCollector] Auto-saving run data on destroy.");
            SaveRun();
        }
    }
}
