using UnityEngine;

public class Meta3 : MonoBehaviour
{
    [SerializeField] private Agente3 kartAgent;

    private void OnTriggerEnter(Collider other)
    {
        kartAgent.ScoredAGoal();
    }
}
