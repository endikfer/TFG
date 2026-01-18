using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointsManagerM : MonoBehaviour
{
    [SerializeField] public CheckPointsM checkpp;

    // Guarda el índice del siguiente checkpoint para cada agente
    private Dictionary<AgenteM, int> agenteCheckpointIndex = new Dictionary<AgenteM, int>();

    // Evento opcional por agente
    public event Action<CheckPointM, AgenteM> reachedCheckpoint;

    private void Awake()
    {
        // Asignar IDs en orden
        for (int i = 0; i < checkpp.checkPoints.Count; i++)
        {
            checkpp.checkPoints[i].checkpointID = i;
        }
    }

    // Registrar un agente al inicio de la escena
    public void RegisterAgent(AgenteM agente)
    {
        if (!agenteCheckpointIndex.ContainsKey(agente))
            agenteCheckpointIndex.Add(agente, 0);
    }

    // Reinicia todos los checkpoints para un agente
    public void ResetCheckpointsForAgent(AgenteM agente)
    {
        agenteCheckpointIndex[agente] = 0;

        foreach (var cp in checkpp.checkPoints)
        {
            cp.ResetTriggerForAgent(agente);
        }
    }

    // Llamado por cada checkpoint al atravesarlo
    public void CheckPointReached(CheckPointM checkpoint, AgenteM agente)
    {
        if (!agenteCheckpointIndex.ContainsKey(agente)) return;

        int currentIndex = agenteCheckpointIndex[agente];

        // Solo aceptamos el checkpoint correcto
        if (checkpoint.checkpointID != currentIndex) return;

        bool isGoal = (currentIndex == checkpp.checkPoints.Count - 1);

        //reachedCheckpoint?.Invoke(agente, isGoal);

        agenteCheckpointIndex[agente]++;

        // Si completa la vuelta → reinicio lógico
        if (isGoal)
        {
            agenteCheckpointIndex[agente] = 0;

            foreach (var cp in checkpp.checkPoints)
                cp.ResetTriggerForAgent(agente);
        }
    }

    // Devuelve el siguiente checkpoint que debe atravesar un agente
    public CheckPointM GetNextCheckpoint(AgenteM agente)
    {
        if (!agenteCheckpointIndex.ContainsKey(agente)) return null;

        int index = agenteCheckpointIndex[agente];
        if (index < checkpp.checkPoints.Count)
            return checkpp.checkPoints[index];

        return null;
    }
}

