using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointsManager2 : MonoBehaviour
{
    [SerializeField] private Agente3 kartAgent;

    // Checkpoint actual
    public CheckPoint3 nextCheckPointToReach { get; private set; }

    // Distancia al siguiente checkpoint
    public float distance { get; private set; }
    public float distanceMaxToNext { get; private set; }

    private int CurrentCheckpointIndex;
    private List<CheckPoint3> Checkpoints;

    [SerializeField] public CheckPoints3 checkpp;

    private CheckPoint3 lastCheckpoint;

    // Evento opcional, si quieres notificar al agente al llegar a un checkpoint
    public event Action<CheckPoint3> reachedCheckpoint;

    private void Awake()
    {
        // Asigna IDs a todos los checkpoints (solo para debug o referencia)
        for (int i = 0; i < checkpp.checkPoints.Count; i++)
        {
            checkpp.checkPoints[i].checkpointID = i;
        }
    }

    private void Start()
    {
        Checkpoints = checkpp.checkPoints;
        ResetCheckpoints();

        // Distancia inicial al primer checkpoint
        distanceMaxToNext = Vector3.Distance(kartAgent.transform.position, nextCheckPointToReach.transform.position);
    }

    public void ResetCheckpoints()
    {
        CurrentCheckpointIndex = 0;
        Checkpoints = checkpp.checkPoints;
        SetNextCheckpoint();
    }

    private void Update()
    {
        // Actualiza la distancia al siguiente checkpoint
        if (nextCheckPointToReach == null) return;

        distance = Vector3.Distance(kartAgent.transform.position, nextCheckPointToReach.transform.position);

        // Debug opcional
        Debug.Log("Indice de checkpoint actual: " + CurrentCheckpointIndex);
    }

    // Llamado por cada checkpoint cuando el coche lo atraviesa
    public void CheckPointReached(CheckPoint3 checkpoint)
    {
        if (nextCheckPointToReach != checkpoint) return;

        lastCheckpoint = Checkpoints[CurrentCheckpointIndex];

        reachedCheckpoint?.Invoke(checkpoint); // Notificación opcional

        CurrentCheckpointIndex++;

        // Actualiza el siguiente checkpoint, si queda alguno
        if (CurrentCheckpointIndex < Checkpoints.Count)
        {
            SetNextCheckpoint();

            distanceMaxToNext = Vector3.Distance(kartAgent.transform.position, nextCheckPointToReach.transform.position);
        }
        else
        {
            // Si el circuito está completo, nextCheckPointToReach = null
            nextCheckPointToReach = null;
        }
    }

    private void SetNextCheckpoint()
    {
        if (Checkpoints.Count > 0 && CurrentCheckpointIndex < Checkpoints.Count)
        {
            nextCheckPointToReach = Checkpoints[CurrentCheckpointIndex];
        }
    }

    // Para que el agente consulte el índice actual si quiere
    public int GetCheckpointIndex()
    {
        return CurrentCheckpointIndex;
    }
}
