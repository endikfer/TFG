using System.Collections.Generic;
using UnityEngine;

public class CheckPointM : MonoBehaviour
{
    [SerializeField] private CheckPointsManagerM manager;

    public int checkpointID;

    // Guarda qué agentes ya han atravesado este checkpoint
    private HashSet<AgenteM> agentesQueLoHanAtravesado = new HashSet<AgenteM>();

    private void OnTriggerEnter(Collider other)
    {
        AgenteM agente = other.GetComponent<AgenteM>();
        if (agente == null) return; // solo coches

        if (agentesQueLoHanAtravesado.Contains(agente)) return;

        agentesQueLoHanAtravesado.Add(agente);
        manager.CheckPointReached(this, agente);
    }

    // Resetea este checkpoint solo para un agente específico
    public void ResetTriggerForAgent(AgenteM agente)
    {
        agentesQueLoHanAtravesado.Remove(agente);
    }
}
