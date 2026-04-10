using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Screen Shake Settings")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 0.3f;
    [SerializeField] private float shakeDecay = 1.5f;
    
    private Vector3 originalPosition;
    private float currentShakeDuration = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPosition = new Vector3(
            GameController.Instance.player.transform.position.x, 
            GameController.Instance.player.transform.position.y, 
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
