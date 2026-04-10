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
