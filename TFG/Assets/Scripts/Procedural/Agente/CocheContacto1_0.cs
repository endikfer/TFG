using UnityEngine;

public class CocheContacto1_0 : MonoBehaviour
{
    // Este script está en el coche, que carga después del circuito.
    // El agente está en el mismo GameObject o en su padre → búsqueda local, coste cero.
    private Agente1_0 karAgent;

    private void Awake()
    {
        // Primero intenta en el mismo GameObject y sus padres/hijos (coste mínimo)
        karAgent = GetComponentInParent<Agente1_0>();

        if (karAgent == null)
            karAgent = GetComponentInChildren<Agente1_0>();

        // Si por alguna razón no está en la jerarquía local, usar el singleton
        if (karAgent == null)
            karAgent = Agente1_0.Instance;

        if (karAgent == null)
            Debug.LogError("[CocheContacto5] No se encontró Agente4.");
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
