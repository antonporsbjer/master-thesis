using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Vision : MonoBehaviour
{
    private static int nextAgentId = 0; // Static counter for unique IDs
    private int agentId; // Unique identifier for the agent
    private string agentType; // Type of agent

    // Discretization parameters
    public float criticalThreshold = 0.23f; // 23% Detection Area Ratio

    DataCollector dataCollector; // Reference to the DataCollector
    AgentData agentData; // Reference to the AgentData for this agent

    public Vector3 axis = Vector3.forward;
    [Range(0f, 180f)] public float angle = 60f; // full angle in degrees
    public float radius = 5f;
    public int circleSteps = 36;
    public Color gizmoColor = Color.cyan;

    private float checkTimer = 0f;
    [Header("Evaluation Frequency")]
    [Tooltip("Time interval in seconds between raycast visibility checks. Lower values increase occlusion accuracy but require more CPU. Set to 0 for per-frame checks.")]
    public float checkInterval = 0.05f; // Default: 20 Hz (0.05s interval) for high occlusion accuracy

    private List<VisibilityVolume> activeVCAs = new List<VisibilityVolume>(); // Track which VCAs we are physically inside

    void Awake()
    {
        agentId = nextAgentId++;
        dataCollector = FindObjectOfType<DataCollector>();
        
        if (dataCollector != null)
        {
            dataCollector.dataRecord.global.totalAgents++;
        }
    }

    void Start()
    {
        // Stagger the evaluation interval to prevent huge frame spikes
        checkTimer = Random.Range(0f, checkInterval);

        // Get the agent type from the GameObject's name
        agentType = transform.parent != null ? transform.parent.gameObject.name : gameObject.name;
        
        // Demographic specific eye level definitions
        float defaultEyeLevel = transform.localPosition.y;
        if (agentType.Contains("Male")) defaultEyeLevel = 1.58f;
        else if (agentType.Contains("Female")) defaultEyeLevel = 1.45f;
        else if (agentType.Contains("Wheelchair")) defaultEyeLevel = 1.17f;
        
        transform.localPosition = new Vector3(transform.localPosition.x, defaultEyeLevel, transform.localPosition.z);

        if (dataCollector != null)
        {
            int startNode = transform.parent != null && transform.parent.GetComponent<Agent>() != null ? transform.parent.GetComponent<Agent>().path[0] : 0;
            int goalNode = transform.parent != null && transform.parent.GetComponent<Agent>() != null ? transform.parent.GetComponent<Agent>().path[^1] : 0;
            float agentHeight = transform.parent != null && transform.parent.GetComponent<CapsuleCollider>() != null ? transform.parent.GetComponent<CapsuleCollider>().height : 1.8f;
            float agentEyeHeight = transform.transform.position.y;
            
            agentData = new AgentData(agentId, agentType, startNode, goalNode, agentHeight, agentEyeHeight);
            
            // Initialize tracking for all currently active signs (supports both batch grid and single-run scenes)
            IEnumerable<VisibilityVolume> targetSigns = (BatchSimulationManager.activeSigns != null && BatchSimulationManager.activeSigns.Count > 0)
                ? BatchSimulationManager.activeSigns
                : FindObjectsOfType<VisibilityVolume>();

            foreach (var signVca in targetSigns)
            {
                if (signVca == null) continue;
                agentData.signTracking[signVca] = new AgentSignData()
                {
                    signPositionX = signVca.transform.position.x,
                    signPositionZ = signVca.transform.position.z,
                    timeInVCA = 0f,
                    timesInVCA = 0,
                    sawSign = false,
                    canSeeSign = false,
                    continuousExposureTime = 0f,
                    isInVCA = false,
                    timeStampEnteredVCA = 0f
                };
            }

            dataCollector.dataRecord.agents.Add(agentData);
        }
        else
        {
            Debug.LogError("DataCollector not found in the scene!");
        }

        if (transform.parent != null)
        {
            transform.parent.gameObject.name = agentType + "_" + agentId;
        }
    }

    void Update()
    {        
        if (agentData == null || agentData.signTracking == null || agentData.signTracking.Count == 0) return;

        // Trace Path Analytics: Nodes Navigated
        Agent myAgent = transform.parent != null ? transform.parent.GetComponent<Agent>() : null;
        if (myAgent != null)
        {
            agentData.totalNodesNavigated = myAgent.path != null ? myAgent.path.Count : 0;
        }

        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            if (checkInterval > 0f) checkTimer -= checkInterval;
            else checkTimer = 0f;

            if (BatchSimulationManager.signGrid != null && BatchSimulationManager.activeSigns != null && BatchSimulationManager.activeSigns.Count > 0)
            {
                // Batch grid spatial search optimization
                float viewDist = BatchSimulationManager.activeSigns[0].ViewingDistance;
                int searchRadius = Mathf.CeilToInt(viewDist / BatchSimulationManager.gridStepSize);

                int agentCellX = Mathf.RoundToInt((transform.position.x - BatchSimulationManager.gridMinX) / BatchSimulationManager.gridStepSize);
                int agentCellZ = Mathf.RoundToInt((transform.position.z - BatchSimulationManager.gridMinZ) / BatchSimulationManager.gridStepSize);

                int minCellX = Mathf.Max(0, agentCellX - searchRadius);
                int maxCellX = Mathf.Min(BatchSimulationManager.gridCols - 1, agentCellX + searchRadius);
                int minCellZ = Mathf.Max(0, agentCellZ - searchRadius);
                int maxCellZ = Mathf.Min(BatchSimulationManager.gridRows - 1, agentCellZ + searchRadius);

                // Iterate over nearby signs mathematically mapped
                for (int z = minCellZ; z <= maxCellZ; z++)
                {
                    for (int x = minCellX; x <= maxCellX; x++)
                    {
                        VisibilityVolume vca = BatchSimulationManager.signGrid[z, x];
                        if (vca == null) continue;
                        EvaluateSignVisibility(vca);
                    }
                }
            }
            else
            {
                // Single run / standard scenario: directly check all signs tracked by this agent
                foreach (var kvp in agentData.signTracking)
                {
                    VisibilityVolume vca = kvp.Key;
                    if (vca == null) continue;
                    EvaluateSignVisibility(vca);
                }
            }

            // 2. Check for exits! Must iterate backwards because we might remove items.
            for (int i = activeVCAs.Count - 1; i >= 0; i--)
            {
                VisibilityVolume vca = activeVCAs[i];
                if (vca == null || !agentData.signTracking.TryGetValue(vca, out AgentSignData signData))
                {
                    activeVCAs.RemoveAt(i);
                    continue;
                }

                bool currentlyInVCA = IsWithinVolume(transform.position, vca);
                if (!currentlyInVCA)
                {
                    float exitTime = Time.time;
                    signData.isInVCA = false;
                    signData.timeInVCA += (exitTime - signData.timeStampEnteredVCA);
                    signData.continuousExposureTime = 0f; 
                    signData.canSeeSign = false;
                    signData.timesInVCA++;
                    activeVCAs.RemoveAt(i);
                }
            }
        }

        // 3. EVERY FRAME: Apply continuous exposure smoothly ONLY for the signs the agent is currently inside
        foreach (var vca in activeVCAs)
        {
            if (vca != null && agentData.signTracking.TryGetValue(vca, out AgentSignData signData))
            {
                if (signData.isInVCA && !signData.sawSign)
                {
                    if (signData.canSeeSign)
                    {
                        signData.continuousExposureTime += Time.deltaTime;
                    }
                    else
                    {
                        signData.continuousExposureTime = 0f;
                    }

                    if (signData.continuousExposureTime >= vca.comprehensionTime)
                    {
                        signData.sawSign = true;
                    }
                }
            }
        }
    }

    private void EvaluateSignVisibility(VisibilityVolume vca)
    {
        if (vca == null || !agentData.signTracking.TryGetValue(vca, out AgentSignData signData)) return;

        bool currentlyInVCA = IsWithinVolume(transform.position, vca);

        if (currentlyInVCA && !signData.isInVCA)
        {
            signData.timeStampEnteredVCA = Time.time;
            signData.isInVCA = true;
            if (!activeVCAs.Contains(vca))
            {
                activeVCAs.Add(vca);
            }
        }

        if (currentlyInVCA && signData.isInVCA)
        {
            if (!signData.sawSign)
            {
                bool canSee = false;
                
                if (vca.useDiscretization)
                {
                    float daRatio = CalculateDARatio(transform.position, vca, signData);
                    if (daRatio >= criticalThreshold) canSee = true;
                }
                else
                {
                    canSee = IfInVcaAndSignIsVisible(transform.position, vca, signData);
                }

                signData.canSeeSign = canSee;
            }
        }
    }

    private void OnDestroy()
    {
        FinalizeActiveVCAs();
    }

    private void OnDisable()
    {
        FinalizeActiveVCAs();
    }

    private void FinalizeActiveVCAs()
    {
        if (agentData != null && activeVCAs != null)
        {
            float exitTime = Time.time;
            foreach (var vca in activeVCAs)
            {
                if (vca != null && agentData.signTracking.TryGetValue(vca, out AgentSignData signData))
                {
                    if (signData.isInVCA)
                    {
                        signData.timeInVCA += (exitTime - signData.timeStampEnteredVCA);
                        signData.isInVCA = false;
                        signData.timesInVCA++;
                    }
                }
            }
            activeVCAs.Clear();
        }
    }

    public float CalculateDARatio(Vector3 origin, VisibilityVolume vca, AgentSignData signData)
    {
        if (vca == null || vca.discreteNodes == null || vca.discreteNodes.Count == 0) return 0f;

        Vector3 directionToSign = (vca.transform.position - origin).normalized;

        // Field of View check (120 degrees)
        float fovAngle = 120f; 
        float halfFovRad = fovAngle * 0.5f * Mathf.Deg2Rad;
        float minCosFov = Mathf.Cos(halfFovRad);

        if (Vector3.Dot(transform.forward, directionToSign) < minCosFov) return 0f;

        string[] layerNames = { "Obstacle", "Agent" };
        int mask = LayerMask.GetMask(layerNames);

        int totalNodes = vca.discreteNodes.Count;
        int visibleNodes = 0;

        foreach (var node in vca.discreteNodes)
        {
            Vector3 directionToNode = (node - origin).normalized;
            
            if (Vector3.Dot(transform.forward, directionToNode) < minCosFov) continue; 

            if (Physics.Raycast(origin, directionToNode, out RaycastHit hit, vca.ViewingDistance, mask))
            {
                if (hit.collider.gameObject == vca.gameObject)
                {
                    visibleNodes++;
                    Color rayColor = signData.sawSign ? Color.green : Color.yellow;
                    Debug.DrawRay(origin, directionToNode * hit.distance, rayColor);
                }
            }
        }

        float daRatio = (float)visibleNodes / totalNodes;
        return daRatio;
    }

    public bool IfInVcaAndSignIsVisible(Vector3 origin, VisibilityVolume vca, AgentSignData signData)
    {
        if (vca == null) return false;

        Vector3 directionToSign = (vca.transform.position - origin).normalized;

        float fovAngle = 120f; 
        float halfFovRad = fovAngle * 0.5f * Mathf.Deg2Rad;
        float minCosFov = Mathf.Cos(halfFovRad);

        if (Vector3.Dot(transform.forward, directionToSign) < minCosFov) return false;

        string[] layerNames = { "Obstacle", "Agent" };
        int mask = LayerMask.GetMask(layerNames);

        if (Physics.Raycast(origin, directionToSign, out RaycastHit hit, vca.ViewingDistance, mask))
        {
            if (hit.collider.gameObject == vca.gameObject)
            {
                Color rayColor = signData.sawSign ? Color.green : Color.yellow;
                Debug.DrawRay(origin, directionToSign * hit.distance, rayColor);
                return true; 
            }
            else
            {
                Debug.DrawRay(origin, directionToSign * hit.distance, Color.red);
            }
        }
        return false;
    }

    private bool IsWithinVolume(Vector3 origin, VisibilityVolume vca)
    {
        if (vca == null) return false;

        Vector3 diff = origin - vca.transform.position;
        float sqrDist = diff.sqrMagnitude;
        if (sqrDist > vca.ViewingDistance * vca.ViewingDistance) return false;

        Vector3 dir = diff.normalized;
        float halfThetaRad = vca.ThetaDegrees * Mathf.Deg2Rad * 0.5f;
        float minCosTheta = Mathf.Cos(halfThetaRad);
        
        // Fast dot product checks against both front and back directions
        if (Mathf.Abs(Vector3.Dot(vca.transform.forward, dir)) >= minCosTheta) return true;

        return false;
    }
}
