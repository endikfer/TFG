using UnityEngine;

public class CocheContacto1_0 : MonoBehaviour
{
    private Agente1_0 karAgent;

    private void Start()
    {
        // Suscribirse al evento y NO desuscribirse nunca: así cada vez que
        // se instancia un coche nuevo, CocheContacto1_0 actualiza su referencia.
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
        karAgent = Agente1_0.Instance;
    }

    private void OnCollisionEnter(Collision col)
    {
        if (karAgent != null)
            karAgent.salidaDePista = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (karAgent != null)
            karAgent.salidaDePista = false;
    }
}