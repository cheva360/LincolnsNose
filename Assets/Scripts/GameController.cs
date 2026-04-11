using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public Player playerScript { get; private set; }

    public GameObject player;

    // use these to toggle the popup and any other actions like audio
    public delegate void SetBool(bool isState);
    public event SetBool ToggleNoseJob;

    public event SetBool ToggleSettingsMenu;

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

    public void ToggleMouseLock(bool isLocked)
    {
        // TODO: do scene id check so that mouse lock stays off when in main menu

        if (isLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    /// <summary>
    /// turn on and off nose job popup menu. 
    /// isVisable is whether the menu will be set as visable or not (true/false)
    /// </summary>
    /// <param name="isVisable"></param>
    public void ToggleNoseJobMenu(bool isVisable)
    {
        Debug.Log("toggling menu " + (isVisable? "off" : "on"));
        ToggleNoseJob?.Invoke(isVisable);
        ToggleMouseLock(!isVisable);
    }

    /// <summary>
    /// sets the players nosejob
    /// noseJob int is remaped to nosejob enum on player
    /// </summary>
    /// <param name="noseJob"></param>
    public void SetPlayerNoseJob(int noseJob)
    {
        Debug.Log("setting player state");

        // set state on player from supplied int
        Player.PlayerState newstate = (Player.PlayerState)noseJob;
        playerScript.SetState(newstate);
    }

    /// <summary>
    /// opens and closes settings
    /// </summary>
    /// <param name="isVisable"></param>
    public void OpenSettings(bool isVisable)
    {
        ToggleSettingsMenu?.Invoke(isVisable);
        //ToggleMouseLock(!isVisable);
    }
}
