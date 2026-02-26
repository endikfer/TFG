using UnityEngine;

public class Meta1_0 : MonoBehaviour
{
    // Ambos son parte del circuito → singleton disponible en Start
    private Agente1_0 kartAgent;
    private CheckPointsManager1_0 manager;

    private void Start()
    {
        manager = CheckPointsManager1_0.Instance;

        // El agente puede no estar aún → esperamos su evento
        if (Agente1_0.Instance != null)
            kartAgent = Agente1_0.Instance;
        else
            Agente1_0.OnAgentReady += OnAgentReady;
    }

    private void OnDestroy()
    {
        Agente1_0.OnAgentReady -= OnAgentReady;
    }

    private void OnAgentReady()
    {
        kartAgent = Agente1_0.Instance;
        Agente1_0.OnAgentReady -= OnAgentReady;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (kartAgent == null || manager == null) return;

        if (manager.GetCheckpointIndex() == manager.checkpp.checkPoints.Count)
            kartAgent.ScoredAGoal();
    }
}
