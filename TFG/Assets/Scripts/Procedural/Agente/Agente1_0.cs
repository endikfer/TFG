using System.Collections.Generic;
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
    private Vector3 _spawnPos;
    private Quaternion _spawnRot;
    private bool _spawnValido = false;

    // ─── Estos SÍ se pueden dejar en el Inspector (son datos, no referencias a objetos que cargan tarde) ───
    [SerializeField] private int numRays = 11;
    [SerializeField] private float maxRayDistance = 10f;
    [SerializeField] private LayerMask obstacleMask;

    public bool salidaDePista = false;
    private bool _isInitialized = false;
    private bool _reseteando = false; // Evita que HandleOffTrack se llame varias veces seguidas

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
        _checkpointManager = CheckPointsManager1_0.Instance;

        if (_checkpointManager == null)
        {
            Debug.LogError("[Agente1_0] CheckPointsManager1_0 no encontrado.");
            return;
        }

        _prometeoCarController = GetComponentInChildren<CarController>();

        if (_prometeoCarController == null)
        {
            Debug.LogError("[Agente1_0] CarController no encontrado.");
            return;
        }

        // Suscribirse al evento del circuito para buscar el spawnPoint
        // cuando la pieza inicial ya esté instanciada
        CircuitoEventos.OnCircuitoListoParaAgente += OnCircuitoListo;

        _checkpointManager.reachedCheckpoint += OnCheckpointReached;

        _isInitialized = true;
        OnAgentReady?.Invoke();
    }

    private void OnDestroy()
    {
        CircuitoEventos.OnCircuitoListoParaAgente -= OnCircuitoListo;

        if (_checkpointManager != null)
            _checkpointManager.reachedCheckpoint -= OnCheckpointReached;

        if (Instance == this)
            Instance = null;
    }

    private void OnCircuitoListo(List<PiezaCircuito> piezasOrdenadas)
    {
        if (piezasOrdenadas.Count == 0) return;

        Transform posicionesSalida = piezasOrdenadas[0].transform.Find("Posiciones de salida");

        if (posicionesSalida == null)
        {
            Debug.LogError("[Agente1_0] No se encontró 'Posiciones de salida' en la pieza inicial.");
            return;
        }

        spawnPoint = posicionesSalida.Find("P1");

        if (spawnPoint == null)
        {
            Debug.LogError("[Agente1_0] No se encontró 'P1' dentro de 'Posiciones de salida'.");
            _spawnValido = false;
        }
        else
        {
            // Guardamos posición y rotación como VALORES, no como referencia.
            // Así si el circuito se regenera y destruye la pieza, los valores siguen siendo válidos
            // hasta que llegue el próximo OnCircuitoListo con los nuevos.
            _spawnPos = spawnPoint.position;
            _spawnRot = spawnPoint.rotation;
            _spawnValido = true;
            Debug.Log($"[Agente1_0] SpawnPoint actualizado: {_spawnPos}");
        }

        // NO nos desuscribimos: si el circuito se regenera necesitamos actualizar el spawn
    }

    private void Update()
    {
        if (!_isInitialized) return;

        if (salidaDePista && !_reseteando)
            HandleOffTrack();
    }

    public override void OnEpisodeBegin()
    {
        if (!_isInitialized) return;
        ResetCar();

        foreach (var checkpoint in _checkpointManager.checkpp.checkPoints)
            checkpoint.ResetTrigger();

        // Reiniciar contador de vueltas de la carrera
        RaceManager.Instance?.ResetCarrera();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (!_isInitialized) return;

        float steering = actionBuffers.ContinuousActions[0];
        float throttle = actionBuffers.ContinuousActions[1];
        float brake = actionBuffers.ContinuousActions[2];

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
        var checkpoints = _checkpointManager.checkpp.checkPoints;

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

    public void EstablecerSpawnPoint(Transform spawn)
    {
        if (spawn == null)
        {
            Debug.LogError("[Agente1_0] EstablecerSpawnPoint recibió null.");
            _spawnValido = false;
            return;
        }

        _spawnPos = spawn.position;
        _spawnRot = spawn.rotation * Quaternion.Euler(0f, -90f, 0f);
        _spawnValido = true;
        Debug.Log($"[Agente1_0] SpawnPoint recibido desde CircuitoInicializador: {_spawnPos}");
    }

    public void ResetCar()
    {
        if (!_spawnValido)
        {
            Debug.LogError("[Agente1_0] No se puede resetear el coche porque el spawnPoint no es válido.");
        }
        else
        {
            var rb = _prometeoCarController.carRigidbody;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = _spawnPos;
            rb.rotation = _spawnRot;

            obj.transform.position = _spawnPos;
            obj.transform.rotation = _spawnRot;
        }

        salidaDePista = false;
        _checkpointManager.ResetCheckpoints();
        _prometeoCarController.carSpeed = 0;
        _prometeoCarController.ResetCarState();
    }

    public void HandleOffTrack()
    {
        _reseteando = true;
        AddReward(-0.2f);
        ResetCar();
        EndEpisode();
        _reseteando = false;
    }

    private void OnCheckpointReached(CheckPoint1_0 checkpoint)
    {
        AddReward(1f);
        AddReward(_prometeoCarController.carSpeed * 0.05f);
        Debug.Log("Checkpoint superado");
    }
}