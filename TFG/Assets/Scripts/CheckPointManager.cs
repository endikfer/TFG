using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{


    [SerializeField] private AgentToDrive kartAgent;
    public Boolean seleccionCamaras;
    //private AgentToDrive kartAgent;



    //public float MaxTimeToReachNextCheckpoint = 25f;
    public float TimeLeftSetting = 25f;
    private float TimeLeft;

    public check1 nextCheckPointToReach;

    public float distanceMaxToNext;

    public float distance; 


    private int CurrentCheckpointIndex;
    private List<check1> Checkpoints;

    private List<Camera> Camaras; 
    [SerializeField] private Checkpointss checkpp;
    [SerializeField] private camaraConjunto camarass;
    //private Checkpointss checkpp;

    private check1 lastCheckpoint;

    public event Action<check1> reachedCheckpoint; 

    void Start()
    {

        Camaras = camarass.camarass; //Cogemos toda la lista de camaras
        //kartAgent = FindObjectOfType<AgentToDrive>();
        //checkpp = FindObjectOfType<Checkpointss>();
        Checkpoints = checkpp.checkPoints;
        ResetCheckpoints();
        Vector3 kartAgentPosition = kartAgent.transform.position;
        Vector3 nextCheckpointPosition = nextCheckPointToReach.transform.position;



        distanceMaxToNext = Vector3.Distance(kartAgentPosition, nextCheckpointPosition);

        for (int i = 1; i < camarass.camarass.Count; i++)
        {
            camarass.camarass[i].gameObject.SetActive(false);
        }
    }

    void OnSceneLoaded(){

        //kartAgent = FindObjectOfType<AgentToDrive>();
        //checkpp = FindObjectOfType<Checkpointss>();
        Checkpoints = checkpp.checkPoints;
        ResetCheckpoints();

    }

    public void ResetCheckpoints()
    {

        if(seleccionCamaras == true) { 
            for (int i = 1; i < camarass.camarass.Count; i++)
            {
                camarass.camarass[i].gameObject.SetActive(false);
            }
        }
        CurrentCheckpointIndex = 0;
        //kartAgent = FindObjectOfType<AgentToDrive>();
        //checkpp = FindObjectOfType<Checkpointss>();
        Checkpoints = checkpp.checkPoints;
        //TimeLeft = MaxTimeToReachNextCheckpoint;
        Checkpoints = checkpp.checkPoints;

        //if (seleccionCamaras == true)
        //{
        //    camarass.camarass[0].gameObject.SetActive(true);
        //}

        SetNextCheckpoint();
    }

    private void Update()
    {
        TimeLeft -= Time.deltaTime;

        Vector3 kartAgentPosition = kartAgent.transform.position;
        Vector3 nextCheckpointPosition = nextCheckPointToReach.transform.position;

        distance = Vector3.Distance(kartAgentPosition, nextCheckpointPosition);

        Debug.Log(CurrentCheckpointIndex);
        
    }

    public void CheckPointReached(check1 checkpoint)
    {
        if (nextCheckPointToReach != checkpoint)
        {

            return;

        }
        
        lastCheckpoint = Checkpoints[CurrentCheckpointIndex];
        reachedCheckpoint?.Invoke(checkpoint);
        CurrentCheckpointIndex++;

        if (CurrentCheckpointIndex >= Checkpoints.Count)
        {
        }
        else
        {
            //kartAgent.GiveReward((50f) / Checkpoints.Count);
            kartAgent.AddReward( 10 * CurrentCheckpointIndex);
            SetNextCheckpoint();
            Vector3 kartAgentPosition = kartAgent.transform.position;
            Vector3 nextCheckpointPosition = nextCheckPointToReach.transform.position;

            distanceMaxToNext = Vector3.Distance(kartAgentPosition, nextCheckpointPosition);

        }

        if (seleccionCamaras == true)
        {

            if (CurrentCheckpointIndex == 4)
            {

                camarass.camarass[1].gameObject.SetActive(true);
            }

            if (CurrentCheckpointIndex == 7)
            {

                camarass.camarass[2].gameObject.SetActive(true);
            }

            if (CurrentCheckpointIndex == 10)
            {

                camarass.camarass[3].gameObject.SetActive(true);
            }
            if (CurrentCheckpointIndex == 11)
            {

                camarass.camarass[4].gameObject.SetActive(true);
            }
            if (CurrentCheckpointIndex == 12)
            {

                camarass.camarass[5].gameObject.SetActive(true);
            }
            if (CurrentCheckpointIndex == 15)
            {

                camarass.camarass[6].gameObject.SetActive(true);
            }
            if (CurrentCheckpointIndex == 16)
            {

                camarass.camarass[7].gameObject.SetActive(true);
            }

            if (CurrentCheckpointIndex == 18)
            {

                camarass.camarass[8].gameObject.SetActive(true);
            }
            if (CurrentCheckpointIndex == 19)
            {

                camarass.camarass[9].gameObject.SetActive(true);
            }
            if (CurrentCheckpointIndex == 20)
            {

                camarass.camarass[10].gameObject.SetActive(true);
            }
            if (CurrentCheckpointIndex == 22)
            {

                camarass.camarass[11].gameObject.SetActive(true);
            }
            if (CurrentCheckpointIndex == 23)
            {

                camarass.camarass[12].gameObject.SetActive(true);
            }
            if (CurrentCheckpointIndex == 27)
            {

                camarass.camarass[13].gameObject.SetActive(true);
            }
            if (CurrentCheckpointIndex == 29)
            {

                camarass.camarass[14].gameObject.SetActive(true);
            }
            if (CurrentCheckpointIndex == 30)
            {

                camarass.camarass[15].gameObject.SetActive(true);
            }
            if (CurrentCheckpointIndex == 31)
            {

                camarass.camarass[16].gameObject.SetActive(true);
            }
            if (CurrentCheckpointIndex == 32)
            {

                camarass.camarass[17].gameObject.SetActive(true);
            }
            if (CurrentCheckpointIndex == 33)
            {

                camarass.camarass[18].gameObject.SetActive(true);
            }
        }




    }

    private void SetNextCheckpoint()
    {
        if (Checkpoints.Count > 0)
        {
            //TimeLeft = MaxTimeToReachNextCheckpoint;
            nextCheckPointToReach = Checkpoints[CurrentCheckpointIndex];
            
        }
    }



    // Funcion para obtener el indice del ultimo checkpoint alcanzado por el coche
    public int GetCheckpointIndex()
    {
        return CurrentCheckpointIndex;
;
    }


}
