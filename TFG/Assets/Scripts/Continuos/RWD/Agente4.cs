using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

public class Agente4 : Agent
{
    //[SerializeField] private CarController _prometeoCarController;
    //[SerializeField] private CheckPointsManager3 _checkpointManager;

    //[SerializeField] private GameObject obj;
    //[SerializeField] private GameObject area;

    //[SerializeField] private int numRays = 11;
    //[SerializeField] private float maxRayDistance = 10f;
    //[SerializeField] private LayerMask obstacleMask;

    //public bool salidaDePista = false;

    //private void Start()
    //{
    //    _checkpointManager.reachedCheckpoint += OnCheckpointReached;
    //}

    //private void Update()
    //{
    //    if (salidaDePista)
    //    {
    //        HandleOffTrack();
    //    }
    //}

    //public override void OnEpisodeBegin()
    //{
    //    ResetCar();

    //    foreach (var checkpoint in _checkpointManager.checkpp.checkPoints)
    //    {
    //        checkpoint.ResetTrigger();
    //    }
    //}

    //public override void OnActionReceived(ActionBuffers actionBuffers)
    //{
    //    float steering = actionBuffers.ContinuousActions[0]; // [-1,1]
    //    float throttle = actionBuffers.ContinuousActions[1]; // [-1,1]
    //    float brake = actionBuffers.ContinuousActions[2];    // [0,1]

    //    // --- Control del coche ---
    //    _prometeoCarController.SetSteering(steering);
    //    _prometeoCarController.SetThrottle(throttle);
    //    _prometeoCarController.SetBrake(brake);

    //    if (_checkpointManager.nextCheckPointToReach == null) return;

    //    // ⭐ Dirección al checkpoint
    //    Vector3 dirToCheckpoint =
    //        (_checkpointManager.nextCheckPointToReach.transform.position - obj.transform.position).normalized;

    //    // Alineación coche-checkpoint
    //    float alignment = Vector3.Dot(obj.transform.forward, dirToCheckpoint);

    //    // --- Recompensa por orientación ---
    //    if (alignment < 0f)
    //    {
    //        AddReward(alignment * 0.02f); // castigo por ir al revés
    //    }
    //    else
    //    {
    //        AddReward(alignment * 0.01f);

    //        // Recompensa por velocidad proyectada
    //        float projectedSpeed =
    //            Vector3.Dot(_prometeoCarController.carRigidbody.linearVelocity, dirToCheckpoint);
    //        AddReward(projectedSpeed * 0.001f);
    //    }

    //    // --- DETECCIÓN DE SUBIDA ---
    //    float uphill =
    //        Vector3.Dot(_prometeoCarController.carRigidbody.linearVelocity.normalized, Vector3.up);

    //    // --- Penalización lateral ADAPTATIVA ---
    //    float slip = Mathf.Abs(_prometeoCarController.localVelocityX);
    //    float slipFactor = uphill > 0.1f ? 0.3f : 1f;
    //    AddReward(-slip * 0.01f * slipFactor);

    //    // --- Recompensa por empujar en subida ---
    //    if (uphill > 0.1f && throttle > 0.5f)
    //    {
    //        AddReward(0.002f);
    //    }

    //    // --- Castigo por quedarse sin velocidad en subida ---
    //    if (uphill > 0.1f &&
    //        _prometeoCarController.carSpeed < 0.3f * _prometeoCarController.maxSpeed)
    //    {
    //        AddReward(-0.005f);
    //    }

    //    // --- Penalización por tiempo (adaptada a chicanes) ---
    //    float distToCheckpoint = Vector3.Distance(
    //        obj.transform.position,
    //        _checkpointManager.nextCheckPointToReach.transform.position
    //    );

    //    if (distToCheckpoint < 5f)
    //        AddReward(-0.0001f);
    //    else
    //        AddReward(-0.0005f);

    //    // --- Fin por límite de pasos ---
    //    if (StepCount >= MaxStep)
    //    {
    //        ResetCar();
    //        EndEpisode();
    //    }
    //}

