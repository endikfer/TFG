using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Agente1_0 : Agent
{
    // ─── Referencias dinámicas (ya NO se asignan desde el Inspector) ───
    [SerializeField] private CarController _prometeoCarController;
    private CheckPointsManager1_0 _checkpointManager;

    [SerializeField] private GameObject obj;
    private Transform spawn;
    private Vector3 _spawnPos;
    private Quaternion _spawnRot;
    private bool _spawnValido = false;

    // ─── Estos SÍ se pueden dejar en el Inspector ───
    [SerializeField] private int numRays = 11;
    [SerializeField] private float maxRayDistance = 10f;
    [SerializeField] private LayerMask obstacleMask;

    public bool salidaDePista = false;
    private bool _isInitialized = false;
    private bool _reseteando = false;
    private bool _circuitoListo = false;

    // ──────────────────────────────────────────────────────────────────────
    // EVENTOS ESTÁTICOS
    //
    // OnAgentReady    → se dispara cuando el agente termina su Start() y está
    //                   listo para recibir referencias.
    //
    // OnNuevoEpisodio → se dispara al inicio de cada episodio ML, ANTES de
    //                   ResetCar(). Lo escucha EntrenamientoManager.
    // ──────────────────────────────────────────────────────────────────────
    public static Agente1_0 Instance { get; private set; }
    public static event System.Action OnAgentReady;
    public static event System.Action OnNuevoEpisodio;

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

        if (_prometeoCarController == null)
        {
            Debug.LogError("[Agente1_0] CarController no encontrado.");
            return;
        }

        _checkpointManager.reachedCheckpoint += OnCheckpointReached;

        _isInitialized = true;
        OnAgentReady?.Invoke();
    }

    private void OnDestroy()
    {
        // El agente nunca se destruye, pero por seguridad limpiamos el evento
        if (_checkpointManager != null)
            _checkpointManager.reachedCheckpoint -= OnCheckpointReached;
    }

    private void Update()
    {
        if (!_isInitialized) return;

        if (salidaDePista && !_reseteando)
            HandleOffTrack();
    }

    public override void OnEpisodeBegin()
    {
        OnNuevoEpisodio?.Invoke();

        if (!_isInitialized) return;

        // Primera vez: inicializar checkpoints desde escena
        if (!_checkpointManager._circuitoListo)
            _checkpointManager.InicializarCheckpoints();

        if (RaceManager.Instance != null && RaceManager.Instance.VueltasNecesarias == 0)
            RaceManager.Instance.InicializarCarrera(0f);

        // Mostrar HUD la primera vez
        if (RaceManager.Instance != null)
        {
            RaceManager.Instance?.ResetCarrera();
            RaceManager.Instance.raceHUD?.MostrarHUD();
        }
            

        if (!_spawnValido) return;

        ResetCar();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

        //if (!_isInitialized) return;

        //if (salidaDePista && !_reseteando)
        //    HandleOffTrack();


        //if (!_isInitialized) return;

        float steering = actionBuffers.ContinuousActions[0];
        float throttle = actionBuffers.ContinuousActions[1];
        float brake = actionBuffers.ContinuousActions[2];

        _prometeoCarController.SetSteering(steering);
        _prometeoCarController.SetThrottle(throttle);
        _prometeoCarController.SetBrake(brake);

        //MoveAgent(actionBuffers.DiscreteActions);

        //if (_checkpointManager.nextCheckPointToReach == null) return;

        //Vector3 dirToCheckpoint =
        //    (_checkpointManager.nextCheckPointToReach.transform.position - obj.transform.position).normalized;

        //float alignment = Vector3.Dot(obj.transform.forward, dirToCheckpoint);

        //if (alignment < 0f)
        //{
        //    AddReward(alignment * 0.02f);
        //}
        //else
        //{
        //    AddReward(alignment * 0.01f);
        //    float projectedSpeed = Vector3.Dot(_prometeoCarController.carRigidbody.linearVelocity, dirToCheckpoint);
        //    AddReward(projectedSpeed * 0.001f);
        //}

        //float uphill = Vector3.Dot(_prometeoCarController.carRigidbody.linearVelocity.normalized, Vector3.up);

        //float slip = Mathf.Abs(_prometeoCarController.localVelocityX);
        //float slipFactor = uphill > 0.1f ? 0.3f : 1f;
        //AddReward(-slip * 0.01f * slipFactor);

        ////if (uphill > 0.1f && throttle > 0.5f)
        ////    AddReward(0.002f);

        //if (uphill > 0.1f && _prometeoCarController.carSpeed < 0.3f * _prometeoCarController.maxSpeed)
        //    AddReward(-0.005f);

        //float distToCheckpoint = Vector3.Distance(
        //    obj.transform.position,
        //    _checkpointManager.nextCheckPointToReach.transform.position
        //);

        //AddReward(distToCheckpoint < 5f ? -0.0001f : -0.0005f);

        //if (StepCount >= MaxStep)
        //{
        //    ResetCar();
        //    EndEpisode();
        //}
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!_isInitialized || _checkpointManager == null || _checkpointManager.nextCheckPointToReach == null)
        {
            sensor.AddObservation(Vector3.zero); // dirToCheckpoint
            sensor.AddObservation(Vector3.zero); // dirToNextNext
            sensor.AddObservation(0f);           // speed
            sensor.AddObservation(0f);           // angle
            for (int i = 0; i < numRays; i++) sensor.AddObservation(1f);
            Debug.LogWarning("[Agente1_0] CollectObservations: agente no inicializado o checkpoint nulo. Agregando observaciones vacías.");
            return;
        }


        Vector3 dirToCheckpoint =
            (_checkpointManager.nextCheckPointToReach.transform.position - obj.transform.position).normalized;
        sensor.AddObservation(dirToCheckpoint);

        int currentIndex = _checkpointManager.GetCheckpointIndex();
        var checkpoints = _checkpointManager.checkpoints;

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


    //public override void Heuristic(in ActionBuffers actionsOut) {
    //    var actions = actionsOut.DiscreteActions;

    //    Debug.LogWarning("entro en heuristica");

    //    if (Input.GetKey(KeyCode.W))
    //    {
    //        actions[1] = 2;
    //    }
    //    if (Input.GetKey(KeyCode.S))
    //    {
    //        actions[1] = 1;
    //    }

    //    if (Input.GetKey(KeyCode.A))
    //    {
    //        actions[0] = 2;
    //    }
    //    if (Input.GetKey(KeyCode.D))
    //    {
    //        actions[0] = 1;
    //    }

    //    if ((!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)))
    //    {
    //        actions[0] = 0;
    //    }
    //}

    public void ScoredAGoal()
    {
        SetReward(100f);
        EndEpisode();
        Debug.Log("META COMPLETADA");
    }

    /// <summary>
    /// Llamado por CircuitoInicializador cada vez que hay un circuito nuevo.
    /// Actualiza las coordenadas de spawn. El -90° se aplica aquí, una sola vez.
    /// </summary>
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
        Debug.Log($"[Agente1_0] SpawnPoint actualizado: {_spawnPos}");
    }

    public void ResetCar()
    {
        spawn = BuscarSpawnPoint();
        EstablecerSpawnPoint(spawn);

        if (!_spawnValido) return;

        //Rigidbody rb = _prometeoCarController.carRigidbody;
        //rb.linearVelocity = Vector3.zero;
        //rb.angularVelocity = Vector3.zero;
        //rb.position = _spawnPos;
        //rb.rotation = _spawnRot;


        obj.transform.position = _spawnPos;
        obj.transform.rotation = _spawnRot;

        salidaDePista = false;

        _checkpointManager.ResetCheckpoints();
        //_prometeoCarController.carSpeed = 0;
        //_prometeoCarController.ResetCarState();
    }

    public void HandleOffTrack()
    {
        Debug.LogWarning("[Agente1_0] HandleOffTrack disparado.");
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

    public void NotificarCircuitoListo()
    {
        _circuitoListo = true;
    }

    public void BloquearHastaCircuito()
    {
        _circuitoListo = false;
    }

    private Transform BuscarSpawnPoint()
    {
        // Busca en toda la escena un transform llamado "P1" 
        // dentro de un objeto "Posiciones de salida"

        Transform spawnTransform;
        GameObject posicionesDeSalida = GameObject.Find("Posiciones de salida");

        if (posicionesDeSalida == null)
        {
            Debug.LogWarning("[Agente1_0] No se encontró 'Posiciones de salida' en la escena. " +
                             "Usando posición actual como spawn de emergencia.");
            spawnTransform = obj.transform;
            return spawnTransform;
        }

        spawnTransform = posicionesDeSalida.transform.Find("P1");

        if (spawnTransform == null)
        {
            Debug.LogWarning("[Agente1_0] No se encontró 'P1' dentro de 'Posiciones de salida'. " +
                             "Usando posición actual como spawn de emergencia.");
            spawnTransform = obj.transform;
            return spawnTransform;
        }
        return spawnTransform;
    }
}