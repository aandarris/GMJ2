using UnityEngine;

public class Jumping : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float airControlSpeed = 2f; // Speed of changing direction in the air

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float fallMultiplier = 2.5f; // Multiplier for faster falling
    [SerializeField] private float lowJumpMultiplier = 2f; // Multiplier for shorter jumps

    private CharacterController characterController;
    private Vector3 velocity;

    private bool isGrounded;
    private bool canDoubleJump;

    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private float horizontalAirVelocity; // Tracks horizontal velocity in the air
    private bool jumpStartedFromStationary; // Tracks if the jump was initiated without horizontal input

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Check if the player is grounded
        isGrounded = CheckIfGrounded();

        // Reset values when grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
            canDoubleJump = true;
            horizontalAirVelocity = 0f; // Reset air movement when landing
            jumpStartedFromStationary = false; // Reset stationary jump flag
        }

        // Handle movement
        Move();
        HandleJump();

        // Apply gravity
        ApplyGravity();
    }

    private void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");

        if (isGrounded)
        {
            // Grounded movement
            Vector3 movement = new Vector3(moveHorizontal, 0, 0);
            characterController.Move(movement * moveSpeed * Time.deltaTime);

            // Reset air velocity based on current movement
            if (moveHorizontal != 0)
            {
                horizontalAirVelocity = moveHorizontal * moveSpeed;
            }
        }
        else
        {
            // Airborne movement
            if (jumpStartedFromStationary)
            {
                // If the jump started stationary, no horizontal movement unless input is provided
                if (moveHorizontal != 0)
                {
                    horizontalAirVelocity = Mathf.MoveTowards(
                        horizontalAirVelocity,
                        moveHorizontal * moveSpeed,
                        airControlSpeed * Time.deltaTime
                    );
                }
                else
                {
                    // No input; character remains stationary horizontally
                    horizontalAirVelocity = 0f;
                }
            }
            else
            {
                // If the jump started with movement, allow smooth direction changes
                if (moveHorizontal != 0)
                {
                    horizontalAirVelocity = Mathf.MoveTowards(
                        horizontalAirVelocity,
                        moveHorizontal * moveSpeed,
                        airControlSpeed * Time.deltaTime
                    );
                }
            }

            // Apply horizontal air movement
            Vector3 airMovement = new Vector3(horizontalAirVelocity, 0, 0);
            characterController.Move(airMovement * Time.deltaTime);
        }
    }

    private void HandleJump()
    {
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            // Check if the jump starts with no horizontal movement
            jumpStartedFromStationary = Mathf.Abs(Input.GetAxis("Horizontal")) < 0.01f;

            Jump();
        }
        else if (!isGrounded && canDoubleJump && Input.GetButtonDown("Jump"))
        {
            // On double jump, immediately change direction to the key pressed
            float moveHorizontal = Input.GetAxis("Horizontal");
            if (Mathf.Abs(moveHorizontal) > 0.01f)
            {
                horizontalAirVelocity = moveHorizontal * moveSpeed; // Immediate direction change
            }

            Jump();
            canDoubleJump = false; // Disable further jumps after the double jump
        }
    }

    private void Jump()
    {
        // Reset vertical velocity for a consistent jump
        velocity.y = jumpForce;
    }

    private void ApplyGravity()
    {
        // Apply stronger gravity when falling
        if (velocity.y < 0)
        {
            velocity.y += Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (velocity.y > 0 && !Input.GetButton("Jump"))
        {
            // Apply lower jump multiplier for shorter jumps if jump button is released
            velocity.y += Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
        else
        {
            // Regular gravity application
            velocity.y += Physics.gravity.y * Time.deltaTime;
        }

        // Apply vertical velocity to the character controller
        characterController.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);
    }

    private bool CheckIfGrounded()
    {
        // Perform a sphere cast to check for ground beneath the player
        return Physics.CheckSphere(transform.position - new Vector3(0, characterController.height / 2, 0), groundCheckDistance, groundLayer);
    }
}
