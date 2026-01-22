using UnityEngine;

public class CocheContacto5 : MonoBehaviour
{
    [SerializeField] private Agente4 karAgent;

    private void OnCollisionEnter(Collision col)
    {
        karAgent.salidaDePista = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        karAgent.salidaDePista = false;
    }
}
