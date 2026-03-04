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
    private float comprehensionTime = 1.0f; // Comprehension time to consider the sign visible
    private bool isVisible = false; // Flag to check if the sign is visible
    private string agentType; // Type of agent (e.g., "WheelchairAgent", "AdultFemaleAgent", etc.)
    private HashSet<int> nodesSeenSign = new HashSet<int>(); // Specific subset of nodes where effective signage detection occurred

    // Discretization parameters
    public float criticalThreshold = 0.23f; // 23% Detection Area Ratio
    private float continuousExposureTime = 0f; // Time the DA ratio constraint is met continuously

    DataCollector dataCollector; // Reference to the DataCollector
    AgentData agentData; // Reference to the AgentData for this agent

    // TODO_ANTON: Let each agent hold a data collector for its own data!

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

        // try to find components if not assigned
        if (sign == null)
            sign = GameObject.FindWithTag("sign");
        if (vca == null)
            vca = FindObjectOfType<VisibilityVolume>();

        dataCollector = FindObjectOfType<DataCollector>();
        dataCollector.dataRecord.global.totalAgents++; // Increment total agents in DataCollector
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get the agent type from the GameObject's name
        // Get the parent GameObject's name (or root if you want the topmost parent)
        agentType = transform.parent != null ? transform.parent.gameObject.name : gameObject.name;
        
        // Demographic specific eye level definitions
        float defaultEyeLevel = transform.localPosition.y;
        if (agentType.Contains("Male")) defaultEyeLevel = 1.58f;
        else if (agentType.Contains("Female")) defaultEyeLevel = 1.45f;
        else if (agentType.Contains("Wheelchair")) defaultEyeLevel = 1.17f;
        
        transform.localPosition = new Vector3(transform.localPosition.x, defaultEyeLevel, transform.localPosition.z);
        // Debug.Log("Agent Type: " + agentType + ", ID: " + agentId);

        // Set the agent ID in the DataCollector
        if (dataCollector != null)
        {
            dataCollector.dataRecord.global.signComprehensionTime = comprehensionTime; // Set the comprehension time in the global data
            int startNode = transform.parent.GetComponent<Agent>().path[0]; // Assuming the first node in the path is the start node
            int goalNode = transform.parent.GetComponent<Agent>().path[^1]; // Assuming the last node in the path is the goal node
            float agentHeight = transform.parent.GetComponent<CapsuleCollider>().height; // Get the agent's height
            float agentEyeHeight = transform.transform.position.y; // Get the agent's eye height
            agentData = new AgentData(agentId, agentType, startNode, goalNode, agentHeight, agentEyeHeight);
            dataCollector.dataRecord.agents.Add(agentData);
        }
        else
        {
            Debug.LogError("DataCollector not found in the scene!");
        }

        // Set the parent GameObject's name as the agent type and ID
        if (transform.parent != null)
        {
            transform.parent.gameObject.name = agentType + "_" + agentId;
        }


        // Find the GameObject with the tag "sign" and set it as the target
        sign = GameObject.FindWithTag("sign");

        if (sign == null)
        {
            Debug.LogError("No GameObject with tag 'sign' found!");
        }

        // Get the VisibilityArea component
        vca = sign.GetComponent<VisibilityVolume>();

        if (vca == null)
        {
            Debug.LogError("No VisibilityArea component found on the target!");
        }
        else
        {
            comprehensionTime = vca.comprehensionTime;
        }
    }

    // Update is called once per frame
    void Update()
    {        
        bool currentlyInVCA = IsWithinVolume(transform.position);

        // Check for VCA entry
        if (currentlyInVCA && !isInVCA)
        {
            timeStampEnteredVCA = Time.time; // Record the time when the agent enters the VCA
            isInVCA = true; // Mark as currently in VCA
            dataCollector.dataRecord.global.inVcaCount++; // Increment the inVCA count in the DataCollector
            // Debug.Log(agentType + ", ID: " + agentId + ", entered VCA at: " + timeStampEnteredVCA);
        }

        // Check if the agent is currently in the VCA
        if (currentlyInVCA && isInVCA)
        {
            // Time spent in VCA
            //Debug.Log(agentType + ", ID: " + agentId + ", time in VCA: " + (Time.time - timeStampEnteredVCA));

            // Check if the sign is visible
            if (!hasSeenSign)
            {
                bool canSeeSign = false;
                
                // If using discretization, check DARatio. Otherwise, fall back to single raycast
                if (vca.useDiscretization)
                {
                    float daRatio = CalculateDARatio(transform.position);
                    if (daRatio >= criticalThreshold)
                    {
                        canSeeSign = true;
                    }
                }
                else
                {
                    canSeeSign = IfInVcaAndSignIsVisible(transform.position);
                }

                if (canSeeSign)
                {
                    isVisible = true; // Mark as visible
                    continuousExposureTime += Time.deltaTime; // Increment exposure timer
                    // Debug.Log(agentType + ", ID: " + agentId + ", can see the sign.");
                }
                else
                {
                    // Reset continuous exposure time if the sight is broken
                    continuousExposureTime = 0f; 
                    isVisible = false;
                }
            }

            // If the sign is visible, check if comprehension time has passed
            // We use the new continuousExposureTime variable
            if (!hasSeenSign && continuousExposureTime >= comprehensionTime)
            {
                isVisible = true; // Mark as visible after comprehension time
                hasSeenSign = true; // Mark as seen so it won't count again
                agentData.sawSign = true; // Mark that the agent saw the sign
                // Debug.Log(agentType + ", ID: " + agentId + ", can see the sign after comprehension time.");
            }

            // If the agent has seen the sign but it is no longer visible
            if (hasSeenSign && !IfInVcaAndSignIsVisible(transform.position))
            {
                isVisible = false; // Mark as not visible if the sign is not in view
                // Debug.Log(agentType + ", ID: " + agentId + ", cannot see the sign anymore.");
            }
        }

        // Trace Path Analytics: Nodes Navigated & Effective Detection Nodes
        Agent myAgent = transform.parent != null ? transform.parent.GetComponent<Agent>() : null;
        if (myAgent != null && agentData != null)
        {
            agentData.totalNodesNavigated = myAgent.path != null ? myAgent.path.Count : 0;
            if (isVisible && myAgent.path != null && myAgent.pathIndex < myAgent.path.Count)
            {
                nodesSeenSign.Add(myAgent.path[myAgent.pathIndex]);
            }
            agentData.nodesWithDetection = nodesSeenSign.Count;
        }

        // Detect exit (transition from inside to outside)
        if (!currentlyInVCA && isInVCA)
        {
            timeStampExitedVCA = Time.time;
            isInVCA = false;
            // Debug.Log(agentType + ", ID: " + agentId + ", exited VCA at: " + timeStampExitedVCA);

            // Calculate time spent in VCA
            timeInVCA = timeStampExitedVCA - timeStampEnteredVCA;
            agentData.timeInVCA = timeInVCA; // Store the time spent in VCA in the AgentData
            // Debug.Log(agentType + ", ID: " + agentId + ", time spent in VCA: " + timeInVCA);

            // Reset visibility and counters
            isVisible = false;
            // hasSeenSign = false; <- Keep this true if we want to log if they EVER saw it
            continuousExposureTime = 0f; // Reset exposure timer upon exiting VCA
            timesInVCACounter++;
            agentData.timesInVCA = timesInVCACounter; // Store the number of times in VCA in the AgentData
            // Debug.Log(agentType + ", ID: " + agentId + ", has been in the VCA " + timesInVCACounter + " times.");
        }
    }

    // Method to calculate the Detection Area Ratio based on discrete nodes
    public float CalculateDARatio(Vector3 origin)
    {
        if (sign == null || vca == null || vca.discreteNodes == null || vca.discreteNodes.Count == 0)
        {
            return 0f;
        }

        Vector3 directionToSign = (sign.transform.position - origin).normalized;

        // Field of View check (120 degrees)
        float fovAngle = 120f; 
        float halfFov = fovAngle / 2f;
        float angleToSign = Vector3.Angle(transform.forward, directionToSign);

        if (angleToSign > halfFov)
        {
            return 0f; // Outside Field of View
        }

        if (!IsWithinVolume(origin))
        {
            return 0f; // Outside VCA
        }

        string[] layerNames = { "Obstacle", "Agent" };
        int mask = LayerMask.GetMask(layerNames);

        int totalNodes = vca.discreteNodes.Count;
        int visibleNodes = 0;

        foreach (var node in vca.discreteNodes)
        {
            Vector3 directionToNode = (node - origin).normalized;
            
            // Re-check FOV for the specific node just in case the sign is wide 
            // and the edges fall out of the FOV.
            if (Vector3.Angle(transform.forward, directionToNode) > halfFov)
            {
                continue; 
            }

            RaycastHit hit;
            if (Physics.Raycast(origin, directionToNode, out hit, vca.ViewingDistance, mask))
            {
                if (hit.collider.gameObject == sign)
                {
                    visibleNodes++;
                    Color rayColor = hasSeenSign ? Color.green : Color.yellow;
                    Debug.DrawRay(origin, directionToNode * hit.distance, rayColor);
                }
                else
                {
                     // Debug.DrawRay(origin, directionToNode * hit.distance, Color.red); // Hit an obstacle/agent
                }
            }
        }

        float daRatio = (float)visibleNodes / totalNodes;
        return daRatio;
    }

    // Method to check if the target point is visible from the agent's position
    public bool IfInVcaAndSignIsVisible(Vector3 origin)
    {
        // Defensive checks
        if (sign == null || vca == null)
        {
            // missing required objects, treat as not visible
            return false;
        }

        RaycastHit hit;
        Vector3 directionToSign = (sign.transform.position - origin).normalized;

        // Field of View check (120 degrees)
        bool toggleFov = true; // Toggle for Field of View (FoV)
        float fovAngle = 120f; // Field of View angle in degrees
        float halfFov = fovAngle / 2f;
        float angleToSign = Vector3.Angle(transform.forward, directionToSign);

        // Visualize the Field of View (FoV) cone
        if (toggleFov)
        {
            Debug.DrawRay(origin, transform.forward * vca.ViewingDistance, Color.blue);
            Debug.DrawRay(origin, Quaternion.Euler(0, -halfFov, 0) * transform.forward * vca.ViewingDistance, Color.blue);
            Debug.DrawRay(origin, Quaternion.Euler(0, halfFov, 0) * transform.forward * vca.ViewingDistance, Color.blue);
            
            // Draw a red ray specifically to the sign (stops at the sign)
            float distToSign = Vector3.Distance(origin, sign.transform.position);
            Debug.DrawRay(origin, directionToSign * distToSign, Color.red);
        }

        if (toggleFov && angleToSign > halfFov)
        {
            // Sign is outside the FoV cone
            // Debug.Log(agentType + ", ID: " + agentId + ", sign is outside FoV.");
            return false;
        }

        string[] layerNames = { "Obstacle", "Agent" };
        int mask = LayerMask.GetMask(layerNames);

        if (IsWithinVolume(origin))
        {
            // Debug.Log(agentType + ", ID: " + agentId + ", is within the Visibility Area (VCA) of the sign.");
            if (Physics.Raycast(origin, directionToSign, out hit, vca.ViewingDistance, mask))
            {
                if (hit.collider.gameObject == sign)
                {
                    // Log the hit information
                    // Debug.Log(agentType + ", ID: " + agentId + ", raycast hit: " + hit.collider.gameObject.name);
                    // Check if the ray is within the Visibility Area (VCA) and hitting the sign
                    Color rayColor = hasSeenSign ? Color.green : Color.yellow;
                    Debug.DrawRay(origin, directionToSign * hit.distance, rayColor);
                    return true; // Ray hit the target within the volume
                }
                else
                {
                    // Draw the ray if agent is within the Visibility Area (VCA) but hits something else
                    Debug.DrawRay(origin, directionToSign * hit.distance, Color.red);
                }
            }
        }
        return false; // Ray did not hit the target or was outside the volume
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
