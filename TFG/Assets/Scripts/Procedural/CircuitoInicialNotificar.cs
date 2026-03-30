using System.Collections.Generic;
using UnityEngine;

public class CircuitoInicialNotificador : MonoBehaviour
{
    public Transform circuitoParent;
    public Transform spawnPointInicial;

    private void Start()
    {
        // Pasar SpawnPoint al agente directamente
        if (spawnPointInicial != null && Agente1_0.Instance != null)
            Agente1_0.Instance.EstablecerSpawnPoint(spawnPointInicial);

        // Recoger checkpoints directamente del circuito inicial
        CheckPointsManager1_0 manager = CheckPointsManager1_0.Instance;
        if (manager != null && manager.checkpp != null)
        {
            List<CheckPoint1_0> checkpoints = new List<CheckPoint1_0>(
                circuitoParent.GetComponentsInChildren<CheckPoint1_0>());

            for (int i = 0; i < checkpoints.Count; i++)
                checkpoints[i].checkpointID = i;

            manager.checkpp.checkPoints = checkpoints;
            manager.ResetCheckpoints();
        }

        // Recoger piezas y notificar
        List<PiezaCircuito> piezas = new List<PiezaCircuito>();
        float distanciaTotal = 0f;

        foreach (Transform hijo in circuitoParent)
        {
            PiezaCircuito pieza = hijo.GetComponent<PiezaCircuito>();
            if (pieza != null)
            {
                piezas.Add(pieza);
                distanciaTotal += pieza.longitud;
            }
        }

        CircuitoEventos.NotificarCircuitoCargado(piezas, distanciaTotal);
    }
}