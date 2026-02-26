using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Agente1_0 : Agent
{
    // ─── Referencias dinámicas (ya NO se asignan desde el Inspector) ───
    private CarController _prometeoCarController;
    private CheckPointsManager1_0 _checkpointManager;

    // obj = el propio coche (este GameObject)
    // area = punto de spawn, búscalo por Tag "SpawnPoint"
    private GameObject obj;
    private Transform spawnPoint;

    // ─── Estos SÍ se pueden dejar en el Inspector (son datos, no referencias a objetos que cargan tarde) ───
    [SerializeField] private int numRays = 11;
    [SerializeField] private float maxRayDistance = 10f;
    [SerializeField] private LayerMask obstacleMask;

    public bool salidaDePista = false;
    private bool _isInitialized = false;

    // ──────────────────────────────────────────────
    // EVENTO ESTÁTICO: el Agente avisa cuando está listo
    // CheckPoint2, Meta4 y CocheContacto5 escuchan esto
    // ──────────────────────────────────────────────
    public static Agente1_0 Instance { get; private set; }
    public static event System.Action OnAgentReady;

    private void Awake()
    {
        Instance = this;
        obj = this.gameObject;
    }

    private void Start()
    {
        // El circuito ya está cargado, así que el Manager existe
        _checkpointManager = CheckPointsManager1_0.Instance;

        if (_checkpointManager == null)
        {
            Debug.LogError("[Agente4] CheckPointsManager3 no encontrado. ¿Se cargó el circuito antes que el agente?");
            return;
        }

        // CarController está en este mismo GameObject o en sus hijos
        _prometeoCarController = GetComponentInChildren<CarController>();

        if (_prometeoCarController == null)
        {
            Debug.LogError("[Agente4] CarController no encontrado en el coche.");
            return;
        }

        // Punto de spawn: busca un GameObject con Tag "SpawnPoint" en la escena
        GameObject spawnObj = GameObject.FindWithTag("SpawnPoint");
        if (spawnObj != null)
            spawnPoint = spawnObj.transform;
        else
            Debug.LogWarning("[Agente4] No se encontró ningún GameObject con Tag 'SpawnPoint'. Se usará la posición inicial del agente.");

        // Suscribirse al evento de checkpoint
        _checkpointManager.reachedCheckpoint += OnCheckpointReached;

        _isInitialized = true;

        // Avisar a CheckPoint2, Meta4, CocheContacto5 de que el agente ya existe
        OnAgentReady?.Invoke();
    }

    private void OnDestroy()
    {
        // Limpiar suscripción y singleton al destruirse
        if (_checkpointManager != null)
            _checkpointManager.reachedCheckpoint -= OnCheckpointReached;

        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!_isInitialized) return;

        if (salidaDePista)
            HandleOffTrack();
    }

    public override void OnEpisodeBegin()
    {
        if (!_isInitialized) return;
        ResetCar();

        foreach (var checkpoint in _checkpointManager.checkpp.checkPoints)
            checkpoint.ResetTrigger();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (!_isInitialized) return;

        float steering = actionBuffers.ContinuousActions[0];
        float throttle  = actionBuffers.ContinuousActions[1];
        float brake     = actionBuffers.ContinuousActions[2];

        _prometeoCarController.SetSteering(steering);
        _prometeoCarController.SetThrottle(throttle);
        _prometeoCarController.SetBrake(brake);

        if (_checkpointManager.nextCheckPointToReach == null) return;

        Vector3 dirToCheckpoint =
            (_checkpointManager.nextCheckPointToReach.transform.position - obj.transform.position).normalized;

        float alignment = Vector3.Dot(obj.transform.forward, dirToCheckpoint);

        if (alignment < 0f)
        {
            AddReward(alignment * 0.02f);
        }
        else
        {
            AddReward(alignment * 0.01f);
            float projectedSpeed = Vector3.Dot(_prometeoCarController.carRigidbody.linearVelocity, dirToCheckpoint);
            AddReward(projectedSpeed * 0.001f);
        }

        float uphill = Vector3.Dot(_prometeoCarController.carRigidbody.linearVelocity.normalized, Vector3.up);

        float slip = Mathf.Abs(_prometeoCarController.localVelocityX);
        float slipFactor = uphill > 0.1f ? 0.3f : 1f;
        AddReward(-slip * 0.01f * slipFactor);

        if (uphill > 0.1f && throttle > 0.5f)
            AddReward(0.002f);

        if (uphill > 0.1f && _prometeoCarController.carSpeed < 0.3f * _prometeoCarController.maxSpeed)
            AddReward(-0.005f);

        float distToCheckpoint = Vector3.Distance(
            obj.transform.position,
            _checkpointManager.nextCheckPointToReach.transform.position
        );

        AddReward(distToCheckpoint < 5f ? -0.0001f : -0.0005f);

        if (StepCount >= MaxStep)
        {
            ResetCar();
            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!_isInitialized || _checkpointManager.nextCheckPointToReach == null) return;

        Vector3 dirToCheckpoint =
            (_checkpointManager.nextCheckPointToReach.transform.position - obj.transform.position).normalized;
        sensor.AddObservation(dirToCheckpoint);

        int currentIndex = _checkpointManager.GetCheckpointIndex();
        var checkpoints  = _checkpointManager.checkpp.checkPoints;

        if (currentIndex + 1 < checkpoints.Count)
        {
            Vector3 dirToNextNext =
                (checkpoints[currentIndex + 1].transform.position - obj.transform.position).normalized;
            sensor.AddObservation(dirToNextNext);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
        }

        sensor.AddObservation(_prometeoCarController.carSpeed / _prometeoCarController.maxSpeed);

        float angle = Vector3.SignedAngle(obj.transform.forward, dirToCheckpoint, Vector3.up) / 180f;
        sensor.AddObservation(angle);

        float angleStep = 180f / (numRays - 1);
        for (int i = 0; i < numRays; i++)
        {
            float rayAngle = -90f + i * angleStep;
            Vector3 dir = Quaternion.Euler(0, rayAngle, 0) * obj.transform.forward;

            if (Physics.Raycast(obj.transform.position + Vector3.up * 0.5f, dir, out RaycastHit hit, maxRayDistance, obstacleMask))
                sensor.AddObservation(hit.distance / maxRayDistance);
            else
                sensor.AddObservation(1f);
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
        if (spawnPoint != null)
        {
            obj.transform.position = spawnPoint.position;
            obj.transform.rotation = spawnPoint.rotation;
        }

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

    private void OnCheckpointReached(CheckPoint1_0 checkpoint)
    {
        AddReward(1f);
        AddReward(_prometeoCarController.carSpeed * 0.05f);
        Debug.Log("Checkpoint superado");
    }
}
