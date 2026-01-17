using UnityEngine;

public class Meta : MonoBehaviour
{
    [SerializeField] private Agente kartAgent;

    private void OnTriggerEnter(Collider other)
    {
        kartAgent.ScoredAGoal();
    }
}
