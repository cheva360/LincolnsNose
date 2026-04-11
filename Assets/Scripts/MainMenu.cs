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

    }

    public void TransitionToVideo()
    {
        _mainMenu.SetActive(false);
        _videoAnimation.SetActive(true);

        _introPlayer.Play();
    }
}
