using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoseJobMenu : MonoBehaviour
{
    private GameController _gameController;
    [SerializeField] private GameObject _noseJobPanel; 

    // store the nose state untill confirm sent
    private int _noseJob = 0;

    [SerializeField] private Transform _topAttachmentpoint;
    [SerializeField] private Transform _leftAttachemntPoint;
    [SerializeField] private Transform _secondNose;
    private float _targetRotation = 0f;
    private float _noseRotation = 0f;

    [SerializeField] private float _rotTweenSpeed = 70;


    void Start()
    {
        _gameController = GameController.Instance;

        // subscribe to ui event
        _gameController.ToggleNoseJob += ToggleVisability;

    }

    void Update()
    {
        if (_targetRotation < _noseRotation)
        {
            _noseRotation -= _rotTweenSpeed * Time.deltaTime;

            if (_targetRotation >= _noseRotation)
            {
                _noseRotation = _targetRotation;
            }

            _secondNose.eulerAngles = new Vector3(0, 0, _noseRotation);
        }

            
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

    public void RotateNose()
    {
        _targetRotation -= 90;

        // skip 270 rotation:
        if (_targetRotation % 360 <= -270)
        {
            // rotate again
            _targetRotation -= 90;
        }

        // update transform rotation
        //_secondNose.eulerAngles = new Vector3(0, 0, _targetRotation);
    }

    public void PutdownNose(Transform nose)
    {
        
    }

    public void PickupNose()
    {   
        // snap to finished rotation
        _noseRotation = _targetRotation;
        _secondNose.eulerAngles = new Vector3(0, 0, _noseRotation);
    }

}
