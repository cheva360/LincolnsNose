using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class mobileButton : MonoBehaviour
{
    [SerializeField] private Button button;


    // Start is called before the first frame update
    void Start()
    {
#if UNITY_WEBGL
        // Check if the platform is a mobile device
        bool isMobile = Application.isMobilePlatform;
        
        // Disable button if not on a mobile device
        if (!isMobile && button != null)
        {
            button.gameObject.SetActive(false);
        }
#endif
    }

    public void OnButtonClick()
    {
        Debug.Log("Mobile button clicked!");
        
        // Call ReleaseWashington on the player
        if (GameController.Instance != null && GameController.Instance.playerScript != null)
        {
            GameController.Instance.playerScript.ReleaseWashington();
        }
        else
        {
            Debug.LogError("Cannot release Washington - GameController or Player is null!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
