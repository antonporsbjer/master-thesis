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

    [Header("Sign Tilt & Aperture Configuration")]
    [Tooltip("Downward pitch angle in degrees towards the crowd (positive tilts down).")]
    [Range(-45f, 45f)]
    public float tiltAngle = 0f;

    [Tooltip("Automatically apply tiltAngle to the GameObject's local rotation around X axis.")]
    public bool autoApplyTiltToTransform = true;

    [Tooltip("Optional vertical aperture angle in degrees. If 0, uses ThetaDegrees symmetrically (conical). If > 0, evaluates an elliptical/pyramidal viewing window.")]
    public float ThetaVerticalDegrees = 0f;

    [Tooltip("If false, only agents in front of the sign (oriented towards +Z) can see it. If true, both front and back directions are included.")]
    public bool isDoubleSided = false;

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
    [HideInInspector]
    public List<Vector3> frontDiscreteNodes = new List<Vector3>();
    [HideInInspector]
    public List<Vector3> backDiscreteNodes = new List<Vector3>();

    private DataCollector dataCollector; // Reference to the DataCollector

    public bool IsTargetAudience(int startNode)
    {
        if (targetStartNodes == null || targetStartNodes.Count == 0) return true;
        return targetStartNodes.Contains(startNode);
    }

    public Vector3 GetFrontNormal()
    {
        return transform.forward.normalized;
    }

    public Vector3 GetFrontRight()
    {
        return transform.right.normalized;
    }

    public Vector3 GetFrontUp()
    {
        return transform.up.normalized;
    }

    public Vector3 GetBackNormal()
    {
        Vector3 nFront = transform.forward.normalized;
        Vector3 nHoriz = new Vector3(nFront.x, 0f, nFront.z);
        Vector3 nVert = new Vector3(0f, nFront.y, 0f);
        if (nHoriz.sqrMagnitude > 1e-5f)
        {
            return (-nHoriz + nVert).normalized;
        }
        return -nFront;
    }

    public Vector3 GetBackRight()
    {
        return -transform.right.normalized;
    }

    public Vector3 GetBackUp()
    {
        Vector3 vFront = transform.up.normalized;
        Vector3 vHoriz = new Vector3(vFront.x, 0f, vFront.z);
        Vector3 vVert = new Vector3(0f, vFront.y, 0f);
        if (vHoriz.sqrMagnitude > 1e-5f)
        {
            return (-vHoriz + vVert).normalized;
        }
        return transform.up.normalized;
    }

    public void ApplyTilt()
    {
        Vector3 euler = transform.localEulerAngles;
        euler.x = tiltAngle;
        transform.localEulerAngles = euler;
    }

    public void SyncTiltFromTransform()
    {
        float pitch = transform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
        tiltAngle = pitch;
    }

    private void OnValidate()
    {
        if (autoApplyTiltToTransform)
        {
            ApplyTilt();
        }
        else
        {
            SyncTiltFromTransform();
        }

        Theta = ThetaDegrees * Mathf.Deg2Rad;

        if (useDiscretization)
        {
            GenerateDiscreteNodes();
        }
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
        if (autoApplyTiltToTransform)
        {
            ApplyTilt();
        }
        else
        {
            SyncTiltFromTransform();
        }

        if (dataCollector == null)
            dataCollector = FindObjectOfType<DataCollector>();

        if (dataCollector != null && dataCollector.dataRecord != null && dataCollector.dataRecord.global != null)
        {
            dataCollector.dataRecord.global.signHeight = transform.position.y;
            dataCollector.dataRecord.global.signPositionX = transform.position.x;
            dataCollector.dataRecord.global.signPositionZ = transform.position.z;
            dataCollector.dataRecord.global.signOrientation = transform.rotation.eulerAngles.y;
            dataCollector.dataRecord.global.signTiltAngle = tiltAngle;
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
        frontDiscreteNodes.Clear();
        backDiscreteNodes.Clear();
        
        // Ensure strictly positive step to avoid infinite loops
        if (gridStep <= 0.001f) gridStep = 0.01f;

        // Calculate half dimensions
        float halfWidth = signWidth * 0.5f;
        float halfHeight = signHeight * 0.5f;

        Vector3 origin = transform.position;
        Vector3 uFront = GetFrontRight();
        Vector3 vFront = GetFrontUp();

        Vector3 uBack = GetBackRight();
        Vector3 vBack = GetBackUp();

        for (float x = -halfWidth; x <= halfWidth + 0.001f; x += gridStep)
        {
            for (float y = -halfHeight; y <= halfHeight + 0.001f; y += gridStep)
            {
                // Front face node (tilted down towards oncoming crowd)
                Vector3 wPosFront = origin + x * uFront + y * vFront;
                frontDiscreteNodes.Add(wPosFront);
                discreteNodes.Add(wPosFront);

                if (isDoubleSided)
                {
                    // Reflected back face node (also tilted down towards oncoming crowd)
                    Vector3 wPosBack = origin + x * uBack + y * vBack;
                    backDiscreteNodes.Add(wPosBack);
                    discreteNodes.Add(wPosBack);
                }
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

        Vector3 nFront = GetFrontNormal();
        Vector3 uFront = GetFrontRight();
        Vector3 vFront = GetFrontUp();

        float halfHorizRad = Mathf.Deg2Rad * ThetaDegrees * 0.5f;
        float halfVertRad = (ThetaVerticalDegrees > 0f ? ThetaVerticalDegrees : ThetaDegrees) * Mathf.Deg2Rad * 0.5f;

        // Draw sphere boundary
        Gizmos.DrawWireSphere(origin, ViewingDistance);

        // Draw front cone (tilts down towards crowd in front)
        DrawConeGizmo(ViewingDistance, halfHorizRad, halfVertRad, origin, nFront, uFront, vFront);

        if (isDoubleSided)
        {
            // Draw back cone (reflected so it ALSO tilts down towards crowd in back)
            Vector3 nBack = GetBackNormal();
            Vector3 uBack = GetBackRight();
            Vector3 vBack = GetBackUp();
            DrawConeGizmo(ViewingDistance, halfHorizRad, halfVertRad, origin, nBack, uBack, vBack);
        }

        // Draw discrete nodes grid and sign face outlines if enabled
        if (useDiscretization && showDiscreteNodesGizmo)
        {
            float hw = signWidth * 0.5f;
            float hh = signHeight * 0.5f;

            // Draw front face wireframe
            Gizmos.color = Color.yellow;
            DrawFaceWireframe(origin, hw, hh, uFront, vFront);

            if (isDoubleSided)
            {
                Vector3 uBack = GetBackRight();
                Vector3 vBack = GetBackUp();
                DrawFaceWireframe(origin, hw, hh, uBack, vBack);
            }

            // Draw individual nodes
            Gizmos.color = Color.red;
            if (Application.isPlaying)
            {
                foreach (var node in discreteNodes)
                {
                    Gizmos.DrawSphere(node, 0.02f);
                }
            }
            else
            {
                float step = gridStep > 0.01f ? gridStep : 0.1f;
                Vector3 uBack = isDoubleSided ? GetBackRight() : Vector3.zero;
                Vector3 vBack = isDoubleSided ? GetBackUp() : Vector3.zero;

                for (float x = -hw; x <= hw + 0.001f; x += step)
                {
                    for (float y = -hh; y <= hh + 0.001f; y += step)
                    {
                        Vector3 wPosFront = origin + x * uFront + y * vFront;
                        Gizmos.DrawSphere(wPosFront, 0.02f);

                        if (isDoubleSided)
                        {
                            Vector3 wPosBack = origin + x * uBack + y * vBack;
                            Gizmos.DrawSphere(wPosBack, 0.02f);
                        }
                    }
                }
            }
        }
    }

    void DrawFaceWireframe(Vector3 origin, float hw, float hh, Vector3 u, Vector3 v)
    {
        Vector3 c1 = origin + (-hw * u + -hh * v);
        Vector3 c2 = origin + (hw * u + -hh * v);
        Vector3 c3 = origin + (hw * u + hh * v);
        Vector3 c4 = origin + (-hw * u + hh * v);
        Gizmos.DrawLine(c1, c2);
        Gizmos.DrawLine(c2, c3);
        Gizmos.DrawLine(c3, c4);
        Gizmos.DrawLine(c4, c1);
    }

    void DrawConeGizmo(float radius, float halfHorizRad, float halfVertRad, Vector3 p, Vector3 n, Vector3 u, Vector3 v)
    {
        int circleSteps = 36;
        Vector3 prev = Vector3.zero;

        for (int i = 0; i <= circleSteps; i++)
        {
            float theta = i / (float)circleSteps * Mathf.PI * 2f;
            Vector3 pointDir;
            if (Mathf.Abs(halfHorizRad - halfVertRad) < 1e-4f)
            {
                float cosHalf = Mathf.Cos(halfHorizRad);
                float sinHalf = Mathf.Sin(halfHorizRad);
                pointDir = cosHalf * n + sinHalf * (Mathf.Cos(theta) * u + Mathf.Sin(theta) * v);
            }
            else
            {
                pointDir = n + Mathf.Tan(halfHorizRad) * Mathf.Cos(theta) * u + Mathf.Tan(halfVertRad) * Mathf.Sin(theta) * v;
                pointDir.Normalize();
            }

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