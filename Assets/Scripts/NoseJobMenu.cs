using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoseJobMenu : MonoBehaviour
{
    private GameController _gameController;
    [SerializeField] private GameObject _noseJobPanel; 

    // store the nose state untill confirm sent
    private int _noseJob = 0;

    void Start()
    {
        _gameController = GameController.Instance;

        // subscribe to ui event
        _gameController.ToggleNoseJob += ToggleVisability;
    }

    public void ToggleVisability(bool isVisable)
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
        // update player
        _gameController.SetPlayerNoseJob(_noseJob);

        // use event so that other listeners are called
        _gameController.ToggleNoseJobMenu(false);
    }

}
