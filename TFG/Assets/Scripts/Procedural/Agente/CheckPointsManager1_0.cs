using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointsManager1_0 : MonoBehaviour
{
    // ─── SINGLETON: se registra solo al cargarse el circuito ───
    public static CheckPointsManager1_0 Instance { get; private set; }

    // El agente se busca dinámicamente cuando llega (NO desde el Inspector)
    private Agente1_0 kartAgent;

    public CheckPoint1_0 nextCheckPointToReach { get; private set; }
    public float distance { get; private set; }
    public float distanceMaxToNext { get; private set; }

    private int CurrentCheckpointIndex;
    private List<CheckPoint1_0> Checkpoints;

    // checkpp SÍ se puede asignar en el Inspector (es parte del circuito,
    // carga al mismo tiempo que este script)
    [SerializeField] public CheckPoints1_0 checkpp;

    private CheckPoint1_0 lastCheckpoint;
    public event Action<CheckPoint1_0> reachedCheckpoint;

    private void Awake()
    {
        // Registrarse como singleton en cuanto el circuito carga
        Instance = this;

        for (int i = 0; i < checkpp.checkPoints.Count; i++)
            checkpp.checkPoints[i].checkpointID = i;
    }

    private void Start()
    {
        Checkpoints = checkpp.checkPoints;
        ResetCheckpoints();

        // Si el agente ya existe (caso raro), enlazarlo directamente
        if (Agente1_0.Instance != null)
            OnAgentReady();
        else
            // Si el agente aún no ha cargado, esperar su evento
            Agente1_0.OnAgentReady += OnAgentReady;
    }

    private void OnDestroy()
    {
        Agente1_0.OnAgentReady -= OnAgentReady;

        if (Instance == this)
            Instance = null;
    }

    // Llamado cuando el Agente ya está en escena y listo
    private void OnAgentReady()
    {
        kartAgent = Agente1_0.Instance;
        Agente1_0.OnAgentReady -= OnAgentReady; // desuscribirse

        // Ahora sí podemos calcular la distancia inicial
        distanceMaxToNext = Vector3.Distance(kartAgent.transform.position, nextCheckPointToReach.transform.position);
    }

    private void Update()
    {
        if (kartAgent == null || nextCheckPointToReach == null) return;

        distance = Vector3.Distance(kartAgent.transform.position, nextCheckPointToReach.transform.position);
        Debug.Log("Indice de checkpoint actual: " + CurrentCheckpointIndex);
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
