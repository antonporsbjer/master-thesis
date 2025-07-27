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
    private DataCollector dataCollector; // Reference to the DataCollector

    // Start is called before the first frame update
    void Start()
    {
        // Find the DataCollector in the scene
        dataCollector = FindObjectOfType<DataCollector>();

        // Initialize the volume parameters
        Theta = ThetaDegrees * Mathf.Deg2Rad; // Convert angle to radians
    }

    // Update is called once per frame
    void Update()
    {
        // Update the angle in radians
        Theta = ThetaDegrees * Mathf.Deg2Rad; // Convert angle to radians
        // Check for collisions
        CheckCollisions();
    }

    // Method to check for collisions
    void CheckCollisions()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, ViewingDistance);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.CompareTag("eye"))
            {
                Vector3 vi = collider.transform.position;
                Vector3 direction = (vi - transform.position).normalized;
                float dotProduct = Vector3.Dot(direction, transform.forward.normalized);
                float angle = Mathf.Acos(dotProduct);
                float distance = Vector3.Distance(vi, transform.position);

                // Check if the collider is within the cone and sphere
                if (angle <= Theta / 2 && distance <= ViewingDistance)
                {
                    GameObject target = collider.gameObject;
                    Debug.Log("Collision detected with: " + target.name);
                }
            }
        }
    }

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
    }
}