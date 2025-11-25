using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class check1 : MonoBehaviour
{


    [SerializeField] private CheckPointManager manager;
    

    void Start()
    {

    }
    
    void OnSceneLoaded(){
   
    }

    private void OnTriggerEnter(Collider other)
    {
      
        if (manager != null)
        {
            manager.CheckPointReached(this);
        } 
    }

}
