using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Position Settings")]
    [SerializeField] private float verticalOffset = 2.5f;

    [Header("Screen Shake Settings")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 0.3f;
    [SerializeField] private float shakeDecay = 1.5f;
    
    [Header("Parallax Settings")]
    [SerializeField] private Transform ParallaxBackground;
    [SerializeField] private float parallaxEffect = 0.5f; // 0 = no movement, 1 = moves with camera

    [Header("CameraZoom")]
    [SerializeField] private float maxZoomout = 9.5f;
    [SerializeField] private float maxZoomin = 7.5f;
    [SerializeField] private float zoomStep = 0.5f;
    [SerializeField] private float zoomSpeed = 5f;
    private float currentZoom = 8.5f;
    private float targetZoom = 8.5f;


    private Vector3 originalPosition;
    private float currentShakeDuration = 0f;
    private Vector3 previousCameraPosition;
    
    // Start is called before the first frame update
    void Start()
    {
        previousCameraPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPosition = new Vector3(
            GameController.Instance.player.transform.position.x, 
            GameController.Instance.player.transform.position.y + verticalOffset, 
            transform.position.z
        );
        
        // Apply screen shake if active
        if (currentShakeDuration > 0)
        {
            Vector3 shakeOffset = Random.insideUnitCircle * shakeMagnitude;
            transform.position = targetPosition + shakeOffset;
            
            currentShakeDuration -= Time.deltaTime * shakeDecay;
        }
        else
        {
            transform.position = targetPosition;
        }
        
        // Apply parallax effect
        if (ParallaxBackground != null)
        {
            Vector3 cameraDelta = transform.position - previousCameraPosition;
            Vector3 parallaxOffset = new Vector3(cameraDelta.x * parallaxEffect, cameraDelta.y * parallaxEffect, 0);
            ParallaxBackground.position += parallaxOffset;
        }
        
        previousCameraPosition = transform.position;

        // Handle scroll wheel zoom input
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput > 0f)
        {
            // Scroll up - zoom in (subtract from target)
            targetZoom -= zoomStep;
        }
        else if (scrollInput < 0f)
        {
            // Scroll down - zoom out (add to target)
            targetZoom += zoomStep;
        }
        
        // Clamp target zoom between min and max
        targetZoom = Mathf.Clamp(targetZoom, maxZoomin, maxZoomout);

        // Lerp camera zoom to target
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
    }

    public void TriggerShake()
    {
        currentShakeDuration = shakeDuration;
    }
    
    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        currentShakeDuration = duration;
    }
}
