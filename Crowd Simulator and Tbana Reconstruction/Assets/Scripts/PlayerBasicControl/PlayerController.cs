using UnityEngine;

public class PlayerController : MonoBehaviour
{
    FirstPersonControl inputControl;
    Vector2 move;
    public float movementSpeed = 25f; // Movement speed
    public float dampingFactor = 0.9f; // Damping factor to reduce sliding
    Rigidbody rb;

    void Start()
    {
        inputControl = new FirstPersonControl();
        inputControl.PlayerMap.Enable();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevent the player from rotating due to physics
    }

    void Update()
    {
        // Get movement input
        move = inputControl.PlayerMap.Movement.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // Perform the player movement in FixedUpdate for consistent physics updates
        playerMove();
    }

    void playerMove()
    {
        // Calculate the velocity
        Vector3 desiredVelocity = (transform.forward * move.y + transform.right * move.x) * movementSpeed;
        desiredVelocity.y = rb.velocity.y; // Keep the current vertical velocity

        // Apply the velocity to the Rigidbody
        rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, dampingFactor * Time.fixedDeltaTime);
    }
}
