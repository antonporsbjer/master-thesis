using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Visibility Area (VCA)
public class VisibilityArea : MonoBehaviour
{
    //[Range(0, 180)]
    public float ThetaDegrees = 90f; // Angle in degrees
    private float Theta; // Angle in radians
    public float ViewingDistance = 15.0f; // View distance (in meters)
    public GameObject signPositionBoundary; // Boundary object to define the area within which the sign can be placed
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

        if (RandomPosition)
        {
            // Random 90 degree angle
            float angle = Random.Range(0, 1) * 90 * Mathf.Deg2Rad;

            // Random x and z within the configured ranges
            float x = Random.Range(signMinX, signMaxX);
            float z = Random.Range(signMinZ, signMaxZ);

            transform.SetPositionAndRotation(new Vector3(x, transform.position.y, z), Quaternion.Euler(0f,  angle * Mathf.Rad2Deg, 0f));
        }

        // If a boundary object is assigned, add a tiny helper component to draw its bounds as a gizmo.
        if (signPositionBoundary != null && signPositionBoundary.GetComponent<BoundaryGizmoDrawer>() == null)
        {
            signPositionBoundary.AddComponent<BoundaryGizmoDrawer>().Initialize(Color.cyan);
        }
    }

    // Helper component that draws the bounding area for a GameObject (uses Collider/Renderer/children)
    private class BoundaryGizmoDrawer : MonoBehaviour
    {
        private Color gizmoColor = Color.cyan;

        public void Initialize(Color color)
        {
            gizmoColor = color;
        }

        void OnDrawGizmos()
        {
            Bounds bounds = new Bounds(transform.position, Vector3.zero);
            bool hasBounds = false;

            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                bounds = col.bounds;
                hasBounds = true;
            }
            else
            {
                Renderer rend = GetComponent<Renderer>();
                if (rend != null)
                {
                    bounds = rend.bounds;
                    hasBounds = true;
                }
                else
                {
                    Renderer[] rends = GetComponentsInChildren<Renderer>();
                    if (rends.Length > 0)
                    {
                        bounds = rends[0].bounds;
                        for (int i = 1; i < rends.Length; i++) bounds.Encapsulate(rends[i].bounds);
                        hasBounds = true;
                    }
                    else
                    {
                        Collider[] cols = GetComponentsInChildren<Collider>();
                        if (cols.Length > 0)
                        {
                            bounds = cols[0].bounds;
                            for (int i = 1; i < cols.Length; i++) bounds.Encapsulate(cols[i].bounds);
                            hasBounds = true;
                        }
                    }
                }
            }

            if (!hasBounds) return;

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            // Optionally draw a filled semi-transparent cube for clarity in editor (uncomment if wanted)
            // Color prev = Gizmos.color;
            // Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.05f);
            // Gizmos.DrawCube(bounds.center, bounds.size);
            // Gizmos.color = prev;
        }
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
        // Update the angle in radians
        Theta = ThetaDegrees * Mathf.Deg2Rad; // Convert angle to radians
        // Check for collisions
        // CheckCollisions();
    }

    // Method to check for collisions
    // void CheckCollisions()
    // {
    //     Collider[] colliders = Physics.OverlapSphere(transform.position, ViewingDistance);
    //     foreach (Collider collider in colliders)
    //     {
    //         if (collider.gameObject.CompareTag("eye"))
    //         {
    //             Vector3 vi = collider.transform.position;
    //             Vector3 direction = (vi - transform.position).normalized;
    //             float dotProduct = Vector3.Dot(direction, transform.forward.normalized);
    //             float angle = Mathf.Acos(dotProduct);
    //             float distance = Vector3.Distance(vi, transform.position);

    //             // Check if the collider is within the cone and sphere
    //             if (angle <= Theta / 2 && distance <= ViewingDistance)
    //             {
    //                 GameObject target = collider.gameObject;
    //                 // Debug.Log("Collision detected with: " + target.name);
    //             }
    //         }
    //     }
    // }

    // Method to visualize the volume in the Scene view
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, ViewingDistance);

        // Draw the cone
        Vector3 right = Vector3.Cross(Vector3.up, transform.forward.normalized).normalized;
        Vector3 up = Vector3.Cross(transform.forward.normalized, right).normalized;

        float halfAngle = Theta / 2;
        float coneHeight = ViewingDistance * Mathf.Cos(halfAngle);
        float coneRadius = ViewingDistance * Mathf.Sin(halfAngle);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward.normalized * coneHeight);
        Gizmos.DrawLine(transform.position, transform.position + (transform.forward.normalized * coneHeight + right * coneRadius));
        Gizmos.DrawLine(transform.position, transform.position + (transform.forward.normalized * coneHeight - right * coneRadius));
        Gizmos.DrawLine(transform.position, transform.position + (transform.forward.normalized * coneHeight + up * coneRadius));
        Gizmos.DrawLine(transform.position, transform.position + (transform.forward.normalized * coneHeight - up * coneRadius));

        Gizmos.DrawLine(transform.position, transform.position - transform.forward.normalized * coneHeight);
        Gizmos.DrawLine(transform.position, transform.position - (transform.forward.normalized * coneHeight + right * coneRadius));
        Gizmos.DrawLine(transform.position, transform.position - (transform.forward.normalized * coneHeight - right * coneRadius));
        Gizmos.DrawLine(transform.position, transform.position - (transform.forward.normalized * coneHeight + up * coneRadius));
        Gizmos.DrawLine(transform.position, transform.position - (transform.forward.normalized * coneHeight - up * coneRadius));

        // Draw the base of the cone
        int segments = 20;
        Vector3 previousPoint = transform.position + transform.forward.normalized * coneHeight + right * coneRadius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2 / segments;
            Vector3 point = transform.position + transform.forward.normalized * coneHeight + right * Mathf.Cos(angle) * coneRadius + up * Mathf.Sin(angle) * coneRadius;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
        Gizmos.DrawLine(previousPoint, transform.position + transform.forward.normalized * coneHeight + right * coneRadius);

        previousPoint = transform.position - transform.forward.normalized * coneHeight + right * coneRadius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2 / segments;
            Vector3 point = transform.position - transform.forward.normalized * coneHeight + right * Mathf.Cos(angle) * coneRadius + up * Mathf.Sin(angle) * coneRadius;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
        Gizmos.DrawLine(previousPoint, transform.position - transform.forward.normalized * coneHeight + right * coneRadius);

    }
}