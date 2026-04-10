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
    
    [Header("Visual Feedback")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPointCount = 20;
    [SerializeField] private Transform joystickIndicator;
    
    private Rigidbody2D rb;
    private float currentDragRadius;
    private Vector2 dragDirection;
    private bool isDragging = false;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
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
        HandleInput();
        
        if (isDragging)
        {
            DrawTrajectory();
            UpdateJoystickVisual();
        }
    }
    
    private void HandleInput()
    {
        // Start dragging
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
        
        // Release and apply force
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            ApplyForce();
            isDragging = false;
            
            if (trajectoryLine != null)
            {
                trajectoryLine.enabled = false;
            }
            
            if (joystickIndicator != null)
            {
                joystickIndicator.gameObject.SetActive(false);
            }
        }
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
        float scale = 0.01f + (currentDragRadius / maxDragDistance) * 1f; // Scale between 0.01 and 0.51
        joystickIndicator.localScale = new Vector3(scale, scale, 1f);

    }
}
