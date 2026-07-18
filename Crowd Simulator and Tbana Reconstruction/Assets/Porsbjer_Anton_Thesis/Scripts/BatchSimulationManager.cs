using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BatchSimulationManager : MonoBehaviour
{
    [Header("Batch Configuration")]
    public bool runBatchSimulation = false;
    public float simulationDurationPerRun = 30f; // Seconds to run each scenario Run faster than real-time
    
    public enum ScenarioType { Motamedi_Scenario_C_Grid, Custom_Density_RQ1 }
    [Header("Scenario Settings")]
    public ScenarioType activeScenario = ScenarioType.Motamedi_Scenario_C_Grid;

    [System.Serializable]
    public struct DensityProfile
    {
        public float uicAlpha;
        public float spawnRate;
    }

    [Tooltip("Profiles to iterate through when activeScenario is Custom_Density_RQ1")]
    public DensityProfile[] rq1DensityProfiles = new DensityProfile[]
    {
        // Comfortable / Low Density: Agents spawn slowly, maintain large personal space
        new DensityProfile { uicAlpha = 0.25f, spawnRate = 0.25f },
        
        // Normal / Medium Density
        new DensityProfile { uicAlpha = 0.5f,  spawnRate = 0.5f },
        
        // Dense / Rush Hour
        new DensityProfile { uicAlpha = 1.0f, spawnRate = 1.0f },
        
        // Extreme / Crowd Crush: Agents flood in, pack physically shoulder-to-shoulder
        new DensityProfile { uicAlpha = 2.0f,  spawnRate = 2.0f }
    };
    
    [Header("Run Limits")]
    [Tooltip("Calculated total runs to execute based on bounds and step size. (Auto-updates)")]
    [SerializeField]
    private int runsToExecute = 0;
    
    [Header("Grid Configuration")]
    public GameObject signGridBounds;
    public float stepSize = 1f;
    
    [Header("Simulated Sign Orientations (Degrees)")]
    public OrientationType orientationType = OrientationType.Horizontal;
    public enum OrientationType { Horizontal = 0, Vertical = 90 }
    
    [Header("References")]
    public GameObject signObject;
    public DataCollector dataCollector;
    
    private int runCounter = 1;
    private bool isBatching = false;
    
    private float minX = 0f;
    private float maxX = 0f;
    private float minZ = 0f;
    private float maxZ = 0f;

    private void OnValidate()
    {
        if (signGridBounds != null && signGridBounds.transform.childCount > 0 && stepSize > 0)
        {
            float tMinX = float.MaxValue;
            float tMaxX = float.MinValue;
            float tMinZ = float.MaxValue;
            float tMaxZ = float.MinValue;

            foreach (Transform child in signGridBounds.transform)
            {
                Vector3 pos = child.position;
                if (pos.x < tMinX) tMinX = pos.x;
                if (pos.x > tMaxX) tMaxX = pos.x;
                if (pos.z < tMinZ) tMinZ = pos.z;
                if (pos.z > tMaxZ) tMaxZ = pos.z;
            }

            int stepsX = Mathf.FloorToInt((tMaxX - tMinX) / stepSize) + 1;
            int stepsZ = Mathf.FloorToInt((tMaxZ - tMinZ) / stepSize) + 1;
            runsToExecute = stepsX * stepsZ;
        }
    }

    void Start()
    {
        if (runBatchSimulation)
        {
            if (signGridBounds != null && signGridBounds.transform.childCount > 0)
            {
                minX = float.MaxValue;
                maxX = float.MinValue;
                minZ = float.MaxValue;
                maxZ = float.MinValue;

                foreach (Transform child in signGridBounds.transform)
                {
                    Vector3 pos = child.position;
                    if (pos.x < minX) minX = pos.x;
                    if (pos.x > maxX) maxX = pos.x;
                    if (pos.z < minZ) minZ = pos.z;
                    if (pos.z > maxZ) maxZ = pos.z;
                }
                
                int stepsX = Mathf.FloorToInt((maxX - minX) / stepSize) + 1;
                int stepsZ = Mathf.FloorToInt((maxZ - minZ) / stepSize) + 1;
                runsToExecute = stepsX * stepsZ;

                Debug.Log($"[BatchSimulationManager] Auto-configured Grid Bounds from {signGridBounds.name}'s children: X[{minX}, {maxX}], Z[{minZ}, {maxZ}]. Expected total runs: {runsToExecute}");
            }
            else
            {
                Debug.LogWarning("[BatchSimulationManager] signGridBounds is not assigned or has no children! Grid bounds will be zero.");
            }

            VisibilityVolume vca = FindObjectOfType<VisibilityVolume>();
            if (vca == null)
            {
                Debug.LogError("[BatchSimulationManager] Could not find any VisibilityVolume in the scene! Aborting batch.");
                return;
            }

            // Force signObject to be the EXACT object the VisibilityVolume component is attached to.
            // This overrides anything manually (and potentially incorrectly) dragged into the Inspector slot.
            signObject = vca.gameObject;

            Debug.Log($"[BatchSimulationManager] Found sign object: {signObject.name}");
            if (signObject.transform.childCount > 10)
            {
                Debug.LogWarning($"[BatchSimulationManager] WARNING: The sign object '{signObject.name}' has {signObject.transform.childCount} children! If the environment is moving, it is because your environment is dragged INSIDE this sign object in the Hierarchy, or the VisibilityVolume script was accidentally added to an environment folder.");
            }

            if (dataCollector == null) dataCollector = FindObjectOfType<DataCollector>();
            
            // Validate references
            if (dataCollector == null)
            {
                Debug.LogError("[BatchSimulationManager] Missing DataCollector reference. Aborting batch.");
                return;
            }
            
            // Disable random positioning on the sign itself so we control it manually
            vca.RandomPosition = false;

            StartCoroutine(RunBatch());
        }
    }

    private IEnumerator RunBatch()
    {
        isBatching = true;
        Debug.Log($"[BatchSimulationManager] Starting batch simulation.");

        float currentX = minX;
        float currentZ = minZ;
        float yaw = (float)orientationType;

        // Density thresholds to test for RQ1
        DensityProfile[] densityThresholds = activeScenario == ScenarioType.Custom_Density_RQ1 
            ? rq1DensityProfiles 
            : new DensityProfile[] { new DensityProfile { uicAlpha = 1.0f, spawnRate = 1.0f } }; // Just default if not doing RQ1

        // Iterate Orientations
        int processedRunsCount = 0;

        foreach (DensityProfile profile in densityThresholds)
        {
            // Iterate Z
        for (currentZ = minZ; currentZ <= maxZ; currentZ += stepSize)
        {
            // Iterate X
            for (currentX = minX; currentX <= maxX; currentX += stepSize)
            {
                // Check if we hit the user-specified limit
                if (runsToExecute > 0 && processedRunsCount >= runsToExecute && activeScenario != ScenarioType.Custom_Density_RQ1)
                {
                    Debug.Log($"[BatchSimulationManager] Reached the maximum run limit of {runsToExecute}. Ending batch early.");
                    goto BatchFinished; // Jump out of all nested loops
                }

                Debug.Log($"[BatchSimulationManager] Run {runCounter}: Placing sign at ({currentX}, {currentZ}) Yaw: {yaw} Alpha: {profile.uicAlpha} SpawnRate: {profile.spawnRate}");
                
                // 1. Reset Global Data
                dataCollector.ResetForNextRun();
                
                // 2. Clear out any existing agents from the previous run
                ClearExistingAgents();

                // 2.5 Set the grid density (UIC alpha parameter) and Spawn Rate
                if (SimulationGrid.instance != null)
                {
                    SimulationGrid.instance.alpha = profile.uicAlpha;
                }
                
                if (SpawnerManager.instance != null)
                {
                    SpawnerManager.instance.SetGlobalSpawnRate(profile.spawnRate);
                }
                else
                {
                    Debug.LogWarning("[BatchSimulationManager] No SpawnerManager instance found. Spawn rate won't be updated.");
                }

                // 3. Move the sign
                Vector3 targetPosition = new Vector3(currentX, signObject.transform.position.y, currentZ);
                Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);

                // If the sign relies on physics, move its rigidbody directly so it doesn't fight our Transform updates
                Rigidbody signRb = signObject.GetComponent<Rigidbody>();
                if (signRb != null)
                {
                    signRb.MovePosition(targetPosition);
                    signRb.MoveRotation(targetRotation);
                    
                    // Force it to sleep to kill lingering physics forces
                    signRb.velocity = Vector3.zero;
                    signRb.angularVelocity = Vector3.zero;
                }
                else
                {
                    signObject.transform.position = targetPosition;
                    signObject.transform.rotation = targetRotation;
                }

                // 4. Force global data capture for this new sign position
                VisibilityVolume vca = signObject.GetComponent<VisibilityVolume>();
                if (vca != null)
                {
                    // Update discrete nodes position!
                    if (vca.useDiscretization)
                    {
                        vca.GenerateDiscreteNodes();
                    }

                    dataCollector.dataRecord.global.signHeight = signObject.transform.position.y;
                    dataCollector.dataRecord.global.signPositionX = currentX;
                    dataCollector.dataRecord.global.signPositionZ = currentZ;
                    dataCollector.dataRecord.global.signOrientation = yaw;
                    dataCollector.dataRecord.global.vcaDistance = vca.ViewingDistance;
                    dataCollector.dataRecord.global.vcaAngle = vca.ThetaDegrees;
                    dataCollector.dataRecord.global.signComprehensionTime = vca.comprehensionTime;
                }

                // 5. Let the simulation run for the specified duration (simulation time via SimulationGrid.dt)
                float elapsedSimTime = 0f;
                while (elapsedSimTime < simulationDurationPerRun)
                {
                    if (SimulationGrid.instance != null)
                    {
                        elapsedSimTime += SimulationGrid.instance.dt;
                    }
                    else
                    {
                        Debug.LogWarning("[BatchSimulationManager] No SimulationGrid found. Using Time.deltaTime.");
                        elapsedSimTime += Time.deltaTime;
                    }
                    yield return null;
                }

                // 6. Save Data
                dataCollector.SaveRun(runCounter);
                runCounter++;
                processedRunsCount++;
            }
        }
        } // End of Density Loop

        BatchFinished:
        // Cleanup
        isBatching = false;
        Debug.Log($"[BatchSimulationManager] BATCH COMPLETE! Processed {processedRunsCount} runs in total.");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    private void ClearExistingAgents()
    {
        // Find existing agents by their component rather than a hardcoded tag
        Agent[] agents = FindObjectsOfType<Agent>();
        Main simMain = FindObjectOfType<Main>();
        
        if (simMain != null && simMain.agentList != null)
        {
            simMain.agentList.Clear();
        }

        foreach (var agent in agents)
        {
            if (agent != null && agent.gameObject != null)
            {
                Destroy(agent.gameObject);
            }
        }
        
        // Let one frame pass to ensure objects are destroyed and spawners can reset
    }
}
