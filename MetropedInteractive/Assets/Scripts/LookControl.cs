using UnityEngine;
using UnityEngine.InputSystem;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform player; 
    public Vector3 offset; // Offset between the camera and player
    public float smoothSpeed = 0.125f; // Smoothing factor
    public float yOffset = 0.3f; // Additional y-offset to adjust camera height

    private Vector2 mouseMovement;
    private float xRotation = 0f;
    private float yRotation = 90f;
    public float mouseSensitivity = 100f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        if (GameManager.Instance.IsMovementPaused)
        {
            return;
        }

        mouseMovement = Mouse.current.delta.ReadValue();

        // Rotation
        xRotation -= mouseMovement.y * mouseSensitivity * 0.01f; // Scale with sensitivity
        xRotation = Mathf.Clamp(xRotation, -90, 90);
        yRotation += mouseMovement.x * mouseSensitivity * 0.01f;

        // Apply the rotation to the player
        player.localRotation = Quaternion.Euler(0, yRotation, 0);

        // Smooth follow
        Vector3 desiredPosition = player.position + offset + new Vector3(0, yOffset, 0);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Apply the rotation to the camera
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
    }
}
