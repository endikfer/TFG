using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointsManager1_0 : MonoBehaviour
{
    // ─── SINGLETON ───
    public static CheckPointsManager1_0 Instance { get; private set; }

    private Agente1_0 kartAgent;

    public CheckPoint1_0 nextCheckPointToReach { get; private set; }
    public float distance { get; private set; }
    public float distanceMaxToNext { get; private set; }

    private int CurrentCheckpointIndex;
    private List<CheckPoint1_0> Checkpoints = new List<CheckPoint1_0>();

    // checkpp sigue existiendo para que CheckPoint1_0 y Agente1_0 accedan a la lista
    public CheckPoints1_0 checkpp;

    private CheckPoint1_0 lastCheckpoint;
    public event Action<CheckPoint1_0> reachedCheckpoint;

    private bool _circuitoListo = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Suscribirse al evento del generador
        CircuitoEventos.OnCircuitoListoParaAgente += OnCircuitoListo;

        // Suscribirse al agente cuando llegue
        if (Agente1_0.Instance != null)
            OnAgentReady();
        else
            Agente1_0.OnAgentReady += OnAgentReady;
    }

    private void OnDestroy()
    {
        CircuitoEventos.OnCircuitoListoParaAgente -= OnCircuitoListo;
        Agente1_0.OnAgentReady -= OnAgentReady;

        if (Instance == this)
            Instance = null;
    }

    // ─── Llamado cuando el generador termina el circuito ───
    private void OnCircuitoListo(List<PiezaCircuito> piezasOrdenadas)
    {
        // Recorrer las piezas EN ORDEN y recoger sus checkpoints
        List<CheckPoint1_0> checkpointsOrdenados = new List<CheckPoint1_0>();

        foreach (PiezaCircuito pieza in piezasOrdenadas)
        {
            // GetComponentsInChildren respeta el orden de jerarquía del prefab
            CheckPoint1_0[] checkpointsDePieza = pieza.GetComponentsInChildren<CheckPoint1_0>();
            checkpointsOrdenados.AddRange(checkpointsDePieza);
        }

        // Asignar IDs en orden
        for (int i = 0; i < checkpointsOrdenados.Count; i++)
            checkpointsOrdenados[i].checkpointID = i;

        // Guardar en checkpp para que el resto del sistema lo use igual que antes
        checkpp.checkPoints = checkpointsOrdenados;

        _circuitoListo = true;

        Debug.Log($"[CheckPointsManager] Circuito listo: {checkpointsOrdenados.Count} checkpoints registrados.");

        // Inicializar la carrera
        Checkpoints = checkpp.checkPoints;
        ResetCheckpoints();

        // Si el agente ya estaba listo, calcular distancia inicial
        if (kartAgent != null)
            distanceMaxToNext = Vector3.Distance(kartAgent.transform.position, nextCheckPointToReach.transform.position);
    }

    private void OnAgentReady()
    {
        kartAgent = Agente1_0.Instance;
        Agente1_0.OnAgentReady -= OnAgentReady;

        if (_circuitoListo && nextCheckPointToReach != null)
            distanceMaxToNext = Vector3.Distance(kartAgent.transform.position, nextCheckPointToReach.transform.position);
    }

    private void Update()
    {
        if (!_circuitoListo || kartAgent == null || nextCheckPointToReach == null) return;

        distance = Vector3.Distance(kartAgent.transform.position, nextCheckPointToReach.transform.position);
    }

    public void ResetCheckpoints()
    {
        CurrentCheckpointIndex = 0;
        Checkpoints = checkpp.checkPoints;
        SetNextCheckpoint();
    }

    public void CheckPointReached(CheckPoint1_0 checkpoint)
    {
        if (nextCheckPointToReach != checkpoint) return;

        lastCheckpoint = Checkpoints[CurrentCheckpointIndex];
        reachedCheckpoint?.Invoke(checkpoint);
        CurrentCheckpointIndex++;

        if (CurrentCheckpointIndex < Checkpoints.Count)
        {
            SetNextCheckpoint();

            if (kartAgent != null)
                distanceMaxToNext = Vector3.Distance(kartAgent.transform.position, nextCheckPointToReach.transform.position);
        }
        else
        {
            nextCheckPointToReach = null;
        }
    }

    private void SetNextCheckpoint()
    {
        if (Checkpoints.Count > 0 && CurrentCheckpointIndex < Checkpoints.Count)
            nextCheckPointToReach = Checkpoints[CurrentCheckpointIndex];
    }

    public int GetCheckpointIndex() => CurrentCheckpointIndex;
}