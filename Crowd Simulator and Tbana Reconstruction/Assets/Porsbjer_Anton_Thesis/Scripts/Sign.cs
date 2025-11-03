using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Visibility Area (VCA)
public class VisibilityVolume : MonoBehaviour
{
    //[Range(0, 180)]
    public float ThetaDegrees = 90f; // Angle in degrees
    private float Theta; // Angle in radians
    public float ViewingDistance = 15.0f; // View distance (in meters)
    public bool RandomPosition = false; // If true, the sign will be placed at a random position within the volume
    private DataCollector dataCollector; // Reference to the DataCollector

    // Range parameters for random sign placement (set these in the Inspector)
    public float signMinX = -5f;
    public float signMaxX = 5f;
    public float signMinZ = -5f;
    public float signMaxZ = 5f;
    public float randomYawMin = 0f;
    public float randomYawMax = 90f;

    void Awake()
    {
        // Find the DataCollector in the scene
        dataCollector = FindObjectOfType<DataCollector>();

        // perform initial randomization (if enabled)
        RandomizePosition();
    }

    // public helper to (re)randomize sign position & yaw
    public void RandomizePosition()
    {
        if (!RandomPosition) return;

        // pick only 0 or 90 degrees
        float yaw = (Random.value < 0.5f) ? 0f : 90f;

        // random position within bounds
        float x = Random.Range(signMinX, signMaxX);
        float z = Random.Range(signMinZ, signMaxZ);

        transform.SetPositionAndRotation(new Vector3(x, transform.position.y, z), Quaternion.Euler(0f, yaw, 0f));
    }

    // Start is called before the first frame update
    void Start()
    {
        dataCollector.dataRecord.global.signHeight = transform.position.y; // Set the sign height in the global data
        dataCollector.dataRecord.global.signPositionX = transform.position.x; // Set the sign X position in the global data
        dataCollector.dataRecord.global.signPositionZ = transform.position.z; // Set the sign Z position in the global data
        dataCollector.dataRecord.global.vcaDistance = ViewingDistance; // Set the viewing distance in the global data
        dataCollector.dataRecord.global.vcaAngle = ThetaDegrees; // Set the viewing angle in the global data

        // Initialize the volume parameters
        Theta = ThetaDegrees * Mathf.Deg2Rad; // Convert angle to radians
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