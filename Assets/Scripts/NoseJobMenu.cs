using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NoseJobMenu : MonoBehaviour
{
    private GameController _gameController;
    [Header("Settings")]
    [SerializeField] private float timeTillMenuClose = 0.5f; // time after placing nose to close menu, allows player to see result before menu closes

    [Header("UI")]
    [SerializeField] private GameObject _noseJobPanel; 

    // store the nose state untill confirm sent
    private int _noseJob = 0;

    // anchors for snapping nose position
    [SerializeField] private Transform _topAnchor;
    [SerializeField] private Transform _leftAnchor;
    [SerializeField] private Transform _noneAnchor;
    [SerializeField] private float _maxSnapDist = 1f;


    [SerializeField] private Transform _dragNose;
    private float _targetRotation = 0f;
    private float _noseRotation = 0f;
    private int _rotState = 0;

    [SerializeField] private float _rotTweenSpeed = 70;
    private bool _finished = false;


    void Start()
    {
        _gameController = GameController.Instance;

        // subscribe to ui event
        _gameController.ToggleNoseJob += ToggleVisability;

        //start snapped to none anchor
        _dragNose.position = _noneAnchor.position;
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

            _dragNose.eulerAngles = new Vector3(0, 0, _noseRotation);
        }

    }

    public void ToggleVisability(bool isVisable)
    {
        _noseJobPanel.SetActive(isVisable);
    }

    public void ConfirmJob()
    {
        // Only allow transformation if a nose job was selected
        if (_noseJob > 0)
        {
            // update player state
            _gameController.SetPlayerNoseJob(_noseJob);
        }

        // use event so that other listeners are called
        _gameController.ToggleNoseJobMenu(false);
    }

    public void RotateNose()
    {
        // move back to _noneAnchor
        _dragNose.position = _noneAnchor.position;
        _finished = false;

        _targetRotation -= 90;
        _rotState += 1;

        // skip 270 rotation:
        if (_targetRotation % 360 <= -270)
        {
            // rotate again
            _targetRotation -= 90;
            _rotState = 0;
        }
    }

    public void PutDownNose(Transform nose)
    {
        // find closest anchor
        Vector3 _activeAnchor = Vector3.zero;
        switch (_rotState)
        {
            case 0: _activeAnchor = _topAnchor.position; break;
            case 1: _activeAnchor = _topAnchor.position; break;
            case 2: _activeAnchor = _leftAnchor.position; break;
        }

        Debug.Log(Vector3.Distance(_activeAnchor, nose.position));

        // check close enough
        if (Vector3.Distance(_activeAnchor, nose.position) < _maxSnapDist)
        {
            nose.position = _activeAnchor;
            _finished = true;
            _noseJob = _rotState + 1;
            
            // Auto-close and confirm after delay
            StartCoroutine(AutoConfirmAfterDelay());
        }
        else
        {
            nose.position = _noneAnchor.position;
            _noseJob = 0;
        }
    }

    public void PickupNose()
    {   
        // snap to finished rotation
        _noseRotation = _targetRotation;
        _dragNose.eulerAngles = new Vector3(0, 0, _noseRotation);
    }

    private IEnumerator AutoConfirmAfterDelay()
    {
        yield return new WaitForSeconds(timeTillMenuClose);
        ConfirmJob();
    }

}
