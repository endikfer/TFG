//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class CheckPointsManager2 : MonoBehaviour
//{
//    [SerializeField] private Agente3 kartAgent;

//    // Checkpoint actual
//    public CheckPoint3 nextCheckPointToReach { get; private set; }

//    // Distancia al siguiente checkpoint
//    public float distance { get; private set; }
//    public float distanceMaxToNext { get; private set; }

//    private int CurrentCheckpointIndex;
//    private List<CheckPoint3> Checkpoints;

//    [SerializeField] public CheckPoints3 checkpp;

//    private CheckPoint3 lastCheckpoint;

//    // Evento opcional, si quieres notificar al agente al llegar a un checkpoint
//    public event Action<CheckPoint3> reachedCheckpoint;

//    private void Awake()
//    {
//        // Asigna IDs a todos los checkpoints (solo para debug o referencia)
//        for (int i = 0; i < checkpp.checkPoints.Count; i++)
//        {
//            checkpp.checkPoints[i].checkpointID = i;
//        }
//    }

//    private void Start()
//    {
//        Checkpoints = checkpp.checkPoints;
//        ResetCheckpoints();

//        // Distancia inicial al primer checkpoint
//        distanceMaxToNext = Vector3.Distance(kartAgent.transform.position, nextCheckPointToReach.transform.position);
//    }

//    public void ResetCheckpoints()
//    {
//        CurrentCheckpointIndex = 0;
//        Checkpoints = checkpp.checkPoints;
//        SetNextCheckpoint();
//    }

//    private void Update()
//    {
//        // Actualiza la distancia al siguiente checkpoint
//        if (nextCheckPointToReach == null) return;

//        distance = Vector3.Distance(kartAgent.transform.position, nextCheckPointToReach.transform.position);

//        // Debug opcional
//        Debug.Log("Indice de checkpoint actual: " + CurrentCheckpointIndex);
//    }

//    // Llamado por cada checkpoint cuando el coche lo atraviesa
//    public void CheckPointReached(CheckPoint3 checkpoint)
//    {
//        if (nextCheckPointToReach != checkpoint) return;

//        lastCheckpoint = Checkpoints[CurrentCheckpointIndex];

//        reachedCheckpoint?.Invoke(checkpoint); // Notificación opcional

//        CurrentCheckpointIndex++;

//        // Actualiza el siguiente checkpoint, si queda alguno
//        if (CurrentCheckpointIndex < Checkpoints.Count)
//        {
//            SetNextCheckpoint();

//            distanceMaxToNext = Vector3.Distance(kartAgent.transform.position, nextCheckPointToReach.transform.position);
//        }
//        else
//        {
//            // Si el circuito está completo, nextCheckPointToReach = null
//            nextCheckPointToReach = null;
//        }
//    }

//    private void SetNextCheckpoint()
//    {
//        if (Checkpoints.Count > 0 && CurrentCheckpointIndex < Checkpoints.Count)
//        {
//            nextCheckPointToReach = Checkpoints[CurrentCheckpointIndex];
//        }
//    }

//    // Para que el agente consulte el índice actual si quiere
//    public int GetCheckpointIndex()
//    {
//        return CurrentCheckpointIndex;
//    }
//}

using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointsManager2 : MonoBehaviour
{
    [SerializeField] public CheckPoints3 checkpp;
    public event Action<CheckPoint3> reachedCheckpoint;

    public bool loopCircuit = true;

    private int CurrentCheckpointIndex;

    private void Start()
    {
        ResetCheckpoints();
    }

    public void ResetCheckpoints()
    {
        CurrentCheckpointIndex = 0;

        // Desactivar todos los checkpoints
        foreach (var cp in checkpp.checkPoints)
        {
            cp.gameObject.SetActive(false);
        }

        // Activar el primer checkpoint
        if (checkpp.checkPoints.Count > 0)
        {
            checkpp.checkPoints[0].gameObject.SetActive(true);
        }
    }

    public void CheckPointReached(CheckPoint3 checkpoint)
    {
        // Desactiva el checkpoint actual
        checkpoint.gameObject.SetActive(false);

        // ⚡ Llama al evento para notificar al agente
        reachedCheckpoint?.Invoke(checkpoint);

        CurrentCheckpointIndex++;

        // Activar el siguiente checkpoint
        if (CurrentCheckpointIndex < checkpp.checkPoints.Count)
        {
            checkpp.checkPoints[CurrentCheckpointIndex].gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("¡Circuito completado!");

            //if (loopCircuit)
            //{
            //    // Reinicia para siguiente vuelta
            //    ResetCheckpoints();
            //}
        }
    }

    public CheckPoint3 GetActiveCheckpoint()
    {
        if (CurrentCheckpointIndex < checkpp.checkPoints.Count)
            return checkpp.checkPoints[CurrentCheckpointIndex].gameObject.activeSelf
                ? checkpp.checkPoints[CurrentCheckpointIndex]
                : null;
        return null;
    }
}

