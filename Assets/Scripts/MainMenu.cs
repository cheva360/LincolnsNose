using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _videoAnimation;
    [SerializeField] private VideoPlayer _introPlayer;

    private GameController _gameController;


    // Start is called before the first frame update
    void Start()
    {
        // get game controller for scene management
        _gameController = GameController.Instance;

        // start with video off, main on
        _mainMenu.SetActive(true);
        _videoAnimation.SetActive(false);

        ShowMainMenu();

        // subscribe to settings event to close main ui
        _gameController.ToggleSettingsMenu += OnSettignsToggle;
    }

    void OnSettignsToggle(bool settingsOn)
    {
        // visability of main is opposite of settings
        _mainMenu.SetActive(!settingsOn);
        _videoAnimation.SetActive(false);
    }

    void ShowMainMenu()
    {
        // start with video off, main on
        _mainMenu.SetActive(true);
        _videoAnimation.SetActive(false);
    }

    public void TransitionToVideo()
    {
        _mainMenu.SetActive(false);
        _videoAnimation.SetActive(true);

        _introPlayer.Play();

        _gameController.GotoLevelScene(17f);
    }

    public void SkipIntro()
    {
        // call gamecontroller switch scene action
        _gameController.GotoLevelScene(0.1f);
    }

    public void OpenSettings()
    {
        // hide main menu
        _mainMenu.SetActive(false);

        // call game controller to open settings
        _gameController.OpenSettings(true);
    }
}
