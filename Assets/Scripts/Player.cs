using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float forcePower = 10f;
    [SerializeField] private float maxDragDistance = 3f;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float torquePower = 5f;
    
    [Header("Jump Settings")]
    [SerializeField] private float maxAngularVelocity = 0.1f;
    [SerializeField] private float requiredStableTime = 0.2f; // Time in seconds angular velocity must be stable
    
    [Header("Visual Feedback")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPointCount = 20;
    [SerializeField] private Transform joystickIndicator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer joystickSpriteRenderer;
    [SerializeField] private Color canJumpColor = Color.white;
    [SerializeField] private Color cannotJumpColor = Color.grey;
    [SerializeField] private float joystickSnapDuration = 0.1f; // Duration for snap back animation
    
    [Header("Hard Landing")]
    [SerializeField] private float hardLandingVelocity = -7f;
    [SerializeField] private CameraFollow cameraFollow;
    
    private Rigidbody2D rb;
    private float currentDragRadius;
    private Vector2 dragDirection;
    private bool isDragging = false;
    private float stableAngularVelocityTimer = 0f;
    private Color joystickOriginalColor;
    private Vector2 velocityBeforeCollision;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
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
        
        // Setup trajectory line if available
        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
        }
        
        // Hide joystick indicator at start
        if (joystickIndicator != null)
        {
            joystickIndicator.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateStableTimer();
        UpdateSpriteColor();
        HandleInput();
        
        if (isDragging)
        {
            DrawTrajectory();
            UpdateJoystickVisual();
        }
    }

    void FixedUpdate()
    {
        // Store velocity before physics
        velocityBeforeCollision = rb.velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //HARD LANDING / CHIP VFX
        // Check if colliding with Ground tag and velocity was below threshold
        if (collision.gameObject.CompareTag("Ground") && velocityBeforeCollision.y < hardLandingVelocity)
        {
            Debug.Log("VFX!");
            // KHANG PLAY VFX HERE!
            
            // Trigger screen shake
            if (cameraFollow != null)
            {
                cameraFollow.TriggerShake();
            }
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
        if (spriteRenderer == null) return;
        
        spriteRenderer.color = CanJump() ? canJumpColor : cannotJumpColor;
    }
    
    private void HandleInput()
    {
        // Start dragging (always allow, regardless of CanJump)
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            currentDragRadius = 0f;
            dragDirection = Vector2.zero;
            
            if (trajectoryLine != null)
            {
                trajectoryLine.enabled = true;
            }
            
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
            
            if (trajectoryLine != null)
            {
                trajectoryLine.enabled = false;
            }
            
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
    
    private void DrawTrajectory()
    {
        if (trajectoryLine == null) return;
        
        Vector2 forceDirection = -dragDirection * forcePower;
        
        trajectoryLine.positionCount = trajectoryPointCount;
        
        Vector2 startPosition = transform.position;
        Vector2 startVelocity = forceDirection / rb.mass;
        
        for (int i = 0; i < trajectoryPointCount; i++)
        {
            float time = i * 0.1f;
            Vector2 pointPosition = startPosition + startVelocity * time + 0.5f * Physics2D.gravity * time * time;
            trajectoryLine.SetPosition(i, pointPosition);
        }
    }
    
    private void UpdateJoystickVisual()
    {        
        // Set world position relative to player
        joystickIndicator.position = (Vector2)transform.position + dragDirection * 0.5f;
        // make joystick scale based on drag distance
        float scale = 0.1f + (currentDragRadius / maxDragDistance) * 5f; // Scale between 0.01 and 0.51
        joystickIndicator.localScale = new Vector3(scale, scale, 1f);
        
        // Update joystick color based on CanJump
        if (joystickSpriteRenderer != null)
        {
            joystickSpriteRenderer.color = CanJump() ? joystickOriginalColor : cannotJumpColor;
        }
    }
}
