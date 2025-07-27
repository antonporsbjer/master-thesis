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

    }
    
    // Method to visualize the Visibility Area (VCA) on the ground plane
    public void VisualizeVCA(Vector3 position, Vector3 direction, float angle, float distance)
    {
        if (groundPlane == null)
        {
            Debug.LogError("Ground plane reference is not set!");
            return;
        }

        // Create a cone to visualize the VCA
        GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cone.transform.position = position;
        cone.transform.rotation = Quaternion.LookRotation(direction);
        cone.transform.localScale = new Vector3(distance, 0.1f, distance); // Adjust scale for visualization

        // Set the cone's parent to the ground plane
        cone.transform.parent = groundPlane.transform;

        // Optionally, set a color for the cone
        Renderer renderer = cone.GetComponent<Renderer>();
        renderer.material.color = Color.yellow; // Change color as needed
    }
}