    //public override void CollectObservations(VectorSensor sensor)
    //{
    //    if (_checkpointManager.nextCheckPointToReach == null) return;

    //    // Dirección al checkpoint
    //    Vector3 dirToCheckpoint =
    //        (_checkpointManager.nextCheckPointToReach.transform.position - obj.transform.position).normalized;
    //    sensor.AddObservation(dirToCheckpoint); // 3

    //    // --- Dirección al checkpoint siguiente (si existe) ---
    //    int currentIndex = _checkpointManager.GetCheckpointIndex();
    //    var checkpoints = _checkpointManager.checkpp.checkPoints;

    //    if (currentIndex + 1 < checkpoints.Count)
    //    {
    //        Vector3 dirToNextNext =
    //            (checkpoints[currentIndex + 1].transform.position - obj.transform.position).normalized;
    //        sensor.AddObservation(dirToNextNext); // 3
    //    }
    //    else
    //    {
    //        sensor.AddObservation(Vector3.zero);
    //    }

    //    // Velocidad normalizada
    //    sensor.AddObservation(
    //        _prometeoCarController.carSpeed / _prometeoCarController.maxSpeed
    //    ); // 1

    //    // Ángulo relativo
    //    float angle = Vector3.SignedAngle(
    //        obj.transform.forward,
    //        dirToCheckpoint,
    //        Vector3.up
    //    ) / 180f;
    //    sensor.AddObservation(angle); // 1

    //    // Raycasts frontales
    //    float angleStep = 180f / (numRays - 1);
    //    for (int i = 0; i < numRays; i++)
    //    {
    //        float rayAngle = -90f + i * angleStep;
    //        Vector3 dir = Quaternion.Euler(0, rayAngle, 0) * obj.transform.forward;

    //        if (Physics.Raycast(
    //            obj.transform.position + Vector3.up * 0.5f,
    //            dir,
    //            out RaycastHit hit,
    //            maxRayDistance,
    //            obstacleMask))
    //        {
    //            sensor.AddObservation(hit.distance / maxRayDistance);
    //        }
    //        else
    //        {
    //            sensor.AddObservation(1f);
    //        }
    //    }
    //}

    //public override void Heuristic(in ActionBuffers actionsOut)
    //{
    //    var actions = actionsOut.ContinuousActions;

    //    actions[0] = Input.GetAxis("Horizontal");
    //    actions[1] = Input.GetAxis("Vertical");
    //    actions[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
    //}

    //public void ScoredAGoal()
    //{
    //    SetReward(100f);
    //    EndEpisode();
    //    Debug.Log("META COMPLETADA");
    //}

    //public void ResetCar()
    //{
    //    obj.transform.position = area.transform.position;
    //    obj.transform.rotation = Quaternion.Euler(0, -90, 0);

    //    _checkpointManager.ResetCheckpoints();

    //    _prometeoCarController.carSpeed = 0;
    //    _prometeoCarController.ResetCarState();
    //}

    //public void HandleOffTrack()
    //{
    //    AddReward(-0.2f);
    //    ResetCar();
    //    EndEpisode();
    //}

    //private void OnCheckpointReached(CheckPoint2 checkpoint)
    //{
    //    AddReward(1f);
    //    AddReward(_prometeoCarController.carSpeed * 0.05f);
    //    Debug.Log("Checkpoint superado");
    //}

    [Header("Referencias")]
    [SerializeField] private CarController _prometeoCarController;
    [SerializeField] private CheckPointsManager3 _checkpointManager;
    [SerializeField] private GameObject obj;
    [SerializeField] private GameObject area;

    [Header("Configuración Raycasts")]
    [SerializeField] private int numRays = 15; // Aumentado para mejor detección
    [SerializeField] private float maxRayDistance = 15f; // Mayor distancia para anticipar
    [SerializeField] private LayerMask obstacleMask;

