using UnityEngine;

public class CocheContacto4 : MonoBehaviour
{
    [SerializeField] private Agente3 karAgent;

    private void OnCollisionEnter(Collision col)
    {
        karAgent.salidaDePista = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        karAgent.salidaDePista = false;
    }
}
