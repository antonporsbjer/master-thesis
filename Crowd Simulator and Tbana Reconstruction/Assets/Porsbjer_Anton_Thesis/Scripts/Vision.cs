using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Vision : MonoBehaviour
{
    private GameObject sign; // Reference to the target GameObject (the sign)
    private VisibilityArea vca; // Reference to the Visibility Area (VCA) component
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

    public bool IsVisible
    {
        get { return isVisible; }
    }

    void Awake()
    {
        // Initialize the DataCollector reference
        dataCollector = FindObjectOfType<DataCollector>();
        agentId = nextAgentId++;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get the agent type from the GameObject's name
        // Get the parent GameObject's name (or root if you want the topmost parent)
        agentType = transform.parent != null ? transform.parent.gameObject.name : gameObject.name;
        Debug.Log("Agent Type: " + agentType + ", ID: " + agentId);

        // Set the agent ID in the DataCollector
        if (dataCollector != null)
        {
            int startNode = transform.parent.GetComponent<Agent>().path[0]; // Assuming the first node in the path is the start node
            int goalNode = transform.parent.GetComponent<Agent>().path[^1]; // Assuming the last node in the path is the goal node
            agentData = new AgentData(agentId, agentType, startNode, goalNode);
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
        vca = sign.GetComponent<VisibilityArea>();

        if (vca == null)
        {
            Debug.LogError("No VisibilityArea component found on the target!");
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
            Debug.Log(agentType + ", ID: " + agentId + ", entered VCA at: " + timeStampEnteredVCA);
        }

        // Check if the agent is currently in the VCA
        if (currentlyInVCA && isInVCA)
        {
            // Time spent in VCA
            //Debug.Log(agentType + ", ID: " + agentId + ", time in VCA: " + (Time.time - timeStampEnteredVCA));

            // Check if the sign is visible
            if (!hasSeenSign && IfInVcaAndSignIsVisible(transform.position))
            {
                isVisible = true; // Mark as visible
                hasSeenSign = true; // Mark as seen so it won't count again
                Debug.Log(agentType + ", ID: " + agentId + ", can see the sign.");
            }

            // If the agent has seen the sign, check if it remains visible
            if (hasSeenSign && IfInVcaAndSignIsVisible(transform.position))
            {
                // If the sign is visible, check if comprehension time has passed
                if (timeStampEnteredVCA > 0 && Time.time - timeStampEnteredVCA >= comprehensionTime)
                {
                    isVisible = true; // Mark as visible after comprehension time
                    agentData.sawSign = true; // Mark that the agent saw the sign
                    Debug.Log(agentType + ", ID: " + agentId + ", can see the sign after comprehension time.");
                }
            }

            // If the agent has seen the sign but it is no longer visible
            if (hasSeenSign && !IfInVcaAndSignIsVisible(transform.position))
            {
                isVisible = false; // Mark as not visible if the sign is not in view
                Debug.Log(agentType + ", ID: " + agentId + ", cannot see the sign anymore.");
            }
        }

        // Detect exit (transition from inside to outside)
        if (!currentlyInVCA && isInVCA)
        {
            timeStampExitedVCA = Time.time;
            isInVCA = false;
            Debug.Log(agentType + ", ID: " + agentId + ", exited VCA at: " + timeStampExitedVCA);

            // Calculate time spent in VCA
            timeInVCA = timeStampExitedVCA - timeStampEnteredVCA;
            agentData.timeInVCA = timeInVCA; // Store the time spent in VCA in the AgentData
            Debug.Log(agentType + ", ID: " + agentId + ", time spent in VCA: " + timeInVCA);

            // Reset visibility and counters
            isVisible = false;
            hasSeenSign = false;
            timesInVCACounter++;
            // TODO_ANTON: Count the number of times the agent has been in the VCA
            Debug.Log(agentType + ", ID: " + agentId + ", has been in the VCA " + timesInVCACounter + " times.");
        }
    }

    // Method to check if the target point is visible from the agent's position
    public bool IfInVcaAndSignIsVisible(Vector3 origin)
    {
        RaycastHit hit;
        Vector3 direction = sign.transform.position - origin;


        int mask = LayerMask.GetMask("Obstacle"); // TODO_ANTON: Add later to test whit agent collision

        if (IsWithinVolume(origin))
        {
            Debug.Log(agentType + ", ID: " + agentId + ", is within the Visibility Area (VCA) of the sign.");
            if (Physics.Raycast(origin, direction, out hit, vca.ViewingDistance, mask))
            {
                // Draw the ray if agent is within the Visibility Area (VCA) but not hitting the sign
                Debug.DrawRay(origin, direction, Color.red);

                if (hit.collider.gameObject == sign)
                {
                    // Log the hit information
                    Debug.Log(agentType + ", ID: " + agentId + ", raycast hit: " + hit.collider.gameObject.name);
                    // Check if the ray is within the Visibility Area (VCA) and hitting the sign
                    Debug.DrawRay(origin, direction, Color.green);
                    return true; // Ray hit the target within the volume
                }
            }
        }
        return false; // Ray did not hit the target or was outside the volume
    }

    // Method to check if a point is within the Visibility Area (VCA)
    private bool IsWithinVolume(Vector3 origin)
    {
        Vector3 direction = (origin - vca.transform.position).normalized;
        float dotProduct = Vector3.Dot(direction, vca.transform.forward.normalized);
        float angle = Mathf.Acos(dotProduct);
        float distance = Vector3.Distance(origin, vca.transform.position);

        // Check if the origin is within the cone and sphere
        return angle <= vca.ThetaDegrees * Mathf.Deg2Rad / 2 && distance <= vca.ViewingDistance;
    }
}
