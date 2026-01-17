using UnityEngine;

public class Meta2 : MonoBehaviour
{
    [SerializeField] private Agente2 kartAgent;

    private void OnTriggerEnter(Collider other)
    {
        kartAgent.ScoredAGoal();
    }
}
