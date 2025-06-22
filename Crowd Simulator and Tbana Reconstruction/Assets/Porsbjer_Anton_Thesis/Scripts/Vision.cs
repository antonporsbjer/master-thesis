using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
    private GameObject target;
    private CustomVolume customVolume;
    private bool isVisible = false; // Flag to check if the target is visible

    public bool IsVisible
    {
        get { return isVisible; }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Find the GameObject with the tag "sign" and set it as the target
        target = GameObject.FindWithTag("sign");

        if (target == null)
        {
            Debug.LogError("No GameObject with tag 'sign' found!");
        }

        // Get the CustomVolume component
        customVolume = target.GetComponent<CustomVolume>();

        if (customVolume == null)
        {
            Debug.LogError("No CustomVolume component found on the target!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        isVisible = getTargetPoint(transform.position);
    }

    // Method to get the target point and shoot a raycast to the target
    public bool getTargetPoint(Vector3 origin)
    {
        RaycastHit hit;
        Vector3 direction = target.transform.position - origin;

        // Draw the ray in the Scene view
        Debug.DrawRay(origin, direction, Color.red);

        LayerMask mask = LayerMask.GetMask("Obstacle", "Agent");

        if (Physics.Raycast(origin, direction, out hit, mask))
        {
            if (hit.collider.gameObject == target)
            {
                // Check if the ray is within the collision volume
                if (IsWithinVolume(origin))
                {
                    // Log the hit information
                    Debug.Log("Raycast hit: " + hit.collider.gameObject.name);
                    Debug.DrawRay(origin, direction, Color.green);
                    return true; // Ray hit the target within the volume
                }
            }
        }
        return false; // Ray did not hit the target or was outside the volume
    }

    // Method to check if a point is within the collision volume
    private bool IsWithinVolume(Vector3 origin)
    {
        Vector3 direction = (origin - customVolume.p).normalized;
        float dotProduct = Vector3.Dot(direction, customVolume.n);
        float angle = Mathf.Acos(dotProduct);
        float distance = Vector3.Distance(origin, customVolume.p);

        // Check if the origin is within the cone and sphere
        return angle <= customVolume.theta / 2 && distance <= customVolume.d;
    }
}
