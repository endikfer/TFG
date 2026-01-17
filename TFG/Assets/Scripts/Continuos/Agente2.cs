using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Agente2 : Agent
{
    [SerializeField] private PrometeoCarController _prometeoCarController;
    [SerializeField] private CheckPointsManager _checkpointManager;

    [SerializeField] private GameObject obj;
    [SerializeField] private GameObject area;

    [SerializeField] private int numRays = 11;
    [SerializeField] private float maxRayDistance = 10f;
    [SerializeField] private LayerMask obstacleMask;

    public bool salidaDePista = false;

    private void Start()
    {
        _checkpointManager.reachedCheckpoint += OnCheckpointReached;
    }

    private void Update()
    {
        if (salidaDePista)
        {
            HandleOffTrack();
        }
    }

    public override void OnEpisodeBegin()
    {
        ResetCar();

        foreach (var checkpoint in _checkpointManager.checkpp.checkPoints)
        {
            checkpoint.ResetTrigger();
        }
    }

    // ⭐ ACCIONES CONTINUAS
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float steering = actionBuffers.ContinuousActions[0];   // [-1, 1]
        float throttle = actionBuffers.ContinuousActions[1];   // [-1, 1]

        // Movimiento suave
        _prometeoCarController.SetSteering(steering);
        _prometeoCarController.SetThrottle(throttle);

        if (_checkpointManager.nextCheckPointToReach == null) return;

        // ⭐ Dirección al checkpoint
        Vector3 dirToCheckpoint =
            (_checkpointManager.nextCheckPointToReach.transform.position - obj.transform.position).normalized;

        // ⭐ Alineación coche-checkpoint (muy importante)
        float alignment = Vector3.Dot(obj.transform.forward, dirToCheckpoint);

        // ⭐ Reward por mirar bien hacia donde toca
        AddReward(alignment * 0.01f);

        // ⭐ Reward por velocidad SOLO si está razonablemente alineado
        if (alignment > 0.5f)
        {
            AddReward(_prometeoCarController.carSpeed * 0.001f);
        }

        // ⭐ Penalización muy pequeña por paso de tiempo
        AddReward(-0.0005f);

        if (StepCount >= MaxStep)
        {
            ResetCar();
            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (_checkpointManager.nextCheckPointToReach == null) return;

        // 1️⃣ Dirección al checkpoint
        Vector3 dirToCheckpoint =
            (_checkpointManager.nextCheckPointToReach.transform.position - obj.transform.position).normalized;
        sensor.AddObservation(dirToCheckpoint); // 3

        // 2️⃣ Velocidad normalizada
        sensor.AddObservation(_prometeoCarController.carSpeed / _prometeoCarController.maxSpeed); // 1

        // 3️⃣ Ángulo relativo
        float angle = Vector3.SignedAngle(
            obj.transform.forward,
            dirToCheckpoint,
            Vector3.up
        ) / 180f;
        sensor.AddObservation(angle); // 1

        float angleStep = 180f / (numRays - 1); // cubre 180 grados adelante
        for (int i = 0; i < numRays; i++)
        {
            float angle2 = -90f + i * angleStep; // -90 a +90 grados
            Vector3 dir = Quaternion.Euler(0, angle2, 0) * obj.transform.forward;

            if (Physics.Raycast(obj.transform.position + Vector3.up * 0.5f, dir, out RaycastHit hit, maxRayDistance, obstacleMask))
            {
                sensor.AddObservation(hit.distance / maxRayDistance); // normalizado 0-1
            }
            else
            {
                sensor.AddObservation(1f); // sin colisión → máximo alcance
            }
        }

    }

    // ⭐ Heurística adaptada a acciones continuas
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;

        actions[0] = Input.GetAxis("Horizontal"); // A/D
        actions[1] = Input.GetAxis("Vertical");   // W/S
    }

    public void ScoredAGoal()
    {
        SetReward(100f);
        EndEpisode();
        Debug.Log("META COMPLETADA");
    }

    public void ResetCar()
    {
        obj.transform.position = area.transform.position;
        obj.transform.rotation = Quaternion.Euler(0, -90, 0);

        _checkpointManager.ResetCheckpoints();

        _prometeoCarController.carSpeed = 0;
        _prometeoCarController.Start();
    }

    public void HandleOffTrack()
    {
        AddReward(-0.2f); // ⭐ penalización suave inicial
        ResetCar();
        EndEpisode();
    }

    public void CheckpointReward()
    {
        AddReward(1f); // ⭐ Ajusta el valor de la recompensa según convenga
        Debug.Log("Checkpoint superado, reward otorgada.");
    }

    private void OnCheckpointReached(CheckPoint2 checkpoint)
    {
        CheckpointReward();
    }
}
