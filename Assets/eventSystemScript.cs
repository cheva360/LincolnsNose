using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eventSystemScript : MonoBehaviour
{
    private static eventSystemScript instance;
    
    public static eventSystemScript Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("eventSystemScript instance is null!");
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
