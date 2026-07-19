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

    public static VisibilityVolume[,] signGrid;
    public static float gridMinX;
    public static float gridMinZ;
    public static float gridStepSize;
    public static int gridCols;
    public static int gridRows;
    public static System.Collections.Generic.List<VisibilityVolume> activeSigns = new System.Collections.Generic.List<VisibilityVolume>();

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

        float yaw = (float)orientationType;

        // Hide original sign object so it's not part of the active tracking
        signObject.SetActive(false);
        activeSigns.Clear();

        // Instantiate grid of signs ONCE
        Debug.Log($"[BatchSimulationManager] Instantiating signs across grid...");
        
        gridMinX = minX;
        gridMinZ = minZ;
        gridStepSize = stepSize;
        gridCols = Mathf.FloorToInt((maxX - minX) / stepSize) + 1;
        gridRows = Mathf.FloorToInt((maxZ - minZ) / stepSize) + 1;
        signGrid = new VisibilityVolume[gridRows, gridCols];

        for (int zIdx = 0; zIdx < gridRows; zIdx++)
        {
            float z = minZ + zIdx * stepSize;
            for (int xIdx = 0; xIdx < gridCols; xIdx++)
            {
                float x = minX + xIdx * stepSize;
                Vector3 targetPosition = new Vector3(x, signObject.transform.position.y, z);
                Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);

                GameObject clone = Instantiate(signObject, targetPosition, targetRotation);
                clone.SetActive(true);
                clone.name = $"Sign_{x}_{z}";

                // Disable MeshRenderers to save rendering performance
                MeshRenderer[] renderers = clone.GetComponentsInChildren<MeshRenderer>();
                foreach (var r in renderers) r.enabled = false;

                VisibilityVolume cloneVCA = clone.GetComponent<VisibilityVolume>();
                if (cloneVCA != null)
                {
                    if (cloneVCA.useDiscretization) cloneVCA.GenerateDiscreteNodes();
                    activeSigns.Add(cloneVCA);
                    signGrid[zIdx, xIdx] = cloneVCA;
                }
            }
        }
        Debug.Log($"[BatchSimulationManager] Instantiated {activeSigns.Count} signs simultaneously.");

        // Density thresholds to test for RQ1
        DensityProfile[] densityThresholds = activeScenario == ScenarioType.Custom_Density_RQ1 
            ? rq1DensityProfiles 
            : new DensityProfile[] { new DensityProfile { uicAlpha = 1.0f, spawnRate = 1.0f } }; // Just default if not doing RQ1

        foreach (DensityProfile profile in densityThresholds)
        {
            // Check if we hit the user-specified limit
            if (runsToExecute > 0 && runCounter > runsToExecute && activeScenario != ScenarioType.Custom_Density_RQ1)
            {
                Debug.Log($"[BatchSimulationManager] Reached the maximum run limit of {runsToExecute}. Ending batch early.");
                break;
            }

            Debug.Log($"[BatchSimulationManager] Run {runCounter}: Testing all signs simultaneously. Alpha: {profile.uicAlpha} SpawnRate: {profile.spawnRate}");
            
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

            // 4. Force global data capture
            VisibilityVolume firstVca = activeSigns.Count > 0 ? activeSigns[0] : null;
            if (firstVca != null)
            {
                dataCollector.dataRecord.global.signHeight = firstVca.transform.position.y;
                dataCollector.dataRecord.global.signOrientation = yaw;
                dataCollector.dataRecord.global.vcaDistance = firstVca.ViewingDistance;
                dataCollector.dataRecord.global.vcaAngle = firstVca.ThetaDegrees;
                dataCollector.dataRecord.global.signComprehensionTime = firstVca.comprehensionTime;
                dataCollector.dataRecord.global.signPositionX = 0; // Legacy, per-sign now handles this
                dataCollector.dataRecord.global.signPositionZ = 0;
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

            // 6. Output the data!
            Debug.Log($"[BatchSimulationManager] Saving data for Density Run {runCounter}");
            dataCollector.SaveRun(runCounter);

            // Also force DataCollector to save the Matrix!
            SaveDensityMatrixData(runCounter);

            runCounter++;
        }

        // Cleanup clones
        foreach (var vca in activeSigns)
        {
            if (vca != null) Destroy(vca.gameObject);
        }
        activeSigns.Clear();
        signObject.SetActive(true);

        Debug.Log("[BatchSimulationManager] All batch runs completed successfully.");
        isBatching = false;
        runBatchSimulation = false;
        
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

    private void SaveDensityMatrixData(int runIndex)
    {
        if (SimulationGrid.instance == null || dataCollector == null) return;

        int nZ = SimulationGrid.instance.nCellsZ;
        int nX = SimulationGrid.instance.nCellsX;

        int startZ = -1, endZ = -1, startX = -1, endX = -1;

        // Find index bounds based on physical signGrid bounds
        for (int z = 0; z < nZ; z++) {
            for (int x = 0; x < nX; x++) {
                Vector3 pos = SimulationGrid.instance.cellCenters[z, x];
                if (pos.x >= minX && pos.x <= maxX && pos.z >= minZ && pos.z <= maxZ) {
                    if (startZ == -1 || z < startZ) startZ = z;
                    if (endZ == -1 || z > endZ) endZ = z;
                    if (startX == -1 || x < startX) startX = x;
                    if (endX == -1 || x > endX) endX = x;
                }
            }
        }

        if (startZ == -1) return; // No cells found in grid bounds

        int rows = endZ - startZ + 1;
        int cols = endX - startX + 1;

        float[,] croppedDensity = new float[rows, cols];
        float[] xCoords = new float[cols];
        float[] zCoords = new float[rows];

        for (int z = 0; z < rows; z++) {
            zCoords[z] = SimulationGrid.instance.cellCenters[startZ + z, startX].z;
            for (int x = 0; x < cols; x++) {
                if (z == 0) xCoords[x] = SimulationGrid.instance.cellCenters[startZ, startX + x].x;
                croppedDensity[z, x] = SimulationGrid.instance.density[startZ + z, startX + x];
            }
        }

        dataCollector.SaveDensityMatrix(croppedDensity, xCoords, zCoords, runIndex);
    }
}
