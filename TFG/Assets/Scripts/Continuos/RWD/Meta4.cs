using UnityEngine;

public class Meta4 : MonoBehaviour
{
    [SerializeField] private Agente4 kartAgent;
    [SerializeField] private CheckPointsManager3 manager; // referencia al manager

    private void OnTriggerEnter(Collider other)
    {
        // Solo se dispara si se ha completado el último checkpoint
        if (manager.GetCheckpointIndex() == manager.checkpp.checkPoints.Count)
        {
            kartAgent.ScoredAGoal();
        }
    }
}
