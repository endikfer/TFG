using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WallLimitcp : MonoBehaviour{

    //[SerializeField] private CheckPointManager _cpmanager;
    private CheckPointManager _cpmanager;
    [SerializeField] private AgentToDrive kartAgent;
    void Start()
    {


    }
    void OnSceneLoaded(){


        
    }


    private void OnTriggerEnter(Collider other)
    {



        kartAgent.ScoredAGoal();


    }


}
