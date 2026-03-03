using UnityEngine;

public class CocheContacto1_0 : MonoBehaviour
{
    private Agente1_0 karAgent;

    private void OnEnable()
    {
        Agente1_0.OnAgentReady += OnAgentReady;
    }

    private void OnDestroy()
    {
        Agente1_0.OnAgentReady -= OnAgentReady;
    }

    private void OnAgentReady()
    {
        karAgent = Agente1_0.Instance;
        Agente1_0.OnAgentReady -= OnAgentReady;
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
