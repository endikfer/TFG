using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

public class AgenteM : Agent
{
    [SerializeField] private CarController _prometeoCarController;
    [SerializeField] private CheckPointsManagerM _checkpointManager;

    [SerializeField] private GameObject obj;   // coche
    [SerializeField] private GameObject area;  // posición inicial

    //[SerializeField] private int numRays = 11;
    //[SerializeField] private float maxRayDistance = 10f;
    //[SerializeField] private LayerMask obstacleMask;

    //public bool salidaDePista = false;

    private void Start()
    {
        // Registrar este agente en el manager
        _checkpointManager.RegisterAgent(this);
        _checkpointManager.reachedCheckpoint += OnCheckpointReached;
    }

    public override void OnEpisodeBegin()
    {
        ResetCar();
        _checkpointManager.ResetCheckpointsForAgent(this);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float steering = actionBuffers.ContinuousActions[0];
        float throttle = actionBuffers.ContinuousActions[1];
        float brake = actionBuffers.ContinuousActions[2];

        _prometeoCarController.SetSteering(steering);
        _prometeoCarController.SetThrottle(throttle);
        _prometeoCarController.SetBrake(brake);

        CheckPointM nextCheckpoint = _checkpointManager.GetNextCheckpoint(this);
        if (nextCheckpoint == null) return;

        Vector3 dirToCheckpoint = (nextCheckpoint.transform.position - obj.transform.position).normalized;
        float alignment = Vector3.Dot(obj.transform.forward, dirToCheckpoint);

        AddReward(alignment * 0.01f);
        AddReward(-0.0005f);

        //if (alignment < 0)
        //    AddReward(alignment * 0.02f);
        //else
        //{
        //    AddReward(alignment * 0.01f);
        //    float projectedSpeed = Vector3.Dot(_prometeoCarController.carRigidbody.linearVelocity, dirToCheckpoint);
        //    AddReward(projectedSpeed * 0.001f);
        //}

        //AddReward(-Mathf.Abs(_prometeoCarController.localVelocityX) * 0.01f);
        //AddReward(-0.0005f);

        //if (StepCount >= MaxStep)
        //{
        //    ResetCar();
        //    EndEpisode();
        //}
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        CheckPointM nextCheckpoint = _checkpointManager.GetNextCheckpoint(this);
        if (nextCheckpoint == null) return;

        Vector3 dirToCheckpoint = (nextCheckpoint.transform.position - obj.transform.position).normalized;
        sensor.AddObservation(dirToCheckpoint);
        sensor.AddObservation(_prometeoCarController.carSpeed / _prometeoCarController.maxSpeed);

        //float angle = Vector3.SignedAngle(obj.transform.forward, dirToCheckpoint, Vector3.up) / 180f;
        //sensor.AddObservation(angle);

        //float angleStep = 180f / (numRays - 1);
        //for (int i = 0; i < numRays; i++)
        //{
        //    float angle2 = -90f + i * angleStep;
        //    Vector3 dir = Quaternion.Euler(0, angle2, 0) * obj.transform.forward;

        //    if (Physics.Raycast(obj.transform.position + Vector3.up * 0.5f, dir, out RaycastHit hit, maxRayDistance, obstacleMask))
        //        sensor.AddObservation(hit.distance / maxRayDistance);
        //    else
        //        sensor.AddObservation(1f);
        //}
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
        actions[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
    }

    public void ResetCar()
    {
        obj.transform.position = area.transform.position;


        //obj.transform.rotation = Quaternion.Euler(0, -90, 0);

        //_prometeoCarController.carSpeed = 0;
        obj.transform.rotation = Quaternion.identity;


        _prometeoCarController.ResetCarState();
    }

    public void HandleOffTrack()
    {
        AddReward(-0.2f);
        ResetCar();
        EndEpisode();
    }

    public void CheckpointReward()
    {
        AddReward(1f);
        Debug.Log($"Agente {name}: Checkpoint superado, reward otorgada.");
    }

    private void OnCheckpointReached(CheckPointM checkpoint, AgenteM agente)
    {
        if (agente == this)
            CheckpointReward();
    }
}
