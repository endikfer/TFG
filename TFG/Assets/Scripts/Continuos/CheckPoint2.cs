using UnityEngine;

public class CheckPoint2 : MonoBehaviour
{
    [SerializeField] private CheckPointsManager manager;

    private bool atravesado = false; // 🔹 Evita que se llame más de una vez

    public int checkpointID;

    private void OnTriggerEnter(Collider other)
    {
        if (!atravesado && manager != null)
        {
            atravesado = true; // Marca como ya usado
            manager.CheckPointReached(this);
            Debug.Log($"Alcanzado el checkpoint {checkpointID}.");
        }
    }

    // 🔹 Si quieres que se resetee cada episodio:
    public void ResetTrigger()
    {
        atravesado = false;
    }
}
