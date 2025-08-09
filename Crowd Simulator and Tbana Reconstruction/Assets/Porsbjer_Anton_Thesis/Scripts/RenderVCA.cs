using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderVCA : MonoBehaviour
{
    public VisibilityArea vca; // Reference to the Visibility Area (VCA) component
    public GameObject groundPlane; // Reference to the ground plane GameObject

    // Start is called before the first frame update
    void Start()
    {
        if (groundPlane == null)
        {
            Debug.LogError("Ground plane reference is not set!");
        }
        else
        {
            VisualizeVCA(vca.transform.position, vca.transform.forward.normalized, vca.ThetaDegrees * Mathf.Deg2Rad, vca.ViewingDistance);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (groundPlane == null)
        {
            Debug.LogError("Ground plane reference is not set!");
        }
        else
        {
            VisualizeVCA(vca.transform.position, vca.transform.forward.normalized, vca.ThetaDegrees * Mathf.Deg2Rad, vca.ViewingDistance);
        }
    }

    // Method to visualize the Visibility Area (VCA) on the ground plane by drawing a triangle
    // at the specified position, direction, angle, and distance
    public void VisualizeVCA(Vector3 position, Vector3 direction, float angle, float distance)
    {
        if (groundPlane == null)
        {
            Debug.LogError("Ground plane reference is not set!");
            return;
        }
    }
}
