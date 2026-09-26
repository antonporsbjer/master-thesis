using UnityEngine;
using TMPro;

public class ExperimentHUD : MonoBehaviour
{
    public TextMeshProUGUI hudText;
    
    public float simulationTime = 0f;
    public float SimulationTime => simulationTime;

    internal float realTimeStart;
    internal float smoothedFPS = 60f;
    private bool tickedThisFrame = false;

    void Awake()
    {
        if (hudText == null)
            hudText = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        ResetHUD();
    }

    public void ResetHUD()
    {
        simulationTime = 0f;
        realTimeStart = Time.realtimeSinceStartup;
        tickedThisFrame = false;
    }

    public void RegisterSimTick()
    {
        float dt = (SimulationGrid.instance != null && SimulationGrid.instance.dt > 0f) ? SimulationGrid.instance.dt : Time.deltaTime;
        simulationTime += dt;
        tickedThisFrame = true;
    }

    void Update()
    {
        // Always increment simulation time: from RegisterSimTick if called, or fallback directly from dt
        if (!tickedThisFrame)
        {
            float dt = (SimulationGrid.instance != null && SimulationGrid.instance.dt > 0f)
                ? SimulationGrid.instance.dt
                : Time.deltaTime;
            simulationTime += dt;
        }
        tickedThisFrame = false;

        if (hudText == null)
        {
            hudText = GetComponentInChildren<TextMeshProUGUI>();
            if (hudText == null) return;
        }

        // Real World Time
        float elapsedRealTime = Time.realtimeSinceStartup - realTimeStart;

        // Frame Rate
        float fps = Time.unscaledDeltaTime > 0f ? 1.0f / Time.unscaledDeltaTime : 0f;
        smoothedFPS = Mathf.Lerp(smoothedFPS, fps, 0.05f);

        string runInfo = "";
        if (RunManager.instance != null)
        {
            runInfo = string.Format("Run: {0}/{1}\nTarget Sim Time: {2:F0}s\n",
                RunManager.instance.CurrentRunNumber,
                RunManager.instance.TotalRunsCount,
                RunManager.instance.simulationTimePerRun);
        }

        hudText.text = string.Format(
            "{0}" +
            "Sim Time: {1:F1}s\n" +
            "Real Time: {2:F1}s\n" +
            "FPS: {3:F0}",
            runInfo, simulationTime, elapsedRealTime, smoothedFPS);
            
        if (elapsedRealTime > simulationTime + 0.5f) 
        {
            hudText.color = Color.yellow;
        }
        else
        {
            hudText.color = Color.white;
        }
    }
}