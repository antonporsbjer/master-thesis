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

    void Awake()
    {
        // Find the DataCollector in the scene
        dataCollector = FindObjectOfType<DataCollector>();

        if (signPositionBoundary != null && RandomPosition)
        {
            // Random angle around the Y-axis
            float angle = Random.Range(0, 1) * 90 * Mathf.Deg2Rad;

            BoxCollider boundary = signPositionBoundary.GetComponentsInParent<BoxCollider>()[0];
            // random x and z within the box collider
            float x = Random.Range(boundary.center.x - boundary.size.x / 2, boundary.center.x + boundary.size.x / 2);
            float z = Random.Range(boundary.center.z - boundary.size.z / 2, boundary.center.z + boundary.size.z / 2);

            transform.SetPositionAndRotation(new Vector3(x, transform.position.y, z), Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0));
            signPositionBoundary.GetComponent<BoxCollider>().enabled = false;
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