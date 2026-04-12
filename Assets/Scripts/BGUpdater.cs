using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGUpdater : MonoBehaviour
{
    private GameController _gameController;
    private SpriteRenderer _spriteRend;

    // Start is called before the first frame update
    void Start()
    {
        _gameController = GameController.Instance;

        //subsscripbe to update
        _gameController.UpdateBackground += updateBG;

        _spriteRend = GetComponent<SpriteRenderer>();
    }

    void updateBG(Sprite sprite)
    {
        _spriteRend.sprite = sprite;
    }

}
