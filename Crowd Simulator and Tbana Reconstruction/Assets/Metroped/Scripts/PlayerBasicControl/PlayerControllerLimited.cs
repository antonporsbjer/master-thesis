using UnityEngine;

public class PlayerControllerLimited : MonoBehaviour
{
    FirstPersonControl inputControl;
    Vector2 move;
    public float movementSpeed = 25f; // Movement speed
    public float dampingFactor = 0.9f; // Damping factor to reduce sliding
    public float minX = -10f; // Minimum x-coordinate
    public float maxX = 10f; // Maximum x-coordinate
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
        if (GameManager.Instance.IsMovementPaused)
        {
            return;
        }
        // Perform the player movement in FixedUpdate for consistent physics updates
        playerMove();
    }

    void playerMove()
    {
        // Calculate the desired velocity
        Vector3 desiredVelocity = (transform.forward * move.y + transform.right * move.x) * movementSpeed;
        desiredVelocity.y = rb.velocity.y; // Keep the current vertical velocity

        // Check the player's current position and adjust the desired velocity accordingly
        Vector3 newPosition = rb.position + desiredVelocity * Time.fixedDeltaTime;

        // If the new position is within the allowed x-coordinate range, apply the velocity
        if (newPosition.x >= minX && newPosition.x <= maxX)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, dampingFactor * Time.fixedDeltaTime);
        }
        else
        {
            // If the new position is outside the allowed range, adjust the velocity to stop movement in x direction
            desiredVelocity.x = 0;
            rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, dampingFactor * Time.fixedDeltaTime);
        }
    }
}
