using UnityEngine;

public class Meta1_0 : MonoBehaviour
{
    private Agente1_0 kartAgent;
    private CheckPointsManager1_0 manager;

    private void Start()
    {
        manager = CheckPointsManager1_0.Instance;

        // Suscribirse al evento y NO desuscribirse nunca: así cada vez que
        // CircuitoInicializador instancia un coche nuevo, Meta1_0 actualiza
        // su referencia automáticamente sin necesidad de reiniciar.
        Agente1_0.OnAgentReady += OnAgentReady;

        // Si el agente ya existe en este momento, actualizamos ya.
        if (Agente1_0.Instance != null)
            OnAgentReady();
    }

    private void OnDestroy()
    {
        Agente1_0.OnAgentReady -= OnAgentReady;
    }

    private void OnAgentReady()
    {
        kartAgent = Agente1_0.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (kartAgent == null || manager == null) return;

        // Verificar que se han pasado TODOS los checkpoints de esta vuelta
        if (manager.GetCheckpointIndex() != manager.checkpoints.Count) return;

        // Delegar en el RaceManager: él decide si es vuelta intermedia o fin de carrera
        if (RaceManager.Instance != null)
        {
            RaceManager.Instance.VueltaCompletada();
        }
        else
        {
            Debug.LogWarning("[Meta1_0] RaceManager no encontrado. Terminando episodio directamente.");
            kartAgent.ScoredAGoal();
        }
    }
}