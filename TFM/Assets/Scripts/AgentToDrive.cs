using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.SceneManagement;

public class AgentToDrive : Agent {

    [SerializeField] private PrometeoCarController _prometeoCarController;

    [SerializeField] private CheckPointManager _checkpointManager;
    //[SerializeField] private CHpManager_SpawnManager _checkpointManager;


    [SerializeField] private GameObject obj; 
    [SerializeField] private GameObject area;
    [SerializeField] private GameObject conjuntoExternos;

    public bool salidaDePista = false;

    public float distance;

    public int contadorSalidaPista = 0;

    public float timeReset = 200.0f; 
    public float timeRemaining = 200.0f; // Tiempo total en segundos , cambiarlo en el UPDATE
    private bool timerIsRunning = true;



    public override void Initialize()
    {
        ResetCar();
        timeRemaining = timeReset; 
        timerIsRunning = true;
    }

    private void Update()
    {

        if (salidaDePista == true)
        {

            ResetCar(); //_checkpointManager.ResetCheckpoints();
            //AddReward(-100f);
            EndEpisode();
        }

        if(GetCumulativeReward() < -500)
        {
            ResetCar();//_checkpointManager.ResetCheckpoints();
            EndEpisode();

        }
        //AddReward(-0.1f);
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                // Resta el tiempo del temporizador en cada frame
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                ResetCar(); //_checkpointManager.ResetCheckpoints();
                EndEpisode();
                
                timeRemaining = 200.0f;
            }
        }

    }

    //Called each time it has timed-out or has reached the goal
    public override void OnEpisodeBegin()    {

        //ResetCar();

        //_checkpointManager.ResetCheckpoints();
        ResetCar();

    }


    

    // Lógica para recibir acciones del agente
    public override void OnActionReceived(ActionBuffers actionBuffers)    {

        // Move the agent using the action.
        MoveAgent(actionBuffers.DiscreteActions);

        // Penalty given each step to encourage agent to finish task quickly.
    }



    public void MoveAgent(ActionSegment<int> vectorAction)
    {

        //HACEMOS AQUI TODO EL TEMA DE GIROS Y DEMAS NUMERO 0 <-----

        int direction = (int)vectorAction[0];
        if (direction == 0)
        {
            _prometeoCarController.TurnLeft();
        }
        else if (direction == 1)
        {
            _prometeoCarController.TurnRight();
        }
        else if (direction == 2)
        {
            _prometeoCarController.ResetSteeringAngle();
        }


        // HACEMOS AQUI TODO EL TEMA DE ACELERACION MOVIMIENTO Y DEMAS NUMERO 1 <----- 


        int velcotiyCar = (int)vectorAction[1];
        if (velcotiyCar == 0)
        {
            _prometeoCarController.GoForward();
        }
        else if (velcotiyCar == 1)
        {
            _prometeoCarController.GoReverse();
        }
        else if (velcotiyCar == 2)
        {
            _prometeoCarController.Brakes();
        }



    }

    public override void CollectObservations(VectorSensor sensor)
    {

        sensor.AddObservation(_prometeoCarController.carSpeed);
        sensor.AddObservation(Vector3.Distance(_checkpointManager.nextCheckPointToReach.transform.position, obj.transform.position));
        float angleDiff = Quaternion.Angle(_checkpointManager.nextCheckPointToReach.transform.rotation, obj.transform.rotation);
        sensor.AddObservation(_checkpointManager.nextCheckPointToReach.transform.position);
        sensor.AddObservation(_checkpointManager.nextCheckPointToReach.transform.rotation);
        sensor.AddObservation(obj.transform.position);
        sensor.AddObservation(obj.transform.rotation);
    }

    public override void Heuristic(in ActionBuffers actionsOut){

        
        var actions = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.W))
        {
            actions[1] = 0; 
        }
        if (!Input.GetKey(KeyCode.W))
        {
            actions[1] = 1;
        }



        if (Input.GetKey(KeyCode.A))
        {
            actions[0] = 0;
        }
        if (Input.GetKey(KeyCode.D))
        {
            actions[0] = 1;
        }
        
        if ((!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)))
        {
            actions[0] = 2;
        }

    }

    public void ScoredAGoal()
    {
        // We use a reward of 5.
        SetReward(1000f);

        // By marking an agent as done AgentReset() will be called automatically.
        //_checkpointManager.ResetCheckpoints();
        timeRemaining = 200f;
        EndEpisode();
        Debug.Log("GOOOOOOOOOOOOOOOOOOAAAAAL!!! REACHED");
        ResetCar();
    }

    public void GiveReward(float rewardGiven)
    {
        // We use a reward of 5.
        AddReward(rewardGiven);

    }

    public void ResetCar()
    {


        obj.transform.position = area.transform.position;
        obj.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
        _checkpointManager.ResetCheckpoints();

        //RESETEA PROPIEDADES DEL COCHE
        _prometeoCarController.carSpeed = 0; 
        _prometeoCarController.Start(); //NOS AYUDA A RESETAR EL COCHE


    }

    public void Stop()
    {
        //RESETEA PROPIEDADES DEL COCHE
        _prometeoCarController.carSpeed = 0;
        _prometeoCarController.Start(); //NOS AYUDA A RESETAR EL COCHE
    }

    public void ResetCarFunct(Vector3 position, Quaternion orient)
    {


        obj.transform.position = position;
        obj.transform.rotation = orient;
    }

}

