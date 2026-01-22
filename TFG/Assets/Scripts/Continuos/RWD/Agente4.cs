using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

public class Agente4 : Agent
{
    [SerializeField] private CarController _prometeoCarController;
    [SerializeField] private CheckPointsManager3 _checkpointManager;

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

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float steering = actionBuffers.ContinuousActions[0]; // [-1,1]
        float throttle = actionBuffers.ContinuousActions[1]; // [-1,1]
        float brake = actionBuffers.ContinuousActions[2];    // [0,1]

        // --- Control del coche ---
        _prometeoCarController.SetSteering(steering);
        _prometeoCarController.SetThrottle(throttle);
        _prometeoCarController.SetBrake(brake);

        if (_checkpointManager.nextCheckPointToReach == null) return;

        // ⭐ Dirección al checkpoint
        Vector3 dirToCheckpoint =
            (_checkpointManager.nextCheckPointToReach.transform.position - obj.transform.position).normalized;

        // Alineación coche-checkpoint
        float alignment = Vector3.Dot(obj.transform.forward, dirToCheckpoint);

        // --- Recompensa por orientación ---
        if (alignment < 0f)
        {
            AddReward(alignment * 0.02f); // castigo por ir al revés
        }
        else
        {
            AddReward(alignment * 0.01f);

            // Recompensa por velocidad proyectada
            float projectedSpeed =
                Vector3.Dot(_prometeoCarController.carRigidbody.linearVelocity, dirToCheckpoint);
            AddReward(projectedSpeed * 0.001f);
        }

        // --- Penalización por derrape lateral ---
        float slipPenalty = Mathf.Abs(_prometeoCarController.localVelocityX) * 0.01f;
        AddReward(-slipPenalty);

        // --- Penalización por tiempo ---
        AddReward(-0.0005f);

        // --- Fin por límite de pasos ---
        if (StepCount >= MaxStep)
        {
            ResetCar();
            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (_checkpointManager.nextCheckPointToReach == null) return;

        // Dirección al checkpoint
        Vector3 dirToCheckpoint =
            (_checkpointManager.nextCheckPointToReach.transform.position - obj.transform.position).normalized;
        sensor.AddObservation(dirToCheckpoint); // 3

        // Velocidad normalizada
        sensor.AddObservation(
            _prometeoCarController.carSpeed / _prometeoCarController.maxSpeed
        ); // 1

        // Ángulo relativo
        float angle = Vector3.SignedAngle(
            obj.transform.forward,
            dirToCheckpoint,
            Vector3.up
        ) / 180f;
        sensor.AddObservation(angle); // 1

        // Raycasts frontales
        float angleStep = 180f / (numRays - 1);
        for (int i = 0; i < numRays; i++)
        {
            float rayAngle = -90f + i * angleStep;
            Vector3 dir = Quaternion.Euler(0, rayAngle, 0) * obj.transform.forward;

            if (Physics.Raycast(
                obj.transform.position + Vector3.up * 0.5f,
                dir,
                out RaycastHit hit,
                maxRayDistance,
                obstacleMask))
            {
                sensor.AddObservation(hit.distance / maxRayDistance);
            }
            else
            {
                sensor.AddObservation(1f);
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;

        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
        actions[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
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
        _prometeoCarController.ResetCarState();
    }

    public void HandleOffTrack()
    {
        AddReward(-0.2f);
        ResetCar();
        EndEpisode();
    }

    private void OnCheckpointReached(CheckPoint2 checkpoint)
    {
        AddReward(1f);
        Debug.Log("Checkpoint superado");
    }
}
