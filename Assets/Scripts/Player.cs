using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Player : MonoBehaviour
{
    // Enum to define player states
    public enum PlayerState
    {
        Normal,
        TShape,
        Stack,
        Kite
    }
    
    [Header("Movement Settings")]
    [SerializeField] private float forcePower = 10f;
    [SerializeField] private float maxDragDistance = 3f;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float torquePower = 5f;
    
    [Header("Jump Settings")]
    [SerializeField] private float maxAngularVelocity = 0.1f;
    [SerializeField] private float requiredStableTime = 0.2f; // Time in seconds angular velocity must be stable
    
    [Header("Visual Feedback")]
    [SerializeField] private Transform joystickIndicator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer joystickSpriteRenderer;
    [SerializeField] private Color canJumpColor = Color.white;
    [SerializeField] private Color cannotJumpColor = Color.grey;
    [SerializeField] private float joystickSnapDuration = 0.1f; // Duration for snap back animation
    [SerializeField] private GameObject rockVFX;
    
    [Header("Hard Landing")]
    [SerializeField] private float hardLandingVelocity = -7f;
    [SerializeField] private CameraFollow cameraFollow;


    [Header("Audio")]
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip softLandSound;
    private AudioSource audioSource;

    private Rigidbody2D rb;
    private float currentDragRadius;
    private Vector2 dragDirection;
    private bool isDragging = false;
    private float stableAngularVelocityTimer = 0f;
    private Color joystickOriginalColor;
    private Vector2 velocityBeforeCollision;
    private TrailRenderer playerTrail;
    
    // State Machine Variables
    private PlayerState currentState = PlayerState.Normal;
    private PlayerState previousState = PlayerState.Normal;
    
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        // Get TrailRenderer component
        playerTrail = GetComponent<TrailRenderer>();
        
        // Get SpriteRenderer if not assigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Get joystick SpriteRenderer if not assigned
        if (joystickIndicator != null && joystickSpriteRenderer == null)
        {
            joystickSpriteRenderer = joystickIndicator.GetComponent<SpriteRenderer>();
        }
        
        // Store the original joystick color
        if (joystickSpriteRenderer != null)
        {
            joystickOriginalColor = joystickSpriteRenderer.color;
        }
        
        // Get CameraFollow if not assigned
        if (cameraFollow == null)
        {
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
        }
        
        // Lock cursor at start
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Setup trail renderer if available
        if (playerTrail != null)
        {
            playerTrail.emitting = false;
        }
        
        // Hide joystick indicator at start
        if (joystickIndicator != null)
        {
            joystickIndicator.gameObject.SetActive(false);
        }
        
        // Initialize state machine
        EnterState(currentState);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateStableTimer();
        UpdateSpriteColor();
        UpdateTrailRenderer();
        
        // State Machine Update
        UpdateState(currentState);
        
        if (isDragging)
        {
            UpdateJoystickVisual();
        }
    }

    void FixedUpdate()
    {
        // Store velocity before physics
        velocityBeforeCollision = rb.velocity;
        
        // State Machine Fixed Update
        FixedUpdateState(currentState);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //HARD LANDING / CHIP VFX
        // Check if colliding with Ground tag and velocity (vertical OR horizontal) was above threshold
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (velocityBeforeCollision.y < hardLandingVelocity || Mathf.Abs(velocityBeforeCollision.x) > Mathf.Abs(hardLandingVelocity))
            {
                // Get the contact point for accurate positioning
                ContactPoint2D contact = collision.GetContact(0);

                // Use the actual contact point Y position, which already accounts for rotation
                rockVFX.transform.position = new Vector2(transform.position.x, contact.point.y);
                rockVFX.GetComponent<VisualEffect>().Play();
                //audioSource.PlayOneShot(landSound);


                cameraFollow.TriggerShake();
                // Trigger screen shake
            }

            else
            {
                // softer landing sfx
                audioSource.PlayOneShot(softLandSound);

            }
        }
    }
    
    // Change state with enter/exit handling
    public void SetState(PlayerState newState)
    {
        if (currentState == newState) return;
        
        ExitState(currentState);
        previousState = currentState;
        currentState = newState;
        EnterState(currentState);
    }
    
    // Enter state logic
    private void EnterState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Normal:
                break;
            case PlayerState.TShape:
                break;
            case PlayerState.Stack:
                break;
            case PlayerState.Kite:
                break;
        }
    }
    
    // Exit state logic
    private void ExitState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Normal:
                break;
            case PlayerState.TShape:
                break;
            case PlayerState.Stack:
                break;
            case PlayerState.Kite:
                break;
        }
    }
    
    // Update state logic
    private void UpdateState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Normal:
                HandleInput();
                break;
            case PlayerState.TShape:
                HandleInput();
                break;
            case PlayerState.Stack:
                HandleInput();
                break;
            case PlayerState.Kite:
                HandleInput();
                break;
        }
    }
    
    // Fixed Update state logic
    private void FixedUpdateState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Normal:
                
                break;
            case PlayerState.TShape:

                break;
            case PlayerState.Stack:

                break;
            case PlayerState.Kite:

                break;
        }
    }
    
    private void UpdateStableTimer()
    {
        // Check if angular velocity is currently below threshold
        if (Mathf.Abs(rb.angularVelocity) < maxAngularVelocity)
        {
            // Increment timer
            stableAngularVelocityTimer += Time.deltaTime;
        }
        else
        {
            // Reset timer if angular velocity exceeds threshold
            stableAngularVelocityTimer = 0f;
        }
    }
    
    private bool CanJump()
    {
        // Check if angular velocity has been stable for required duration
        return stableAngularVelocityTimer >= requiredStableTime;
    }
    
    private void UpdateSpriteColor()
    {
        
        spriteRenderer.color = CanJump() ? canJumpColor : cannotJumpColor;
    }
    
    private void UpdateTrailRenderer()
    {
        if (playerTrail == null) return;
        
        // Enable trail when player cannot jump, disable when they can
        playerTrail.emitting = !CanJump();
    }
    
    private void HandleInput()
    {
        // Start dragging (always allow, regardless of CanJump)
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            currentDragRadius = 0f;
            dragDirection = Vector2.zero;
            
            if (joystickIndicator != null)
            {
                joystickIndicator.gameObject.SetActive(true);
                joystickIndicator.position = transform.position;
            }
        }
        
        // Update drag position based on mouse movement
        if (Input.GetMouseButton(0) && isDragging)
        {
            // Get mouse delta movement
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            
            Vector2 mouseDelta = new Vector2(mouseX, mouseY) * mouseSensitivity;
            
            // Add mouse delta to drag direction
            dragDirection += mouseDelta;
            
            // Calculate radius (distance from center)
            currentDragRadius = dragDirection.magnitude;
            
            // Limit drag distance
            if (currentDragRadius > maxDragDistance)
            {
                dragDirection = dragDirection.normalized * maxDragDistance;
                currentDragRadius = maxDragDistance;
            }
        }
        
        // Release and apply force only if can jump
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            if (CanJump())
            {
                ApplyForce();
            }
            
            isDragging = false;
            
            if (joystickIndicator != null)
            {
                StartCoroutine(SnapJoystickToNeutral());
            }
        }
    }
    
    private IEnumerator SnapJoystickToNeutral()
    {
        Vector2 startPosition = joystickIndicator.position;
        Vector3 startScale = joystickIndicator.localScale;
        Vector2 targetPosition = transform.position;
        Vector3 targetScale = new Vector3(0.01f, 0.01f, 1f);
        
        float elapsed = 0f;
        
        while (elapsed < joystickSnapDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / joystickSnapDuration;
            
            // Smoothly interpolate position and scale
            joystickIndicator.position = Vector2.Lerp(startPosition, targetPosition, t);
            joystickIndicator.localScale = Vector3.Lerp(startScale, targetScale, t);
            
            yield return null;
        }
        
        // Ensure final values are set
        joystickIndicator.position = targetPosition;
        joystickIndicator.localScale = targetScale;
        
        // Deactivate joystick
        joystickIndicator.gameObject.SetActive(false);
    }
    
    private void ApplyForce()
    {
        // Calculate force direction (opposite of drag)
        Vector2 forceDirection = -dragDirection;
        
        // Apply force to rigidbody
        rb.AddForce(forceDirection * forcePower, ForceMode2D.Impulse);
        
        // Apply torque based on horizontal force direction
        // Positive torque (clockwise) when force is to the right
        // Negative torque (counter-clockwise) when force is to the left
        float torque = -forceDirection.x * torquePower;
        rb.AddTorque(torque, ForceMode2D.Impulse);
    }
    
    private void UpdateJoystickVisual()
    {        
        // Set world position relative to player
        joystickIndicator.position = (Vector2)transform.position + dragDirection * 0.5f;
        // make joystick scale based on drag distance
        float scale = 0.1f + (currentDragRadius / maxDragDistance) * 3.5f; // Scale between 0.01 and 0.51
        joystickIndicator.localScale = new Vector3(scale, scale, 1f);
        
        // Update joystick color based on CanJump
        if (joystickSpriteRenderer != null)
        {
            joystickSpriteRenderer.color = CanJump() ? joystickOriginalColor : cannotJumpColor;
        }
    }
}
