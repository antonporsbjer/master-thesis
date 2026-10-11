using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class RunParameters
{
    [Tooltip("Sign mounting height (Y position in meters).")]
    public float signHeight = 2.3f;

    [Tooltip("Sign downward pitch tilt angle in degrees.")]
    public float signTilt = 0f;

    [Tooltip("Crowd density / spawn rate (agents per second).")]
    public float crowdDensity = 1.0f;
}

public class RunManager : MonoBehaviour
{
    public static RunManager instance;

    [Header("Run Configuration")]
    [Tooltip("Simulation duration per run in seconds.")]
    public float simulationTimePerRun = 60f;

    [Tooltip("Individual parameters for each run (sign height, tilt, crowd density).")]
    public List<RunParameters> runs = new List<RunParameters>();

    [Header("Sign Target (Optional)")]
    [Tooltip("Specific sign to configure. If null/empty, all signs in scene are configured.")]
    public VisibilityVolume targetSign;
    public string targetSignName = "";

    [Header("Data Collection")]
    private DataCollector dataCollector;

    private int currentRun = 0;

    public int CurrentRunNumber => currentRun + 1;
    public int TotalRunsCount => runs != null ? runs.Count : 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            currentRun = 0;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (instance != this) return;

        if (targetSign != null && string.IsNullOrEmpty(targetSignName))
        {
            targetSignName = targetSign.signName;
        }

        ExperimentHUD.RunInfoProvider = () => string.Format("Run: {0}/{1}\nTarget Sim Time: {2:F0}s\n",
            CurrentRunNumber,
            TotalRunsCount,
            simulationTimePerRun);

        StartCoroutine(RunLoop());
    }

    IEnumerator RunLoop()
    {
        int total = TotalRunsCount;
        if (total == 0)
        {
            Debug.LogWarning("[RunManager] No runs configured in runs list. Exiting RunLoop.");
            yield break;
        }

        while (currentRun < total)
        {
            yield return new WaitUntil(() => FindObjectOfType<DataCollector>() != null);
            dataCollector = FindObjectOfType<DataCollector>();
            if (dataCollector != null)
                dataCollector.runIndex = currentRun + 1;

            RunParameters param = GetParametersForRun(currentRun);
            ApplyParameters(param);

            ExperimentHUD hud = FindObjectOfType<ExperimentHUD>();
            if (hud != null)
            {
                hud.ResetHUD();
            }

            Debug.Log($"[RunManager] Run {currentRun + 1}/{total} started — Height: {param.signHeight}m, Tilt: {param.signTilt}°, SpawnRate: {param.crowdDensity}, SimTime: {simulationTimePerRun}s");

            // Wait until accumulated simulation time reaches simulationTimePerRun
            float simTimeElapsed = 0f;
            while (simTimeElapsed < simulationTimePerRun)
            {
                yield return null;
                if (hud == null)
                    hud = FindObjectOfType<ExperimentHUD>();

                if (hud != null)
                {
                    simTimeElapsed = hud.SimulationTime;
                }
                else
                {
                    float dt = (SimulationGrid.instance != null && SimulationGrid.instance.dt > 0f) ? SimulationGrid.instance.dt : Time.deltaTime;
                    simTimeElapsed += dt;
                }
            }

            dataCollector.SaveRun(currentRun + 1);
            currentRun++;

            if (currentRun >= total)
                break;

            dataCollector.ResetForNextRun();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            yield return null;
        }

        Debug.Log($"[RunManager] Completed all {total} runs.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    RunParameters GetParametersForRun(int index)
    {
        if (runs != null && runs.Count > 0)
        {
            return runs[Mathf.Clamp(index, 0, runs.Count - 1)];
        }
        return new RunParameters();
    }

    void ApplyParameters(RunParameters param)
    {
        if (param == null) return;

        // Resolve target sign after scene reload if needed
        if (targetSign == null && !string.IsNullOrEmpty(targetSignName))
        {
            foreach (var s in FindObjectsOfType<VisibilityVolume>())
            {
                if (s != null && (s.signName == targetSignName || s.gameObject.name == targetSignName))
                {
                    targetSign = s;
                    break;
                }
            }
        }

        // Apply height and tilt to target or all signs
        VisibilityVolume[] signs = targetSign != null ? new[] { targetSign } : FindObjectsOfType<VisibilityVolume>();
        foreach (var sign in signs)
        {
            if (sign == null) continue;

            Vector3 pos = sign.transform.position;
            pos.y = param.signHeight;
            sign.transform.position = pos;

            sign.tiltAngle = param.signTilt;
            sign.ApplyTilt();

            if (sign.useDiscretization)
                sign.GenerateDiscreteNodes();
        }

        // Apply crowd density / spawn rate to SpawnerManager and spawners
        SpawnerManager spawnerManager = FindObjectOfType<SpawnerManager>();
        if (spawnerManager != null)
        {
            spawnerManager.ApplyCrowdDensity(param.crowdDensity);
        }
        else
        {
            foreach (NewSpawner spawner in FindObjectsOfType<NewSpawner>())
            {
                if (spawner == null) continue;
                spawner.spawnRate = param.crowdDensity;
                if (spawner.spawnMethod == NewSpawner.Method.continuousSpawn)
                {
                    spawner.StopAllCoroutines();
                    spawner.continousSpawn();
                }
            }
        }

        // Sync DataCollector global records
        if (dataCollector != null && dataCollector.dataRecord != null && dataCollector.dataRecord.global != null)
        {
            dataCollector.dataRecord.global.signHeight = param.signHeight;
            dataCollector.dataRecord.global.signTiltAngle = param.signTilt;
            dataCollector.dataRecord.global.crowdDensityAlpha = param.crowdDensity;
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            ExperimentHUD.RunInfoProvider = null;
        }
    }
}