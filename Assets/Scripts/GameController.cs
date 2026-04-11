using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public Player playerScript { get; private set; }

    public GameObject player;

    // use these to toggle the popup and any other actions like audio
    public delegate void SetBool(bool isState);
    public event SetBool ToggleNoseJob;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

    /// <summary>
    /// turn on and off nose job popup menu. 
    /// isVisable is whether the menu will be set as visable or not (true/false)
    /// </summary>
    public void ToggleNoseJobMenu(bool isVisable)
    {
        ToggleNoseJob?.Invoke(isVisable);
    }


    // Start is called before the first frame update
    void Start()
    {
        //set state example
        //playerScript.SetState(Player.PlayerState.Normal);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
