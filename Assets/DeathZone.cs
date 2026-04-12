using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private GameController _gc;


    void Start()
    {
        _gc = GameController.Instance;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            _gc.KillPlayer();
        }
    }

}
