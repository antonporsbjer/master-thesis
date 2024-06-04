using UnityEngine;
using UnityEngine.InputSystem;

public class LookControl : MonoBehaviour
{
    public GameObject Player;
    float xRotation;
    float yRotation;
    Vector2 mouseMovement;
    float cameraY;
    Vector3 position;
    public float mouseSensitivity = 100f; 

    void Start()
    {
        cameraY = transform.position.y - Player.transform.position.y;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Update Mouse position to player position
        position = Player.transform.position;
        position.y += cameraY;
        transform.position = position;

        // Get mouse movement using the new input system
        mouseMovement = Mouse.current.delta.ReadValue();

        // Rotation
        xRotation -= mouseMovement.y * mouseSensitivity * 0.01f; // Scale with sensitivity
        xRotation = Mathf.Clamp(xRotation, -90, 90);
        yRotation += mouseMovement.x * mouseSensitivity * 0.01f;

        // Apply the rotation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        // Rotating the player
        Player.transform.localRotation = Quaternion.Euler(0, yRotation, 0);
    }
}
