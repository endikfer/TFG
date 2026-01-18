using UnityEngine;

public class CheckPoint3 : MonoBehaviour
{
    [SerializeField] private CheckPointsManager2 manager;

    private bool atravesado = false; // 🔹 Evita que se llame más de una vez

    public int checkpointID;

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!atravesado && manager != null)
    //    {
    //        atravesado = true; // Marca como ya usado
    //        manager.CheckPointReached(this);
    //        Debug.Log($"Alcanzado el checkpoint {checkpointID}.");
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Agente")) return; // Solo el coche
        if (!gameObject.activeSelf) return;     // Solo si el collider está activo

        // Llama al manager
        manager.CheckPointReached(this);
        Debug.Log($"Alcanzado el checkpoint {checkpointID}.");
    }

    // 🔹 Si quieres que se resetee cada episodio:
    public void ResetTrigger()
    {
        atravesado = false;
    }
}
