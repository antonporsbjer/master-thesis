using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Vision : MonoBehaviour
{
    private GameObject sign; // Reference to the target GameObject (the sign)
    private VisibilityVolume vca; // Reference to the Visibility Volume component
    private static int nextAgentId = 0; // Static counter for unique IDs
    private int agentId; // Unique identifier for the agent
    private bool hasSeenSign = false; // Track if agent has already seen the sign
    private bool isInVCA = false;     // Track if agent is currently in VCA
    private int timesInVCACounter = 0; // Counter for time spent in the Visibility Area (VCA)
    private float timeStampEnteredVCA = 0.0f; // Timestamp when the VCA was entered
    private float timeStampExitedVCA = 0.0f; // Timestamp when the VCA was exited
    private float timeInVCA = 0.0f; // Time spent in the Visibility Area (VCA)
    private readonly float comprehensionTime = 1.0f; // Comprehension time to consider the sign visible
    private bool isVisible = false; // Flag to check if the sign is visible
    private string agentType; // Type of agent (e.g., "WheelchairAgent", "AdultFemaleAgent", etc.)

    DataCollector dataCollector; // Reference to the DataCollector
    AgentData agentData; // Reference to the AgentData for this agent

    // TODO_ANTON: Let each agent hold a data collector for its own data!

    [Header("Settings")]
    public float checkInterval = 0.2f; // Check 5 times per second by default
    public string targetTag = "sign";
    public LayerMask obstacleMask; // Set this in Inspector!

    private float nextCheckTime = 0f;
    private Agent parentAgent; // specific reference to the Agent script

    public Vector3 axis = Vector3.forward;
    [Range(0f, 180f)] public float angle = 60f; // full angle in degrees
    public float radius = 5f;
    public int circleSteps = 36;
    public Color gizmoColor = Color.cyan;

    public bool IsVisible
    {
        get { return isVisible; }
    }

    void Awake()
    {
        // ensure agentId assigned
        agentId = nextAgentId++;

        // Robustly find parent agent
        parentAgent = GetComponentInParent<Agent>();
        if (parentAgent == null) {
            Debug.LogError($"Vision component on {gameObject.name} could not find an Agent in parent hierarchy!");
        }

        // try to find components if not assigned
        if (sign == null)
            sign = GameObject.FindWithTag(targetTag);
        if (vca == null)
            vca = FindObjectOfType<VisibilityVolume>();

        dataCollector = FindObjectOfType<DataCollector>();
        if(dataCollector != null)
             dataCollector.dataRecord.global.totalAgents++;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get the agent type from the GameObject's name
        // Use robust reference
        if (parentAgent != null) {
            agentType = parentAgent.gameObject.name;
        } else {
             agentType = transform.parent != null ? transform.parent.gameObject.name : gameObject.name;
        }

        // Set the agent ID in the DataCollector
        if (dataCollector != null && parentAgent != null)
        {
            dataCollector.dataRecord.global.signComprehensionTime = comprehensionTime; // Set the comprehension time in the global data
            
            // Safe access to path
            int startNode = -1;
            int goalNode = -1;
            if (parentAgent.path != null && parentAgent.path.Count > 0) {
                startNode = parentAgent.path[0]; 
                goalNode = parentAgent.path[^1];
            }
            
            float agentHeight = 0f;
            var collider = parentAgent.GetComponent<CapsuleCollider>();
            if (collider != null) agentHeight = collider.height;

            float agentEyeHeight = transform.position.y; // Get the agent's eye height
            
            agentData = new AgentData(agentId, agentType, startNode, goalNode, agentHeight, agentEyeHeight);
            dataCollector.dataRecord.agents.Add(agentData);
            
            // Set name for debugging
            parentAgent.gameObject.name = agentType + "_" + agentId;
        }
        else
        {
            if (dataCollector == null) Debug.LogError("DataCollector not found in the scene!");
        }

        // Find the GameObject with the tag and set it as the target
        if (sign == null) // might have been found in Awake
             sign = GameObject.FindWithTag(targetTag);

        if (sign == null)
        {
            Debug.LogError($"No GameObject with tag '{targetTag}' found!");
        }
        else 
        {
            // Get the VisibilityArea component
            vca = sign.GetComponent<VisibilityVolume>();
            if (vca == null) Debug.LogError("No VisibilityVolume component found on the target!");
        }
        
        // Randomize start check time slightly to spread load across frames for thousands of agents
        nextCheckTime = Time.time + Random.Range(0f, checkInterval);
    }

    // Update is called once per frame
    void Update()
    {        
        // THROTTLING: Only run logic if interval has passed
        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + checkInterval;

        bool currentlyInVCA = IsWithinVolume(transform.position);

        // Check for VCA entry
        if (currentlyInVCA && !isInVCA)
        {
            timeStampEnteredVCA = Time.time; // Record the time when the agent enters the VCA
            isInVCA = true; // Mark as currently in VCA
            if (dataCollector != null) dataCollector.dataRecord.global.inVcaCount++; 
        }

        // Check if the agent is currently in the VCA
        if (currentlyInVCA && isInVCA)
        {
            // Check if the sign is visible
            if (!hasSeenSign && IfInVcaAndSignIsVisible(transform.position))
            {
                isVisible = true; // Mark as visible
                hasSeenSign = true; // Mark as seen so it won't count again
            }

            // If the agent has seen the sign, check if it remains visible
            if (hasSeenSign && IfInVcaAndSignIsVisible(transform.position))
            {
                // If the sign is visible, check if comprehension time has passed
                if (timeStampEnteredVCA > 0 && Time.time - timeStampEnteredVCA >= comprehensionTime)
                {
                    isVisible = true; // Mark as visible after comprehension time
                    if (agentData != null) agentData.sawSign = true; 
                }
            }

            // If the agent has seen the sign but it is no longer visible
            if (hasSeenSign && !IfInVcaAndSignIsVisible(transform.position))
            {
                isVisible = false; // Mark as not visible if the sign is not in view
            }
        }

        // Detect exit (transition from inside to outside)
        if (!currentlyInVCA && isInVCA)
        {
            timeStampExitedVCA = Time.time;
            isInVCA = false;

            // Calculate time spent in VCA
            timeInVCA = timeStampExitedVCA - timeStampEnteredVCA;
            if (agentData != null) agentData.timeInVCA = timeInVCA; 

            // Reset visibility and counters
            isVisible = false;
            hasSeenSign = false;
            timesInVCACounter++;
            if (agentData != null) agentData.timesInVCA = timesInVCACounter; 
        }
    }

    // Method to check if the target point is visible from the agent's position
    public bool IfInVcaAndSignIsVisible(Vector3 origin)
    {
        // Defensive checks
        if (sign == null || vca == null)
        {
            return false;
        }

        RaycastHit hit;
        Vector3 directionToSign = (sign.transform.position - origin).normalized;

        // Field of View check (120 degrees)
        // ... (FoV visualization removed/reduced for performance in production, or kept if needed)
        // bool toggleFov = true; 
        float fovAngle = 120f; 
        float halfFov = fovAngle / 2f;
        float angleToSign = Vector3.Angle(transform.forward, directionToSign);

        if (angleToSign > halfFov)
        {
            return false;
        }

        // Use the mask from Inspector
        // Automatically default to Default+Obstacle+Agent if nothing set? 
        // LayerMask defaults to 0 (Nothing) which is bad. 
        // If 0, let's fallback or assume user set it. 
        // Actually LayerMask value 0 is "Default" layer usually? No, it's bitmask. 
        // If mask.value == 0, it means NO layers. We should probably set a default in Awake or use hardcoded if 0.
        int mask = obstacleMask.value;
        if (mask == 0) {
             string[] layerNames = { "Obstacle", "Agent", "Default" };
             mask = LayerMask.GetMask(layerNames);
        }

        if (IsWithinVolume(origin))
        {
            if (Physics.Raycast(origin, directionToSign, out hit, vca.ViewingDistance, mask))
            {
                if (hit.collider.gameObject == sign)
                {
                    //Debug.DrawRay(origin, directionToSign * vca.ViewingDistance, Color.green);
                    return true; 
                }
            }
        }
        return false; 
    }

    // Method to check if a point is within the Visibility Area (VCA)
    private bool IsWithinVolume(Vector3 origin)
    {
        if (vca == null)
            return false;

        // distance check
        float dist = Vector3.Distance(origin, vca.transform.position);
        if (dist > vca.ViewingDistance)
            return false;

        // angle check (support double-sided if you want)
        Vector3 dir = (origin - vca.transform.position).normalized;
        float halfThetaRad = vca.ThetaDegrees * Mathf.Deg2Rad * 0.5f;
        float angle = Vector3.Angle(vca.transform.forward, dir) * Mathf.Deg2Rad;
        if (angle <= halfThetaRad) return true;

        // also allow opposite direction (both sides)
        angle = Vector3.Angle(-vca.transform.forward, dir) * Mathf.Deg2Rad;
        return angle <= halfThetaRad;
    }
}
