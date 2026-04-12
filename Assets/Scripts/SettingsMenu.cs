using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    private GameController _gameController;

    [SerializeField] private GameObject _settingsMenu;

    // Start is called before the first frame update
    void Start()
    {
        _gameController = GameController.Instance;

        _gameController.ToggleSettingsMenu += ToggleVisability;

        // start turned off
        _settingsMenu.SetActive(false);
    }

    public void CloseSettings()
    {
        _gameController.OpenSettings(false);
    }

    void ToggleVisability(bool isVisable)
    {
        _settingsMenu.SetActive(isVisable);
    }

    public void GotoMain()
    {
        _gameController.GotoMenuScene(0.1f);
    }

}
