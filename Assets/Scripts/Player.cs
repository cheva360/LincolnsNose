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
        Stack,
        TShape,
        Kite
    }
    //hi
    [Header("State Settings")]
    [SerializeField] private PlayerState playerState = PlayerState.Normal;
    
    [Header("Movement Settings")]
    [SerializeField] private float forcePower = 10f;
    [SerializeField] private float maxDragDistance = 3f;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float torquePower = 5f;

    [Header("Washington")]
    [SerializeField] private Transform washingtonTransform;
    [SerializeField] private SpriteRenderer washingtonSpriteRenderer;
    [SerializeField] private float menuCooldownTime = 2f; // Time in seconds before menu can be opened again
    private bool isCollidingWithWashington = false;
    private float menuCooldownTimer = 0f;
    private bool canOpenMenu = true;
    private bool isNoseJobMenuActive = false; // Track if nose job menu is open
    private Vector2 frozenVelocity;
    private float frozenAngularVelocity;

    [Header("Jump Settings")]
    [SerializeField] private float maxAngularVelocity = 0.1f;
    [SerializeField] private float requiredStableTime = 0.2f; // Time in seconds angular velocity must be stable
    
    [Header("Stack State Settings")]
    [SerializeField] private float stackChargeRate = 5f; // How fast charge builds up
    [SerializeField] private float stackMaxCharge = 10f; // Maximum charge
    [SerializeField] private float stackForcePower = 15f; // Vertical force multiplier
    
    [Header("Kite State Settings")]
    [SerializeField] private float kiteChargeRate = 5f;
    [SerializeField] private float kiteMaxCharge = 10f;
    [SerializeField] private float kiteForcePower = 15f; // Horizontal force multiplier
    
    [Header("TShape State Settings")]
    [SerializeField] private float tShapeChargeRate = 5f;
    [SerializeField] private float tShapeMaxCharge = 10f;
    [SerializeField] private float tShapeForcePower = 5f; // Smaller jump
    [SerializeField] private float tShapeTorquePower = 20f; // High torque for spin
    [SerializeField] private float tShapeBreakRadius = 1f; // Radius for breaking objects
    
    [Header("Visual Feedback")]
    [SerializeField] private Transform joystickIndicator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer joystickSpriteRenderer;
    [SerializeField] private Color canJumpColor = Color.white;
    [SerializeField] private Color cannotJumpColor = Color.grey;
    [SerializeField] private float joystickSnapDuration = 0.1f; // Duration for snap back animation
    [SerializeField] private VisualEffect landingVFX;
    [SerializeField] private GameObject transformationVFX;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Transformation")]
    [SerializeField] private float transformationRotationDuration = 0.3f; // Duration to lerp rotation to 0
    [SerializeField] private float transformationYLerpDuration = 0.5f; // Duration to lerp position up
    [SerializeField] private float lerpYoffset = 1f; // Vertical offset for transformation movement
    [SerializeField] private float transformationCompletionDelay = 2.25f; // Time before PlayCircle and FinishTransformation are called

    [Header("Hard Landing")]
    [SerializeField] private float hardLandingVelocity = -7f;
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Soft Landing")]
    [SerializeField] private float softLandingAngularVelocity = 50f; // Angular velocity threshold for soft landing
    [Header("Audio")]
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip softLandSound;
    private AudioSource audioSource;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Collider2D washingtonCollider;
    private float currentDragRadius;
    private Vector2 dragDirection;
    private bool isDragging = false;
    private float stableAngularVelocityTimer = 0f;
    private Color joystickOriginalColor;
    private Vector2 velocityBeforeCollision;
    private float angularVelocityBeforeCollision;
    private TrailRenderer playerTrail;
    private bool isSoftLandSoundPlaying = false;
    
    // State Machine Variables
    private PlayerState currentState = PlayerState.Normal;
    private PlayerState previousState = PlayerState.Normal;
    
    // Charge variables for aerial abilities
    private float currentCharge = 0f;
    private bool isCharging = false;
    private bool isGrounded = false;
    private int groundContactCount = 0; // Track number of ground contacts (player + washington)
    private bool hasUsedAerialAbility = false; // Track if aerial ability has been used
    
    // Transformation variables
    private bool isTransforming = false;
    private bool waitingForLerpToStart = false;
    private float transformationTimer = 0f;
    private float transformationWaitTimer = 0f;
    private Vector2 transformationStartPosition;
    private Vector2 transformationTargetPosition;
    
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        playerCollider = GetComponent<Collider2D>();

        // Get TrailRenderer component
        playerTrail = GetComponent<TrailRenderer>();
        
        
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
        
        // Get Washington's SpriteRenderer and Collider if not assigned
        if (washingtonTransform != null)
        {
            if (washingtonSpriteRenderer == null)
            {
                washingtonSpriteRenderer = washingtonTransform.GetComponent<SpriteRenderer>();
            }
            washingtonCollider = washingtonTransform.GetComponent<Collider2D>();
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
        
        // Initialize state machine with inspector value
        currentState = playerState;
        EnterState(currentState);

        // Register playerScript reference in GameController
        if (GameController.Instance != null)
        {
            GameController.Instance.RegisterPlayer(this);
            // Subscribe to nose job menu events
            GameController.Instance.ToggleNoseJob += OnNoseJobMenuToggle;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (GameController.Instance != null)
        {
            GameController.Instance.ToggleNoseJob -= OnNoseJobMenuToggle;
        }
    }

    // Called when nose job menu opens or closes
    private void OnNoseJobMenuToggle(bool isOpen)
    {
        isNoseJobMenuActive = isOpen;
        
        if (isOpen)
        {
            // Freeze player physics
            FreezePlayer();
        }
        else
        {
            // Unfreeze player physics
            UnfreezePlayer();
        }
    }

    private void FreezePlayer()
    {
        // Store current velocities
        frozenVelocity = rb.velocity;
        frozenAngularVelocity = rb.angularVelocity;
        
        // Freeze all physics
        rb.bodyType = RigidbodyType2D.Static;
        
        // Cancel any ongoing drag
        if (isDragging)
        {
            isDragging = false;
            if (joystickIndicator != null)
            {
                joystickIndicator.gameObject.SetActive(false);
            }
        }
        
        Debug.Log("Player frozen for nose job menu");
    }

    private void UnfreezePlayer()
    {
        // Restore physics
        rb.bodyType = RigidbodyType2D.Dynamic;
        
        // Restore velocities (set to zero for clean state after nose job)
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        Debug.Log("Player unfrozen after nose job menu");
    }

    // Update is called once per frame
    void Update()
    {
        // Don't process input while nose job menu is active
        if (isNoseJobMenuActive)
        {
            return;
        }
        
        // Check if state changed in inspector during runtime
        if (playerState != currentState)
        {
            SetState(playerState);
        }
        
        UpdateStableTimer();
        UpdateSpriteColor();
        UpdateTrailRenderer();
        UpdateMenuCooldown();
        
        // Handle transformation lerp in Update
        if (isTransforming)
        {
            UpdateTransformationLerp();
        }
        
        
        // Handle right-click to release Washington (only when attached as child)
        if (Input.GetMouseButtonDown(1) && washingtonTransform != null && washingtonTransform.parent == transform)
        {
            ReleaseWashington();
        }
        
        // State Machine Update
        UpdateState(currentState);
        
        if (isDragging)
        {
            UpdateJoystickVisual();
        }
    }

    void FixedUpdate()
    {
        // Don't process physics while nose job menu is active
        if (isNoseJobMenuActive)
        {
            return;
        }
        
        // Store velocity and angular velocity before physics
        velocityBeforeCollision = rb.velocity;
        angularVelocityBeforeCollision = rb.angularVelocity;
        
        // State Machine Fixed Update
        FixedUpdateState(currentState);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollisionEnter(collision, true);
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        HandleCollisionExit(collision, true);
    }
    
    // Public methods for Washington to call (add WashingtonCollider script to Washington)
    public void OnWashingtonCollisionEnter(Collision2D collision)
    {
        HandleCollisionEnter(collision, false);
    }
    
    public void OnWashingtonCollisionExit(Collision2D collision)
    {
        HandleCollisionExit(collision, false);
    }
    
    private void UpdateMenuCooldown()
    {
        // Countdown the cooldown timer
        if (!canOpenMenu)
        {
            menuCooldownTimer -= Time.deltaTime;
            
            if (menuCooldownTimer <= 0f)
            {
                canOpenMenu = true;
                menuCooldownTimer = 0f;
            }
        }
    }
    
    private void HandleCollisionEnter(Collision2D collision, bool isPlayerCollider)
    {
        // Check if colliding with Ground tag
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundContactCount++;
            isGrounded = true;
            
            // Set hasUsedAerialAbility to true when landing
            hasUsedAerialAbility = true;
            
            // HARD LANDING / CHIP VFX - only trigger once from player collider
            if (isPlayerCollider)
            {
                // Check if velocity (vertical OR horizontal) was above threshold
                if (velocityBeforeCollision.y < hardLandingVelocity || Mathf.Abs(velocityBeforeCollision.x) > Mathf.Abs(hardLandingVelocity))
                {
                    // Get the contact point for accurate positioning
                    ContactPoint2D contact = collision.GetContact(0);

                    // Use the actual contact point Y position, which already accounts for rotation
                    landingVFX.transform.position = new Vector2(transform.position.x, contact.point.y);
                    landingVFX.SendEvent("OnLand");
                    //landingVFX.SendEvent("OnLandCave");
                    audioSource.PlayOneShot(landSound);

                    cameraFollow.TriggerShake();
                }
                // SOFT LAND VFX - only check for soft landing if it wasn't a hard landing
                else if (Mathf.Abs(angularVelocityBeforeCollision) > (Mathf.Abs(softLandingAngularVelocity)))
                {
                    Debug.Log("Soft landing detected with angular velocity: " + angularVelocityBeforeCollision + "soft landing" + softLandingAngularVelocity);
                    // Play soft land sound only if not already playing
                    if (!isSoftLandSoundPlaying && softLandSound != null)
                    {
                        StartCoroutine(PlaySoftLandSound());
                    }
                }
            }
        }

        // Check for Washington collision - ONLY when player's collider detects it
        // This prevents duplicate detection from WashingtonCollider script
        if (isPlayerCollider && collision.gameObject.CompareTag("Washington") && currentState == PlayerState.Normal && canOpenMenu)
        {
            isCollidingWithWashington = true;
            Debug.Log("Player collided with Washington! Opening menu and freezing player...");
            // Open nose job menu (this will trigger FreezePlayer via event)
            if (GameController.Instance != null)
            {
                GameController.Instance.ToggleNoseJobMenu(true);
            }
            else
            {
                Debug.LogError("GameController.Instance is null!");
            }
        }
        
        // Handle breakable collisions from Washington when in TShape state
        if (!isPlayerCollider && currentState == PlayerState.TShape && collision.gameObject.CompareTag("Breakable"))
        {
            if (hasUsedAerialAbility && !isGrounded)
            {
                Destroy(collision.gameObject);
            }
        }
    }
    
    private void HandleCollisionExit(Collision2D collision, bool isPlayerCollider)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundContactCount--;
            if (groundContactCount <= 0)
            {
                groundContactCount = 0;
                isGrounded = false;
            }
        }

        // Track when leaving Washington collision
        if (collision.gameObject.CompareTag("Washington") && isPlayerCollider)
        {
            isCollidingWithWashington = false;
        }
    }
    
    private IEnumerator PlaySoftLandSound()
    {
        isSoftLandSoundPlaying = true;
        audioSource.PlayOneShot(softLandSound);
        
        // Wait for the duration of the sound clip
        yield return new WaitForSeconds(softLandSound.length);
        
        isSoftLandSoundPlaying = false;
    }
    
    // Change state with enter/exit handling
    public void SetState(PlayerState newState)
    {
        if (currentState == newState) return;
        
        ExitState(currentState);
        previousState = currentState;
        currentState = newState;
        playerState = newState; // Sync inspector value
        EnterState(currentState);
    }

    // Release Washington from being a child (on right-click)
    private void ReleaseWashington()
    {
        if (washingtonTransform == null) return;

        Debug.Log("Releasing Washington");

        // Detach Washington from player
        washingtonTransform.SetParent(null);

        // Start cooldown timer
        canOpenMenu = false;
        menuCooldownTimer = menuCooldownTime;

        // Return to Normal state
        SetState(PlayerState.Normal);
    }
    
    // Enter state logic
    private void EnterState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Normal:
                break;
            case PlayerState.TShape:
                StartTransformation();
                break;
            case PlayerState.Stack:
                StartTransformation();
                break;
            case PlayerState.Kite:
                StartTransformation();
                break;
        }
    }

    // Start transformation sequence: Freeze physics, reset rotation, play animation, then wait for timed completion
    private void StartTransformation()
    {
        //Debug.Log("StartTransformation called");
        
        // Disable player collider during transformation
        playerCollider.enabled = false;
        landingVFX.transform.position = washingtonTransform.position;
        VisualEffect vfx = landingVFX.GetComponent<VisualEffect>();
        vfx.SendEvent("OnLand");
        washingtonTransform.gameObject.SetActive(false);
        
        // Disable gravity and freeze rotation during transformation
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Reset rotation to 0
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        
        // Play the transformation animation
        animator.SetTrigger("Transformation");
        
        // Store positions for lerp and START IT IMMEDIATELY
        transformationStartPosition = transform.position;
        transformationTargetPosition = transformationStartPosition + Vector2.up * lerpYoffset;
        
        // Start lerping immediately (during animation)
        isTransforming = true;
        transformationTimer = 0f;
        waitingForLerpToStart = false;
        
        // Start the timed transformation completion coroutine
        StartCoroutine(CompleteTransformationAfterDelay());
        
        //Debug.Log($"Transformation started at position {transformationStartPosition}, target: {transformationTargetPosition}");
    }

    // Coroutine to wait for the specified delay, then call PlayCircle and FinishTransformation
    private IEnumerator CompleteTransformationAfterDelay()
    {
        yield return new WaitForSeconds(transformationCompletionDelay);
        
        PlayCircle();
        FinishTransformation();
    }
    
    // Update transformation position lerp
    private void UpdateTransformationLerp()
    {
        transformationTimer += Time.deltaTime;
        float t = transformationTimer / transformationYLerpDuration;
        
        if (t >= 1f)
        {
            // Lerp complete
            transform.position = transformationTargetPosition;
            isTransforming = false;
            transformationTimer = 0f;
            Debug.Log($"Transformation lerp complete. Final position: {transform.position}");
            return;
        }
        
        // Lerp position upward
        transform.position = Vector2.Lerp(transformationStartPosition, transformationTargetPosition, t);
    }

    // Called after delay to restore physics (can still be called by animation event if needed)
    public void FinishTransformation()
    {
        Debug.Log("FinishTransformation called");
        
        // Re-enable player collider
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }
        
        // Restore gravity
        rb.gravityScale = 2f;
        
        // Restore rotation constraints (allow rotation again)
        rb.constraints = RigidbodyConstraints2D.None;
        
        // Reset velocities
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        // Reset aerial ability state and stable timer after transformation completes
        hasUsedAerialAbility = true;
        stableAngularVelocityTimer = 0f;
        
        // Reset ground state to ensure proper state tracking
        // This fixes the bug where aerial abilities don't work after transformation
        isGrounded = false;
        groundContactCount = 0;
        
        Debug.Log($"Transformation finished. hasUsedAerialAbility: {hasUsedAerialAbility}, isGrounded: {isGrounded}");
        
        // Attach Washington as child and set position/rotation based on state
        AttachWashington();
    }

    // Called after delay to play transformation VFX (can still be called by animation event if needed)
    public void PlayCircle()
    {
        if (transformationVFX != null)
        {
            transformationVFX.transform.position = transform.position;
            VisualEffect vfx = transformationVFX.GetComponent<VisualEffect>();
            if (vfx != null)
            {
                vfx.Play();
            }
        }
    }
    
    // Attach Washington as child and position based on current state
    private void AttachWashington()
    {        
        // Activate Washington before setting position
        washingtonTransform.gameObject.SetActive(true);
        
        // Set Washington as child of player
        washingtonTransform.SetParent(transform);



        // Set local position and rotation based on current state
        switch (currentState)
        {
            case PlayerState.TShape:
                washingtonTransform.localPosition = new Vector3(-0.067f, 0.267f, 0f);
                washingtonTransform.localRotation = Quaternion.Euler(0f, 0f, 95f);
                break;
            case PlayerState.Stack:
                washingtonTransform.localPosition = new Vector3(-0.049f, 0.345f, 0f);
                washingtonTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            case PlayerState.Kite:
                washingtonTransform.localPosition = new Vector3(-0.238f, 0.032f, 0f);
                washingtonTransform.localRotation = Quaternion.Euler(0f, 0f, -187.502f);
                break;
            case PlayerState.Normal:
                // Optionally detach Washington or set a default position
                washingtonTransform.SetParent(null);
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
            case PlayerState.Stack:
            case PlayerState.Kite:
                // Detach Washington when exiting a transformation state
                if (washingtonTransform != null && washingtonTransform.parent == transform)
                {
                    washingtonTransform.SetParent(null);


                }
                break;
        }
        
        // Reset charge when exiting any state
        currentCharge = 0f;
        isCharging = false;
    }
    
    // Update state logic
    public void UpdateState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Normal:
                HandleNormalInput();
                break;
            case PlayerState.TShape:
                HandleTShapeInput();
                break;
            case PlayerState.Stack:
                HandleStackInput();
                break;
            case PlayerState.Kite:
                HandleKiteInput();
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
                // Only check for breakables if aerial ability has been used and spinning fast
                if (hasUsedAerialAbility && !isGrounded && Mathf.Abs(rb.angularVelocity) > softLandingAngularVelocity)
                {
                    CheckForBreakables();
                }
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
            
            // Reset aerial ability flag when stable on ground and can jump
            if (isGrounded && stableAngularVelocityTimer >= requiredStableTime && CanJump())
            {
                hasUsedAerialAbility = false;
            }
        }
        else
        {
            // Reset timer if angular velocity exceeds threshold
            stableAngularVelocityTimer = 0f;
        }
    }
    
    private bool CanJump()
    {
        // Can jump if grounded and stable, OR in air but haven't used aerial ability yet (except Normal state)
        if (isGrounded)
        {
            return stableAngularVelocityTimer >= requiredStableTime;
        }
        else
        {
            // In air: Normal state can't jump, other states can use ability once
            if (currentState == PlayerState.Normal)
            {
                return false; // Normal has no aerial ability
            }
            else
            {
                // Other states can jump in air if they haven't used their aerial ability yet
                return !hasUsedAerialAbility;
            }
        }
    }
    
    private void UpdateSpriteColor()
    {
        // Update player sprite color
        spriteRenderer.color = CanJump() ? canJumpColor : cannotJumpColor;
        
        // Update Washington sprite color to match
        if (washingtonSpriteRenderer != null && washingtonTransform != null && washingtonTransform.gameObject.activeSelf)
        {
            washingtonSpriteRenderer.color = CanJump() ? canJumpColor : cannotJumpColor;
        }
    }
    
    private void UpdateTrailRenderer()
    {
        if (playerTrail == null) return;
        
        // Enable trail when player cannot jump, disable when they can
        playerTrail.emitting = !CanJump();
    }
    
    // Normal state input (original behavior - no aerial ability)
    private void HandleNormalInput()
    {
        // Start dragging (allow even when can't jump for visual feedback)
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
        
        // Release and apply force only if can jump and grounded
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            if (CanJump() && isGrounded)
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
    
    // Stack state: Normal jump on ground, vertical charge jump in air
    private void HandleStackInput()
    {
        // Ground behavior: normal jump
        if (isGrounded)
        {
            // Use normal jump logic when grounded
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
            
            if (Input.GetMouseButton(0) && isDragging)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                Vector2 mouseDelta = new Vector2(mouseX, mouseY) * mouseSensitivity;
                dragDirection += mouseDelta;
                currentDragRadius = dragDirection.magnitude;
                if (currentDragRadius > maxDragDistance)
                {
                    dragDirection = dragDirection.normalized * maxDragDistance;
                    currentDragRadius = maxDragDistance;
                }
            }
            
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
            return;
        }
        
        // Aerial ability not used yet - use charge behavior
        if (!hasUsedAerialAbility)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isCharging = true;
                currentCharge = 0f;
                isDragging = true;
                dragDirection = Vector2.down; // Start pointing down
                
                if (joystickIndicator != null)
                {
                    joystickIndicator.gameObject.SetActive(true);
                    joystickIndicator.position = transform.position;
                }
            }
            
            if (Input.GetMouseButton(0) && isCharging)
            {
                // Increase charge over time
                currentCharge += stackChargeRate * Time.deltaTime;
                currentCharge = Mathf.Min(currentCharge, stackMaxCharge);
                
                // Update joystick to grow downward
                currentDragRadius = (currentCharge / stackMaxCharge) * maxDragDistance;
                dragDirection = Vector2.down * currentDragRadius;
            }
            
            if (Input.GetMouseButtonUp(0) && isCharging)
            {
                // Apply vertical force based on charge
                Vector2 verticalForce = Vector2.up * currentCharge * stackForcePower;
                rb.AddForce(verticalForce, ForceMode2D.Impulse);
                
                // Mark ability as used
                hasUsedAerialAbility = true;
                
                isCharging = false;
                isDragging = false;
                currentCharge = 0f;
                
                if (joystickIndicator != null)
                {
                    StartCoroutine(SnapJoystickToNeutral());
                }
            }
        }
        // Aerial ability already used - revert to normal drag behavior (no force applied)
        else
        {
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
            
            if (Input.GetMouseButton(0) && isDragging)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                Vector2 mouseDelta = new Vector2(mouseX, mouseY) * mouseSensitivity;
                dragDirection += mouseDelta;
                currentDragRadius = dragDirection.magnitude;
                if (currentDragRadius > maxDragDistance)
                {
                    dragDirection = dragDirection.normalized * maxDragDistance;
                    currentDragRadius = maxDragDistance;
                }
            }
            
            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                // Don't apply force - ability already used
                isDragging = false;
                if (joystickIndicator != null)
                {
                    StartCoroutine(SnapJoystickToNeutral());
                }
            }
        }
    }
    
    // Kite state: Normal jump on ground, horizontal dash in air
    private void HandleKiteInput()
    {
        // Ground behavior: normal jump
        if (isGrounded)
        {
            // Use normal jump logic when grounded
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
            
            if (Input.GetMouseButton(0) && isDragging)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                Vector2 mouseDelta = new Vector2(mouseX, mouseY) * mouseSensitivity;
                dragDirection += mouseDelta;
                currentDragRadius = dragDirection.magnitude;
                if (currentDragRadius > maxDragDistance)
                {
                    dragDirection = dragDirection.normalized * maxDragDistance;
                    currentDragRadius = maxDragDistance;
                }
            }
            
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
            return;
        }
        
        // Aerial ability not used yet - use charge behavior
        if (!hasUsedAerialAbility)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isCharging = true;
                currentCharge = 0f;
                isDragging = true;
                dragDirection = Vector2.zero;
                
                if (joystickIndicator != null)
                {
                    joystickIndicator.gameObject.SetActive(true);
                    joystickIndicator.position = transform.position;
                }
            }
            
            if (Input.GetMouseButton(0) && isCharging)
            {
                // Get horizontal mouse input
                float mouseX = Input.GetAxis("Mouse X");
                
                // Accumulate horizontal direction
                dragDirection.x += mouseX * mouseSensitivity;
                
                // Clamp to left or right (X-axis only)
                dragDirection.x = Mathf.Clamp(dragDirection.x, -maxDragDistance, maxDragDistance);
                dragDirection.y = 0f; // Keep Y at zero
                
                // Increase charge over time
                currentCharge += kiteChargeRate * Time.deltaTime;
                currentCharge = Mathf.Min(currentCharge, kiteMaxCharge);
                
                currentDragRadius = Mathf.Abs(dragDirection.x);
            }
            
            if (Input.GetMouseButtonUp(0) && isCharging)
            {
                // Apply horizontal force based on charge and direction (opposite of drag direction)
                float direction = -Mathf.Sign(dragDirection.x);
                if (dragDirection.x == 0f) direction = 0f; // Handle no direction case
                
                Vector2 horizontalForce = Vector2.right * direction * currentCharge * kiteForcePower;
                rb.AddForce(horizontalForce, ForceMode2D.Impulse);
                
                // Mark ability as used
                hasUsedAerialAbility = true;
                
                isCharging = false;
                isDragging = false;
                currentCharge = 0f;
                
                if (joystickIndicator != null)
                {
                    StartCoroutine(SnapJoystickToNeutral());
                }
            }
        }
        // Aerial ability already used - revert to normal drag behavior (no force applied)
        else
        {
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
            
            if (Input.GetMouseButton(0) && isDragging)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                Vector2 mouseDelta = new Vector2(mouseX, mouseY) * mouseSensitivity;
                dragDirection += mouseDelta;
                currentDragRadius = dragDirection.magnitude;
                if (currentDragRadius > maxDragDistance)
                {
                    dragDirection = dragDirection.normalized * maxDragDistance;
                    currentDragRadius = maxDragDistance;
                }
            }
            
            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                // Don't apply force - ability already used
                isDragging = false;
                if (joystickIndicator != null)
                {
                    StartCoroutine(SnapJoystickToNeutral());
                }
            }
        }
    }
    
    // TShape state: Normal jump on ground, small spin attack in air
    private void HandleTShapeInput()
    {
        // Ground behavior: normal jump
        if (isGrounded)
        {
            // Use normal jump logic when grounded
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
            
            if (Input.GetMouseButton(0) && isDragging)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                Vector2 mouseDelta = new Vector2(mouseX, mouseY) * mouseSensitivity;
                dragDirection += mouseDelta;
                currentDragRadius = dragDirection.magnitude;
                if (currentDragRadius > maxDragDistance)
                {
                    dragDirection = dragDirection.normalized * maxDragDistance;
                    currentDragRadius = maxDragDistance;
                }
            }
            
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
            return;
        }
        
        // Aerial ability not used yet - use charge behavior
        if (!hasUsedAerialAbility)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isCharging = true;
                currentCharge = 0f;
                isDragging = true;
                dragDirection = Vector2.zero;
                
                if (joystickIndicator != null)
                {
                    joystickIndicator.gameObject.SetActive(true);
                    joystickIndicator.position = transform.position;
                }
            }
            
            if (Input.GetMouseButton(0) && isCharging)
            {
                // Get mouse delta movement (any direction, but limited range)
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                
                Vector2 mouseDelta = new Vector2(mouseX, mouseY) * mouseSensitivity;
                
                // Add mouse delta to drag direction
                dragDirection += mouseDelta;
                
                // Calculate radius (distance from center)
                currentDragRadius = dragDirection.magnitude;
                
                // Limit drag distance (smaller for TShape)
                float tShapeMaxDrag = maxDragDistance * 0.5f; // Half the normal max drag for small jump
                if (currentDragRadius > tShapeMaxDrag)
                {
                    dragDirection = dragDirection.normalized * tShapeMaxDrag;
                    currentDragRadius = tShapeMaxDrag;
                }
                
                // Increase charge over time
                currentCharge += tShapeChargeRate * Time.deltaTime;
                currentCharge = Mathf.Min(currentCharge, tShapeMaxCharge);
            }
            
            if (Input.GetMouseButtonUp(0) && isCharging)
            {
                // Apply small force in aimed direction
                Vector2 forceDirection = -dragDirection.normalized;
                rb.AddForce(forceDirection * currentCharge * tShapeForcePower, ForceMode2D.Impulse);
                
                // Apply high torque for spin
                float torque = -forceDirection.x * currentCharge * tShapeTorquePower;
                rb.AddTorque(torque, ForceMode2D.Impulse);
                
                // Mark ability as used
                hasUsedAerialAbility = true;
                
                isCharging = false;
                isDragging = false;
                currentCharge = 0f;
                
                if (joystickIndicator != null)
                {
                    StartCoroutine(SnapJoystickToNeutral());
                }
            }
        }
        // Aerial ability already used - revert to normal drag behavior (no force applied)
        else
        {
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
            
            if (Input.GetMouseButton(0) && isDragging)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                Vector2 mouseDelta = new Vector2(mouseX, mouseY) * mouseSensitivity;
                dragDirection += mouseDelta;
                currentDragRadius = dragDirection.magnitude;
                if (currentDragRadius > maxDragDistance)
                {
                    dragDirection = dragDirection.normalized * maxDragDistance;
                    currentDragRadius = maxDragDistance;
                }
            }
            
            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                // Don't apply force - ability already used
                isDragging = false;
                if (joystickIndicator != null)
                {
                    StartCoroutine(SnapJoystickToNeutral());
                }
            }
        }
    }
    
    private void CheckForBreakables()
    {
        // Find all colliders within radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, tShapeBreakRadius);
        
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Breakable"))
            {
                // Destroy breakable object
                Destroy(col.gameObject);
                // You can add VFX or sound here
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
        
        // Apply force to rigidbody (always use normal force power for ground jumps)
        rb.AddForce(forceDirection * forcePower, ForceMode2D.Impulse);
        
        // Apply torque based on horizontal force direction
        // Positive torque (clockwise) when force is to the right
        // Negative torque (counter-clockwise) when force is to the left
        float torque = -forceDirection.x * torquePower;
        rb.AddTorque(torque, ForceMode2D.Impulse);
    }
    
    private void UpdateJoystickVisual()
    {
        switch (currentState)
        {
            case PlayerState.Normal:
                UpdateJoystickNormal();
                break;
            case PlayerState.TShape:
                // Only use TShape joystick when in air AND ability not used yet
                if (!isGrounded && !hasUsedAerialAbility)
                {
                    UpdateJoystickTShape(); // Small omni-directional joystick
                }
                else
                {
                    UpdateJoystickNormal(); // Normal joystick when grounded or ability used
                }
                break;
            case PlayerState.Stack:
                if (isGrounded || hasUsedAerialAbility)
                {
                    UpdateJoystickNormal(); // Normal joystick when grounded or ability used
                }
                else
                {
                    UpdateJoystickStack(); // Vertical only, below player
                }
                break;
            case PlayerState.Kite:
                if (isGrounded || hasUsedAerialAbility)
                {
                    UpdateJoystickNormal(); // Normal joystick when grounded or ability used
                }
                else
                {
                    UpdateJoystickKite(); // Horizontal only
                }
                break;
        }
        
        // Update joystick color based on CanJump
        if (joystickSpriteRenderer != null)
        {
            joystickSpriteRenderer.color = CanJump() ? joystickOriginalColor : cannotJumpColor;
        }
    }
    
    private void UpdateJoystickNormal()
    {
        // Set world position relative to player
        joystickIndicator.position = (Vector2)transform.position + dragDirection * 0.5f;
        // make joystick scale based on drag distance
        float scale = 0.1f + (currentDragRadius / maxDragDistance) * 5f;
        joystickIndicator.localScale = new Vector3(scale, scale, 1f);
    }
    
    private void UpdateJoystickStack()
    {
        // Position joystick below player only
        float offset = (currentCharge / stackMaxCharge) * maxDragDistance * 0.5f;
        joystickIndicator.position = (Vector2)transform.position + Vector2.down * offset;
        
        // Scale based on charge
        float scale = 0.1f + (currentCharge / stackMaxCharge) * 3.5f;
        joystickIndicator.localScale = new Vector3(scale, scale, 1f);
    }
    
    private void UpdateJoystickKite()
    {
        // Position joystick horizontally from player (X-axis only)
        float horizontalOffset = dragDirection.x * 0.5f;
        joystickIndicator.position = (Vector2)transform.position + new Vector2(horizontalOffset, 0f);
        
        // Scale based on charge
        float scale = 0.1f + (currentCharge / kiteMaxCharge) * 3.5f;
        joystickIndicator.localScale = new Vector3(scale, scale, 1f);
    }
    
    private void UpdateJoystickTShape()
    {
        // Small omni-directional joystick (any direction, limited range)
        joystickIndicator.position = (Vector2)transform.position + dragDirection * 0.5f;
        
        // Smaller scale for TShape
        float tShapeMaxDrag = maxDragDistance * 0.5f;
        float scale = 0.1f + (currentDragRadius / tShapeMaxDrag) * 2.0f; // Smaller max scale
        joystickIndicator.localScale = new Vector3(scale, scale, 1f);
    }
}
