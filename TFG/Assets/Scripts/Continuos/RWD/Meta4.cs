using UnityEngine;

public class Meta4 : MonoBehaviour
{
    [SerializeField] private Agente4 kartAgent;

    private void OnTriggerEnter(Collider other)
    {
        kartAgent.ScoredAGoal();
    }
}
