using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WallLimit : MonoBehaviour
{
    [SerializeField] private AgentToDrive kartAgent;
    [SerializeField] private CheckPointManager _cpmanager;
    //[SerializeField] private CheckPointManager manager;



    void Start()
    {


    }
    void OnSceneLoaded(){
        
       
    }


    private void OnCollisionEnter(Collision collision)
    {
        //kartAgent.ResetCar();
        //kartAgent.AddReward(-5f);
        //kartAgent.EndEpisode();
    }

    private void OnCollisionStay(Collision collision)
    {
        //kartAgent.AddReward(-5f);
    }

}
