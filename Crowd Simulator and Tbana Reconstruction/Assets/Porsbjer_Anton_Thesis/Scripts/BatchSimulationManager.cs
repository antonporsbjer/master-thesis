using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BatchSimulationManager : MonoBehaviour
{
    [Header("Batch Configuration")]
    public bool runBatchSimulation = false;
    public float simulationDurationPerRun = 30f; // Seconds to run each scenario
    public float timeScaleMultiplier = 5f; // Run faster than real-time
    
    [Header("Grid Configuration")]
    public float minX = -5f;
    public float maxX = 5f;
    public float minZ = -5f;
    public float maxZ = 5f;
    public float stepSize = 1f;
    
    [Header("Simulated Orientations (Degrees)")]
    public float[] orientations = { 0f, 90f }; // Horizontal and Vertical

    [Header("References")]
    public GameObject signObject;
    public DataCollector dataCollector;
    
    private int runCounter = 1;
    private bool isBatching = false;

    void Start()
    {
        if (runBatchSimulation)
        {
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
        Time.timeScale = timeScaleMultiplier;
        Debug.Log($"[BatchSimulationManager] Starting batch simulation at {timeScaleMultiplier}x speed.");

        float currentX = minX;
        float currentZ = minZ;

        // Iterate Orientations
        foreach (float yaw in orientations)
        {
            // Iterate Z
            for (currentZ = minZ; currentZ <= maxZ; currentZ += stepSize)
            {
                // Iterate X
                for (currentX = minX; currentX <= maxX; currentX += stepSize)
                {
                    Debug.Log($"[BatchSimulationManager] Run {runCounter}: Placing sign at ({currentX}, {currentZ}) Yaw: {yaw}");
                    
                    // 1. Reset Global Data
                    dataCollector.ResetForNextRun();
                    
                    // 2. Clear out any existing agents from the previous run
                    ClearExistingAgents();

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

                    // 5. Let the simulation run for the specified duration (scaled time)
                    yield return new WaitForSeconds(simulationDurationPerRun);

                    // 6. Save Data
                    dataCollector.SaveRun(runCounter);
                    runCounter++;
                }
            }
        }

        // Cleanup
        Time.timeScale = 1f;
        isBatching = false;
        Debug.Log("[BatchSimulationManager] Batch simulation completed.");
        
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
