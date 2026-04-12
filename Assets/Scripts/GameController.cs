using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public Player playerScript { get; private set; }

    public GameObject player;

    // use these to toggle the popup and any other actions like audio
    public delegate void SetBool(bool isState);
    public delegate void Trigger();
    public event SetBool ToggleNoseJob;

    public event SetBool ToggleSettingsMenu;

    public delegate void setBG(Sprite bgSprite);
    public event setBG UpdateBackground;


    // settings for scene changing
    [SerializeField] private int _mainMenuSceneID = 0;
    [SerializeField] private int _levelSceneID = 1;
    public event Trigger SceneChangeTrigger;


    // for tracking checkpoints
    private Vector3 _activeCheckpoint;
    public event Trigger PlayerHasDied;


    // input actions because they suck
    [SerializeField] private PlayerInput _playerInput;
    private InputAction _escAction;

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

    void Start()
    {
        if (playerScript != null)
        {
            _activeCheckpoint = playerScript.transform.position;
        }

        _escAction = _playerInput.actions.FindAction("Esc");
        _escAction.performed += OnEsc;
    }

    // Register player reference (called from Player.Start)
    public void RegisterPlayer(Player player)
    {
        playerScript = player;
        if (_activeCheckpoint == Vector3.zero)
        {
            _activeCheckpoint = playerScript.transform.position;
        }
    }

    public void ToggleMouseLock(bool isLocked)
    {
        // keep unlocked in main menu
        if (isLocked && SceneManager.GetActiveScene().buildIndex != 0)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// turn on and off nose job popup menu. 
    /// isVisable is whether the menu will be set as visable or not (true/false)
    /// </summary>
    /// <param name="isVisable"></param>
    public void ToggleNoseJobMenu(bool isVisable)
    {
        Debug.Log("toggling menu " + (isVisable? "on" : "off"));
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
        if (playerScript != null)
        {
            playerScript.SetState(newstate);
        }
    }

    /// <summary>
    /// opens and closes settings
    /// </summary>
    /// <param name="isVisable"></param>
    public void OpenSettings(bool isVisable)
    {
        ToggleSettingsMenu?.Invoke(isVisable);
        ToggleMouseLock(!isVisable);
    }

    public void GotoLevelScene(float delay)
    {
        SceneChangeTrigger?.Invoke();
        // call scene change on delay
        StartCoroutine(ChangeScene(delay, _levelSceneID));
    }

    public void GotoMenuScene(float delay)
    {
        SceneChangeTrigger?.Invoke();
        // call scene change on delay
        StartCoroutine(ChangeScene(delay, _mainMenuSceneID));
    }

    private IEnumerator ChangeScene(float waitTime, int sceneID)
    {
        // call on delay to allow other code to finish executing
        yield return new WaitForSeconds(waitTime);

        // change scene on exit
        SceneManager.LoadScene(sceneID);
    }

    public void UpdateCheckpoint(Vector3 position)
    {
        _activeCheckpoint = position;
        Debug.Log("checkpoint saved: " + _activeCheckpoint);
    }

    public void KillPlayer()
    {
        // for other related things like blank screen
        PlayerHasDied?.Invoke();

        // reset player to checkpoint
        player.transform.eulerAngles = Vector3.zero;
        player.transform.position = _activeCheckpoint;

        Debug.Log("respawned Player");
    }

    public void OnEsc(InputAction.CallbackContext context)
    {
        OpenSettings(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetBackground(Sprite sprite)
    {
        UpdateBackground?.Invoke(sprite);
    }

}
