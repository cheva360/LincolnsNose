using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoseJobMenu : MonoBehaviour
{
    private GameController _gameController = GameController.Instance;
    [SerializeField] private GameObject _noseJobPanel; 

    // store the nose state untill confirm sent
    private int _noseJob = 0;

    void Start()
    {
        // subscribe to ui event
        _gameController.ToggleNoseJob += ToggleNoseJobMenu;
    }

    public void ToggleNoseJobMenu(bool isVisable)
    {
        _noseJobPanel.SetActive(isVisable);
    }

    public void SetNoseJob(int noseJobInt)
    {
        // call update on gamecontroller
        _noseJob = noseJobInt;
        Debug.Log($"nosejob is now: {_noseJob}");
    }

    public void ConfirmJob()
    {
        // use event so that other listeners are called
        _gameController.ToggleNoseJobMenu(false);
    }

}
