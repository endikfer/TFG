using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CHpManager_SpawnManager : MonoBehaviour
{

    [SerializeField] private AgentToDrive kartAgent;
    //private AgentToDrive kartAgent;

    [SerializeField] private camaraConjunto camarass;

    //public float MaxTimeToReachNextCheckpoint = 25f;
    public float TimeLeftSetting = 25f;
    private float TimeLeft;

    public check1 nextCheckPointToReach;

    public float distanceMaxToNext;

    public float distance;


    private int CurrentCheckpointIndex;
    private List<check1> Checkpoints;
    [SerializeField] private Checkpointss checkpp;

    private check1 lastCheckpoint;

    public event Action<check1> reachedCheckpoint;

    public int contadorCheckPoints;

    void Start()
    {
        int randomNumber = UnityEngine.Random.Range(1, 29); // Genera un número aleatorio del 1 al 28 inclusive
        CurrentCheckpointIndex = randomNumber;
        Checkpoints = checkpp.checkPoints;
        ResetCheckpointsSpecial();
        contadorCheckPoints = 0;
    }

    public void ResetCheckpoints()
    {
        contadorCheckPoints = 0;
        CurrentCheckpointIndex = 0;
        Checkpoints = checkpp.checkPoints;
        Checkpoints = checkpp.checkPoints;
        SetNextCheckpoint();
    }

    public void ResetCheckpointsSpecial()
    {

        Checkpoints = checkpp.checkPoints;

        int randomNumber = UnityEngine.Random.Range(1, 29); // Genera un número aleatorio del 1 al 28 inclusive
        CurrentCheckpointIndex = randomNumber;

        contadorCheckPoints = 0;

        check1 cpSpawn = Checkpoints[CurrentCheckpointIndex - 1];

        Vector3 rotacion = cpSpawn.transform.rotation.eulerAngles;

        rotacion = rotacion + new Vector3(0, -90, 0);

        Vector3 position = cpSpawn.transform.position; 
        kartAgent.ResetCarFunct(position, Quaternion.Euler(rotacion));
        //RESETEA PROPIEDADES DEL COCHE
        kartAgent.Stop();

        SetNextCheckpoint();
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

        if (CurrentCheckpointIndex <= Checkpoints.Count)
        {

            contadorCheckPoints ++;

            if (contadorCheckPoints >= 5)
            {

                kartAgent.ScoredAGoal();

            }
            else
            {
                kartAgent.AddReward(10 * (contadorCheckPoints));
                SetNextCheckpoint();
            }

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
            nextCheckPointToReach = Checkpoints[CurrentCheckpointIndex];

        }
    }

    public int GetCheckpointIndex()
    {
        return CurrentCheckpointIndex;
        ;
    }


}

