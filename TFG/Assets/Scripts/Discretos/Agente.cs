using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Agente : Agent
{
    [SerializeField] private PrometeoCarController _prometeoCarController;

    [SerializeField] private ManagerCheckPoints _checkpointManager;

    [SerializeField] private GameObject obj;
    [SerializeField] private GameObject area;

    public bool salidaDePista = false;

    private float lastDistance;

    private void Update()
    {

        if (salidaDePista)
        {
            HandleOffTrack(); // reset + penalización
        }

        if (GetCumulativeReward() < -500)
        {
            ResetCar();
            EndEpisode();
        }
    }

    //Called each time it has timed-out or has reached the goal
    public override void OnEpisodeBegin()
    {
        ResetCar();
        lastDistance = _checkpointManager.distance;

        foreach (var checkpoint in _checkpointManager.checkpp.checkPoints)
        {
            checkpoint.ResetTrigger();
        }
    }

    // Lógica para recibir acciones del agente
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Move the agent using the action.
        MoveAgent(actionBuffers.DiscreteActions);

        if (_checkpointManager.nextCheckPointToReach == null) return;

        // Recompensa por acercarse al checkpoint
        float deltaDistance = lastDistance - _checkpointManager.distance;
        AddReward(deltaDistance * 0.1f);
        lastDistance = _checkpointManager.distance;

        // Penalización por velocidad baja
        if (_prometeoCarController.carSpeed < 0.1f)
            AddReward(-0.01f);

        AddReward(-0.001f);
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
        if (_checkpointManager.nextCheckPointToReach == null) return;

        // 1️⃣ Dirección al checkpoint (normalizada)
        Vector3 dirToCheckpoint = (_checkpointManager.nextCheckPointToReach.transform.position - obj.transform.position).normalized;
        sensor.AddObservation(dirToCheckpoint); // 3 floats

        // 2️⃣ Distancia normalizada al checkpoint
        float distanceToCheckpoint = _checkpointManager.distance / _checkpointManager.distanceMaxToNext;
        sensor.AddObservation(distanceToCheckpoint); // 1 float

        // 3️⃣ Velocidad normalizada
        sensor.AddObservation(_prometeoCarController.carSpeed / _prometeoCarController.maxSpeed); // 1 float

        // 4️⃣ Orientación relativa del coche al checkpoint
        Vector3 forward = obj.transform.forward;
        float angle = Vector3.SignedAngle(forward, dirToCheckpoint, Vector3.up) / 180f; // entre -1 y 1
        sensor.AddObservation(angle); // 1 float
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {


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
        SetReward(1000f);

        EndEpisode();
        Debug.Log("GOOOOOOOOOOOOOOOOOOAAAAAL!!! REACHED");
    }

    public void GiveReward(float rewardGiven)
    {
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

    public void HandleOffTrack()
    {
        AddReward(-1f); // penalización por salirse
        ResetCar();
        EndEpisode();
    }

}
