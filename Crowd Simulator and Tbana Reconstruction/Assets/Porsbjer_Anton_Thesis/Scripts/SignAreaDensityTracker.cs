using System.Collections.Generic;
using UnityEngine;

public class SignAreaDensityTracker : MonoBehaviour
{
    public static SignAreaDensityTracker instance;

    public enum MeasurementMode
    {
        SignVCA,        // Exact viewing cone/sector of the sign (d = viewing distance, theta = aperture)
        RadialRadius,   // Circular radius around the sign position
        BoxArea         // Rectangular concourse corridor in front of the sign
    }

    [Header("Target Sign")]
    [Tooltip("Target sign to measure density around. If null, automatically detected.")]
    public VisibilityVolume targetSign;

    [Header("Measurement Area Configuration")]
    public MeasurementMode mode = MeasurementMode.SignVCA;

    [Tooltip("Radius in meters if using RadialRadius mode.")]
    public float customRadius = 10.0f;

    [Tooltip("Box size (X = width, Z = depth) if using BoxArea mode.")]
    public Vector2 boxDimensions = new Vector2(10f, 15f);

    [Header("Sampling Settings")]
    [Tooltip("Sampling interval in seconds (e.g., 0.1s = 10 Hz). Set 0 to sample every frame.")]
    public float sampleInterval = 0.1f;

    [Header("Live Metrics (Read-Only)")]
    [SerializeField] private int currentAgentCount = 0;
    [SerializeField] private float currentDensity = 0f;      // Instantaneous agents / m^2
    [SerializeField] private float averageDensity = 0f;      // Time-weighted average agents / m^2
    [SerializeField] private float peakDensity = 0f;         // Peak observed agents / m^2
    [SerializeField] private float totalDuration = 0f;       // Total sampling time in seconds
    [SerializeField] private float measuredAreaM2 = 0f;      // Area in m^2

    public int CurrentAgentCount => currentAgentCount;
    public float CurrentDensity => currentDensity;
    public float AverageDensity => averageDensity;
    public float PeakDensity => peakDensity;
    public float MeasuredAreaM2 => measuredAreaM2;
    public float TotalDuration => totalDuration;

    private Main mainScript;
    private float sampleTimer = 0f;
    private float accumulatedDensityTime = 0f;

    void Awake()
    {
        if (instance == null) instance = this;
        LocateTargetSign();
        CalculateArea();
    }

    void Start()
    {
        LocateTargetSign();
        CalculateArea();
        ResetMetrics();
    }

    public void LocateTargetSign()
    {
        if (targetSign != null) return;

        // 1. Check if attached directly to a sign
        targetSign = GetComponent<VisibilityVolume>();
        if (targetSign != null) return;

        // 2. Search scene for signs, preferring Sign_Hotel (Scenario C default)
        VisibilityVolume[] allSigns = FindObjectsOfType<VisibilityVolume>();
        if (allSigns != null && allSigns.Length > 0)
        {
            foreach (var s in allSigns)
            {
                if (s != null && s.signName.Contains("Hotel"))
                {
                    targetSign = s;
                    return;
                }
            }
            targetSign = allSigns[0];
        }
    }

    public void CalculateArea()
    {
        switch (mode)
        {
            case MeasurementMode.SignVCA:
                if (targetSign != null)
                {
                    // Sector area = (Theta / 360) * pi * R^2
                    measuredAreaM2 = (targetSign.ThetaDegrees / 360f) * Mathf.PI * Mathf.Pow(targetSign.ViewingDistance, 2);
                }
                else
                {
                    // Default fallback matching Sign_Hotel: 90 deg, 15m
                    measuredAreaM2 = (90f / 360f) * Mathf.PI * Mathf.Pow(15.0f, 2);
                }
                break;

            case MeasurementMode.RadialRadius:
                measuredAreaM2 = Mathf.PI * Mathf.Pow(customRadius, 2);
                break;

            case MeasurementMode.BoxArea:
                measuredAreaM2 = boxDimensions.x * boxDimensions.y;
                break;
        }

        if (measuredAreaM2 <= 0.001f)
        {
            measuredAreaM2 = 1.0f;
        }
    }

    public void ResetMetrics()
    {
        accumulatedDensityTime = 0f;
        totalDuration = 0f;
        sampleTimer = 0f;
        currentAgentCount = 0;
        currentDensity = 0f;
        averageDensity = 0f;
        peakDensity = 0f;
        CalculateArea();
    }

