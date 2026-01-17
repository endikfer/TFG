using UnityEngine;

public class CocheContacto3 : MonoBehaviour
{
    [SerializeField] private Agente2 karAgent;

    private void OnCollisionEnter(Collision col)
    {
        karAgent.salidaDePista = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        karAgent.salidaDePista = false;
    }
}
