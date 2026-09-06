using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Visibility Area (VCA)
public class VisibilityVolume : MonoBehaviour
{
    [Header("Sign Identification")]
    public string signName = "Sign_Main";

    [Header("Visibility Parameters")]
    public float ThetaDegrees = 90f; // Angle in degrees
    private float Theta; // Angle in radians
    public float ViewingDistance = 15.0f; // View distance (in meters)

    [Header("Target Audience Filtering")]
    [Tooltip("Allowed start nodes for target audience (e.g. 2, 4, 6 for Sign_Main). Leave empty if all agents are target audience.")]
    public List<int> targetStartNodes = new List<int>();

    // Discretization parameters
    [Header("Signage Discretization")]
    public bool useDiscretization = true;
    public float signWidth = 2.0f;
    public float signHeight = 0.5f;
    public float gridStep = 0.1f; // 0.01f is too fine for real-time raycasting
    public float comprehensionTime = 1.0f; // Minimum required human comprehension time (t)
    public bool showDiscreteNodesGizmo = true;
    
    [HideInInspector]
    public List<Vector3> discreteNodes = new List<Vector3>();

    private DataCollector dataCollector; // Reference to the DataCollector

    public bool IsTargetAudience(int startNode)
    {
        if (targetStartNodes == null || targetStartNodes.Count == 0) return true;
        return targetStartNodes.Contains(startNode);
    }

    void Awake()
    {
        if (string.IsNullOrEmpty(signName))
        {
            signName = gameObject.name;
        }

        // Find the DataCollector in the scene
        dataCollector = FindObjectOfType<DataCollector>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (dataCollector == null)
            dataCollector = FindObjectOfType<DataCollector>();

        if (dataCollector != null && dataCollector.dataRecord != null && dataCollector.dataRecord.global != null)
        {
            dataCollector.dataRecord.global.signHeight = transform.position.y;
            dataCollector.dataRecord.global.signPositionX = transform.position.x;
            dataCollector.dataRecord.global.signPositionZ = transform.position.z;
            dataCollector.dataRecord.global.signOrientation = transform.rotation.eulerAngles.y;
            dataCollector.dataRecord.global.vcaDistance = ViewingDistance;
            dataCollector.dataRecord.global.vcaAngle = ThetaDegrees;
            dataCollector.dataRecord.global.signComprehensionTime = comprehensionTime;
        }

        // Initialize the volume parameters
        Theta = ThetaDegrees * Mathf.Deg2Rad; // Convert angle to radians
        
        // Generate discrete nodes for raycasting
        if (useDiscretization)
        {
            GenerateDiscreteNodes();
        }
    }

    public void GenerateDiscreteNodes()
    {
        discreteNodes.Clear();
        
        // Ensure strictly positive step to avoid infinite loops
        if (gridStep <= 0.001f) gridStep = 0.01f;

        // Calculate half dimensions
        float halfWidth = signWidth * 0.5f;
        float halfHeight = signHeight * 0.5f;

        // Determine step count
        int numStepsX = Mathf.CeilToInt(signWidth / gridStep);
        int numStepsY = Mathf.CeilToInt(signHeight / gridStep);
        
        // To precisely center the grid, we will distribute nodes evenly
        // but if width/height is exactly divisible by gridStep, we might need numSteps+1 points to cover the edges
        // Let's iterate from -half to +half
        for (float x = -halfWidth; x <= halfWidth + 0.001f; x += gridStep)
        {
            for (float y = -halfHeight; y <= halfHeight + 0.001f; y += gridStep)
            {
                // Local position
                Vector3 localPos = new Vector3(x, y, 0f);
                // Convert to world position using the sign's transform
                Vector3 worldPos = transform.TransformPoint(localPos);
                discreteNodes.Add(worldPos);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position;
        Vector3 normal = transform.forward.normalized;
        float halfThetaRad = Mathf.Deg2Rad * ThetaDegrees * 0.5f;

        // Draw sphere boundary
        Gizmos.DrawWireSphere(origin, ViewingDistance);

        // Build orthonormal basis u,v perpendicular to n
        Vector3 u = Vector3.Cross(normal, Vector3.up);
        if (u.sqrMagnitude < 1e-6f) u = Vector3.Cross(normal, Vector3.right);
        u.Normalize();
        Vector3 v = Vector3.Cross(normal, u);

        // draw cone rim (circle at angle on sphere)
        DrawConeGizmo(ViewingDistance, halfThetaRad, origin, normal, u, v);
        DrawConeGizmo(-ViewingDistance, halfThetaRad, origin, normal, u, v);

        // Draw discrete nodes grid if enabled
        if (useDiscretization && showDiscreteNodesGizmo)
        {
            Gizmos.color = Color.yellow;
            // Draw a boundary box representing the sign surface
            Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(signWidth, signHeight, 0.01f));
            Gizmos.matrix = oldGizmosMatrix;

            // Draw individual nodes
            Gizmos.color = Color.red;
            if (Application.isPlaying)
            {
                // If playing, use the generated world-space list
                foreach (var node in discreteNodes)
                {
                    Gizmos.DrawSphere(node, 0.02f);
                }
            }
            else
            {
                // In editor mode (not playing), calculate them dynamically so they update in real-time
                float hw = signWidth * 0.5f;
                float hh = signHeight * 0.5f;
                float step = gridStep > 0.01f ? gridStep : 0.1f;
                for (float x = -hw; x <= hw + 0.001f; x += step)
                {
                    for (float y = -hh; y <= hh + 0.001f; y += step)
                    {
                        Vector3 wPos = transform.TransformPoint(new Vector3(x, y, 0f));
                        Gizmos.DrawSphere(wPos, 0.02f);
                    }
                }
            }
        }
    }

    void DrawConeGizmo(float radius, float halfThetaRad, Vector3 p, Vector3 n, Vector3 u, Vector3 v)
    {
        int circleSteps = 36;
        float cosHalf = Mathf.Cos(halfThetaRad);
        Vector3 prev = Vector3.zero;

        for (int i = 0; i <= circleSteps; i++)
        {
            float theta = i / (float)circleSteps * Mathf.PI * 2f;
            // direction: rotate from axis by half-angle
            // point direction = cos(half)*n + sin(half)*(cos(theta)*u + sin(theta)*v)
            Vector3 pointDir = cosHalf * n + Mathf.Sin(halfThetaRad) * (Mathf.Cos(theta) * u + Mathf.Sin(theta) * v);
            Vector3 rim = p + pointDir * radius;
            if (i > 0)
                Gizmos.DrawLine(prev, rim);
            prev = rim;

            // draw lines from apex to rim (sparse)
            if (i % (circleSteps / 8 == 0 ? 1 : circleSteps / 8) == 0)
                Gizmos.DrawLine(p, rim);
        }
    }
}