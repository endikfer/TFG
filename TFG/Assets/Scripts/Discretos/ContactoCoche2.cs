using UnityEngine;

public class ContactoCoche2 : MonoBehaviour
{
    [SerializeField] private Agente karAgent;

    private void OnCollisionEnter(Collision col)
    {
        karAgent.salidaDePista = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        karAgent.salidaDePista = false;
    }
}