    [Header("Sistema de Recompensas")]
    [SerializeField] private float checkpointReward = 10f; // MUCHO mayor que antes
    [SerializeField] private float progressRewardScale = 0.02f; // Recompensa continua
    [SerializeField] private float speedRewardScale = 0.005f;
    [SerializeField] private float timePenaltyScale = 0.0001f; // REDUCIDA drásticamente
    [SerializeField] private float offTrackPenalty = -5f; // Reducida para no desmoralizar

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    public bool salidaDePista = false;

    // Variables de seguimiento
    private float lastDistanceToCheckpoint;
    private int checkpointsReachedThisEpisode = 0;
    private float totalRewardThisEpisode = 0f;
    private Vector3 lastPosition;

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

        // Reset checkpoints
        foreach (var checkpoint in _checkpointManager.checkpp.checkPoints)
        {
            checkpoint.ResetTrigger();
        }

        // Reset variables de tracking
        checkpointsReachedThisEpisode = 0;
        totalRewardThisEpisode = 0f;
        lastPosition = obj.transform.position;

        if (_checkpointManager.nextCheckPointToReach != null)
        {
            lastDistanceToCheckpoint = Vector3.Distance(
                obj.transform.position,
                _checkpointManager.nextCheckPointToReach.transform.position
            );
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float steering = actionBuffers.ContinuousActions[0]; // [-1,1]
        float throttle = actionBuffers.ContinuousActions[1]; // [-1,1]
        float brake = actionBuffers.ContinuousActions[2];    // [0,1]

        // Control del coche
        _prometeoCarController.SetSteering(steering);
        _prometeoCarController.SetThrottle(throttle);
        _prometeoCarController.SetBrake(brake);

        if (_checkpointManager.nextCheckPointToReach == null) return;

        // ═══════════════════════════════════════════════════════
        // 🎯 SISTEMA DE RECOMPENSAS MEJORADO
        // ═══════════════════════════════════════════════════════

        Vector3 carPos = obj.transform.position;
        Vector3 checkpointPos = _checkpointManager.nextCheckPointToReach.transform.position;
        Vector3 dirToCheckpoint = (checkpointPos - carPos).normalized;

        float currentDistance = Vector3.Distance(carPos, checkpointPos);

        // 1️⃣ RECOMPENSA POR PROGRESO CONTINUO (cada frame que se acerca)
        float progressDelta = lastDistanceToCheckpoint - currentDistance;
        if (progressDelta > 0)
        {
            float progressReward = progressDelta * progressRewardScale;
            AddReward(progressReward);
        }
        else
        {
            // Penalización pequeña si se aleja
            AddReward(progressDelta * progressRewardScale * 0.5f);
        }
        lastDistanceToCheckpoint = currentDistance;

        // 2️⃣ RECOMPENSA POR ALINEACIÓN + VELOCIDAD
        float alignment = Vector3.Dot(obj.transform.forward, dirToCheckpoint);

        if (alignment > 0.5f) // Si va relativamente bien orientado
        {
            // Recompensa por velocidad hacia el checkpoint
            float projectedSpeed = Vector3.Dot(
                _prometeoCarController.carRigidbody.linearVelocity,
                dirToCheckpoint
            );

            // Escalado no lineal: premia más velocidades medias-altas
            float speedReward = Mathf.Pow(Mathf.Clamp01(projectedSpeed / 20f), 1.5f) * speedRewardScale;
            AddReward(speedReward);
        }
        else if (alignment < -0.3f)
        {
            // Penalización si va marcha atrás o muy mal orientado
            AddReward(alignment * 0.01f);
        }

        // 3️⃣ GESTIÓN DE DESLIZAMIENTO LATERAL (más permisivo)
        float slip = Mathf.Abs(_prometeoCarController.localVelocityX);
        float speed = _prometeoCarController.carSpeed;

        // Permitir más deslizamiento en curvas rápidas
        float slipTolerance = Mathf.Lerp(3f, 8f, speed / _prometeoCarController.maxSpeed);

        if (slip > slipTolerance)
        {
            AddReward(-(slip - slipTolerance) * 0.002f); // Penalización suave
        }

        // 4️⃣ DETECCIÓN Y GESTIÓN DE SUBIDAS/BAJADAS
        float uphill = Vector3.Dot(obj.transform.forward, Vector3.up);

        if (uphill > 0.15f) // Subida significativa (Eau Rouge)
        {
            // Recompensa por mantener acelerador en subidas
            if (throttle > 0.6f && speed > 0.2f * _prometeoCarController.maxSpeed)
            {
                AddReward(0.005f);
            }

            // Penalización reducida por pérdida de velocidad en subida
            if (speed < 0.15f * _prometeoCarController.maxSpeed)
            {
                AddReward(-0.001f);
            }
        }

        // 5️⃣ RECOMPENSA POR MIRAR HACIA ADELANTE (anticipación)
        int currentIndex = _checkpointManager.GetCheckpointIndex();
        var checkpoints = _checkpointManager.checkpp.checkPoints;

        if (currentIndex + 1 < checkpoints.Count)
        {
            Vector3 dirToNextNext = (checkpoints[currentIndex + 1].transform.position - carPos).normalized;
            float anticipationAlignment = Vector3.Dot(obj.transform.forward, dirToNextNext);

            // Pequeña recompensa por tener buena línea de carrera
            if (anticipationAlignment > 0.7f)
            {
                AddReward(0.001f);
            }
        }

        // 6️⃣ PENALIZACIÓN POR TIEMPO (MUY REDUCIDA)
        AddReward(-timePenaltyScale);

        // 7️⃣ DETECCIÓN DE ESTANCAMIENTO
        float movementThisFrame = Vector3.Distance(carPos, lastPosition);
        if (movementThisFrame < 0.01f && speed < 0.5f)
        {
            AddReward(-0.001f); // Penalización por estar parado
        }
        lastPosition = carPos;

        // Tracking para debug
        totalRewardThisEpisode += GetCumulativeReward();

        // Debug info
        if (showDebugInfo && StepCount % 500 == 0)
        {
            Debug.Log($"[Step {StepCount}] Checkpoints: {checkpointsReachedThisEpisode}/31 | " +
                      $"Speed: {speed:F1} | Reward: {GetCumulativeReward():F2}");
        }

        // Fin por límite de pasos
        if (StepCount >= MaxStep)
        {
            if (showDebugInfo)
            {
                Debug.Log($"⏱️ Episode Timeout | Checkpoints: {checkpointsReachedThisEpisode}/31 | " +
                          $"Total Reward: {GetCumulativeReward():F2}");
            }
            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (_checkpointManager.nextCheckPointToReach == null) return;

        Vector3 carPos = obj.transform.position;
        Vector3 carForward = obj.transform.forward;
        Vector3 carUp = obj.transform.up;

        // ═══════════════════════════════════════════════════════
        // 🔍 OBSERVACIONES MEJORADAS
        // ═══════════════════════════════════════════════════════

        // 1️⃣ Dirección normalizada al checkpoint actual (3)
        Vector3 dirToCheckpoint = (_checkpointManager.nextCheckPointToReach.transform.position - carPos).normalized;
        Vector3 localDir = obj.transform.InverseTransformDirection(dirToCheckpoint);
        sensor.AddObservation(localDir);

        // 2️⃣ Dirección al siguiente checkpoint (anticipación) (3)
        int currentIndex = _checkpointManager.GetCheckpointIndex();
        var checkpoints = _checkpointManager.checkpp.checkPoints;

        if (currentIndex + 1 < checkpoints.Count)
        {
            Vector3 dirToNextNext = (checkpoints[currentIndex + 1].transform.position - carPos).normalized;
            Vector3 localDirNext = obj.transform.InverseTransformDirection(dirToNextNext);
            sensor.AddObservation(localDirNext);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
        }

        // 3️⃣ Velocidad normalizada (1)
        float normalizedSpeed = _prometeoCarController.carSpeed / _prometeoCarController.maxSpeed;
        sensor.AddObservation(normalizedSpeed);

        // 4️⃣ Velocidad lateral normalizada (deslizamiento) (1)
        float normalizedSlip = _prometeoCarController.localVelocityX / 10f;
        sensor.AddObservation(Mathf.Clamp(normalizedSlip, -1f, 1f));

        // 5️⃣ Velocidad angular normalizada (qué tan rápido gira) (3)
        Vector3 angularVel = _prometeoCarController.carRigidbody.angularVelocity;
        sensor.AddObservation(angularVel / 5f); // Normalizado

        // 6️⃣ Pitch y roll del coche (importante para Eau Rouge) (2)
        float pitch = Vector3.SignedAngle(Vector3.forward,
            Vector3.ProjectOnPlane(carForward, Vector3.right),
            Vector3.right) / 90f;
        float roll = Vector3.SignedAngle(Vector3.up,
            Vector3.ProjectOnPlane(carUp, carForward),
            carForward) / 90f;
        sensor.AddObservation(pitch);
        sensor.AddObservation(roll);

        // 7️⃣ Distancia normalizada al checkpoint (1)
        float distToCheckpoint = Vector3.Distance(carPos, _checkpointManager.nextCheckPointToReach.transform.position);
        sensor.AddObservation(Mathf.Clamp01(distToCheckpoint / 50f));

        // 8️⃣ Raycasts mejorados (15)
        float angleStep = 180f / (numRays - 1);
        for (int i = 0; i < numRays; i++)
        {
            float rayAngle = -90f + i * angleStep;
            Vector3 dir = Quaternion.Euler(0, rayAngle, 0) * carForward;

            if (Physics.Raycast(
                carPos + Vector3.up * 0.5f,
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

        // TOTAL OBSERVACIONES: 3 + 3 + 1 + 1 + 3 + 2 + 1 + 15 = 29
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
        // ¡MEGA RECOMPENSA por completar la vuelta!
        AddReward(50f);

        if (showDebugInfo)
        {
            Debug.Log($"🏁 ¡META COMPLETADA! Total Reward: {GetCumulativeReward():F2} | Steps: {StepCount}");
        }

        EndEpisode();
    }

    public void ResetCar()
    {
        obj.transform.position = area.transform.position;
        obj.transform.rotation = Quaternion.Euler(0, -90, 0);

        _checkpointManager.ResetCheckpoints();
        _prometeoCarController.carSpeed = 0;
        _prometeoCarController.ResetCarState();

        salidaDePista = false;
    }

    public void HandleOffTrack()
    {
        AddReward(offTrackPenalty);

        if (showDebugInfo)
        {
            Debug.Log($"❌ Off Track | Checkpoints: {checkpointsReachedThisEpisode}/31 | " +
                      $"Reward: {GetCumulativeReward():F2}");
        }

        EndEpisode();
    }

    private void OnCheckpointReached(CheckPoint2 checkpoint)
    {
        checkpointsReachedThisEpisode++;

        // Recompensa base por checkpoint
        float reward = checkpointReward;

        // Bonus por velocidad al pasar checkpoint
        float speedBonus = (_prometeoCarController.carSpeed / _prometeoCarController.maxSpeed) * 2f;
        reward += speedBonus;

        // Bonus progresivo (cada vez más importante llegar lejos)
        float progressBonus = (checkpointsReachedThisEpisode / 31f) * 5f;
        reward += progressBonus;

        AddReward(reward);

        if (showDebugInfo)
        {
            Debug.Log($"✅ Checkpoint {checkpointsReachedThisEpisode}/31 | " +
                      $"Reward: +{reward:F2} | Total: {GetCumulativeReward():F2}");
        }

        // Reset distancia para el siguiente checkpoint
        if (_checkpointManager.nextCheckPointToReach != null)
        {
            lastDistanceToCheckpoint = Vector3.Distance(
                obj.transform.position,
                _checkpointManager.nextCheckPointToReach.transform.position
            );
        }
    }
}
