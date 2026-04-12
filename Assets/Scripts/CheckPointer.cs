using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointer : MonoBehaviour
{
    private GameController _gc;
    [SerializeField] private Transform _spawnTransform;
    [SerializeField] private Sprite _bgSprite;

    // prevent going back in checkpoints
    private bool _passed = false;

    void Start()
    {
        _gc = GameController.Instance;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("triggered by");
        if(other.CompareTag("Player") && !_passed)
        {
            Debug.Log("valid");
            _gc.UpdateCheckpoint(_spawnTransform.position);
            _passed = true;
            
            if (_bgSprite != null)
            {
                _gc.SetBackground(_bgSprite);
            }
        }
    }

}
