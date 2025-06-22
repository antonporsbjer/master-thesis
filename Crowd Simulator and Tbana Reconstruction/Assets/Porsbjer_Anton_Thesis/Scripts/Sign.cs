using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomVolume : MonoBehaviour
{
    private Vector3 v = new Vector3(1, 0, 0); // Direction vector of the cone
    public Vector3 n; // Normal vector of the cone
    public Vector3 p { get; private set; }
    public float theta { get; private set; }
    public float d { get; private set; }

    public DataCollector dataCollector; // Reference to the DataCollector

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the volume parameters
        n = transform.right.normalized; // Sign direction
        p = transform.position;
        theta = 1.57f; // (ca 90 degrees) | Mathf.PI / 4 (45 degrees)
        d = 15.0f; // View distance (in meters)
    }

    // Update is called once per frame
    void Update()
    {
        // Check for collisions
        CheckCollisions();
    }

    // Method to check for collisions
    void CheckCollisions()
    {
        Collider[] colliders = Physics.OverlapSphere(p, d);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.CompareTag("eye"))
            {
                Vector3 vi = collider.transform.position;
                Vector3 direction = (vi - p).normalized;
                float dotProduct = Vector3.Dot(direction, n);
                float angle = Mathf.Acos(dotProduct);
                float distance = Vector3.Distance(vi, p);

                // Check if the collider is within the cone and sphere
                if (angle <= theta / 2 && distance <= d)
                {
                    GameObject target = collider.gameObject;
                    Debug.Log("Collision detected with: " + target.name);
                    //if (target.)
                    //collider.gameObject
                    //dataCollector.IncrementVisibleSignCount(); // Increment the visible sign count
                }
            }
        }
    }

    // Method to visualize the volume in the Scene view
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(p, d);

        // Draw the cone
        Vector3 forward = n.normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3 up = Vector3.Cross(forward, right).normalized;

        float halfAngle = theta / 2;
        float coneHeight = d * Mathf.Cos(halfAngle);
        float coneRadius = d * Mathf.Sin(halfAngle);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(p, p + forward * coneHeight);
        Gizmos.DrawLine(p, p + (forward * coneHeight + right * coneRadius));
        Gizmos.DrawLine(p, p + (forward * coneHeight - right * coneRadius));
        Gizmos.DrawLine(p, p + (forward * coneHeight + up * coneRadius));
        Gizmos.DrawLine(p, p + (forward * coneHeight - up * coneRadius));

        // Draw the base of the cone
        int segments = 20;
        Vector3 previousPoint = p + forward * coneHeight + right * coneRadius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2 / segments;
            Vector3 point = p + forward * coneHeight + right * Mathf.Cos(angle) * coneRadius + up * Mathf.Sin(angle) * coneRadius;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
        Gizmos.DrawLine(previousPoint, p + forward * coneHeight + right * coneRadius);
    }
}