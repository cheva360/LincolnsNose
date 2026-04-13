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

    // Update is called once per frame
    void Update()
    {
        
    }
}