    void Update()
    {
        if (mainScript == null)
        {
            mainScript = Main.instance != null ? Main.instance : FindObjectOfType<Main>();
        }

        if (targetSign == null)
        {
            LocateTargetSign();
            CalculateArea();
            if (targetSign == null) return;
        }

        sampleTimer += Time.deltaTime;
        if (sampleInterval <= 0f || sampleTimer >= sampleInterval)
        {
            float dt = sampleTimer;
            sampleTimer = 0f;

            currentAgentCount = CountAgentsInZone();
            currentDensity = currentAgentCount / measuredAreaM2;

            if (currentDensity > peakDensity)
            {
                peakDensity = currentDensity;
            }

            accumulatedDensityTime += currentDensity * dt;
            totalDuration += dt;

            if (totalDuration > 0f)
            {
                averageDensity = accumulatedDensityTime / totalDuration;
            }
        }
    }

    private int CountAgentsInZone()
    {
        List<Agent> agents = null;
        if (mainScript != null && mainScript.agentList != null)
        {
            agents = mainScript.agentList;
        }

        if (agents == null || agents.Count == 0)
        {
            return 0;
        }

        int count = 0;
        Vector3 signPos = targetSign.transform.position;
        Vector3 signForward = targetSign.transform.forward;
        signForward.y = 0f;
        signForward.Normalize();

        for (int i = 0; i < agents.Count; i++)
        {
            Agent agent = agents[i];
            if (agent == null) continue;

            Vector3 agentPos = agent.transform.position;
            Vector3 toAgent = agentPos - signPos;
            toAgent.y = 0f; // 2D ground projection

            switch (mode)
            {
                case MeasurementMode.SignVCA:
                    float dist = toAgent.magnitude;
                    if (dist <= targetSign.ViewingDistance)
                    {
                        float angle = Vector3.Angle(signForward, toAgent);
                        if (angle <= targetSign.ThetaDegrees * 0.5f)
                        {
                            count++;
                        }
                    }
                    break;

                case MeasurementMode.RadialRadius:
                    if (toAgent.sqrMagnitude <= customRadius * customRadius)
                    {
                        count++;
                    }
                    break;

                case MeasurementMode.BoxArea:
                    Vector3 localPos = targetSign.transform.InverseTransformPoint(agentPos);
                    if (localPos.z >= 0f && localPos.z <= boxDimensions.y &&
                        Mathf.Abs(localPos.x) <= boxDimensions.x * 0.5f)
                    {
                        count++;
                    }
                    break;
            }
        }

        return count;
    }

    private void OnDrawGizmos()
    {
        if (targetSign == null)
        {
            LocateTargetSign();
            if (targetSign == null) return;
        }

        Vector3 pos = targetSign.transform.position;
        pos.y = 0.05f; // Project onto floor

        Gizmos.color = new Color(1f, 0.55f, 0f, 0.6f);

        switch (mode)
        {
            case MeasurementMode.RadialRadius:
                Gizmos.DrawWireSphere(pos, customRadius);
                break;

            case MeasurementMode.BoxArea:
                Matrix4x4 oldMat = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(pos + targetSign.transform.forward * (boxDimensions.y * 0.5f), targetSign.transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(boxDimensions.x, 0.1f, boxDimensions.y));
                Gizmos.matrix = oldMat;
                break;

            case MeasurementMode.SignVCA:
                float r = targetSign.ViewingDistance;
                float halfAngle = targetSign.ThetaDegrees * 0.5f;
                Vector3 fwd = targetSign.transform.forward;
                fwd.y = 0;
                fwd.Normalize();

                Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * fwd * r;
                Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * fwd * r;

                Gizmos.DrawLine(pos, pos + leftDir);
                Gizmos.DrawLine(pos, pos + rightDir);

                int arcSegments = 24;
                Vector3 prevPoint = pos + leftDir;
                for (int i = 1; i <= arcSegments; i++)
                {
                    float currentAngle = -halfAngle + (targetSign.ThetaDegrees * i / arcSegments);
                    Vector3 nextDir = Quaternion.Euler(0, currentAngle, 0) * fwd * r;
                    Vector3 nextPoint = pos + nextDir;
                    Gizmos.DrawLine(prevPoint, nextPoint);
                    prevPoint = nextPoint;
                }
                break;
        }

#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.yellow;
        string info = $"[Sign Density: {targetSign.signName}]\n" +
                      $"Active Agents: {currentAgentCount}\n" +
                      $"Instant Density: {currentDensity:F2} ped/m²\n" +
                      $"Run Average: {averageDensity:F2} ped/m²\n" +
                      $"Peak: {peakDensity:F2} ped/m²\n" +
                      $"Area: {measuredAreaM2:F1} m²";
        UnityEditor.Handles.Label(pos + Vector3.up * 1.5f, info);
#endif
    }
}
