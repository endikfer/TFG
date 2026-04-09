using UnityEngine;

public class CheckPoint1_0 : MonoBehaviour
{
    // El manager es parte del circuito, carga antes → acceso directo por singleton
    private CheckPointsManager1_0 manager;

    private bool atravesado = false;
    public int checkpointID;

    private void Start()
    {
        // El circuito carga primero, el singleton ya existe en este punto
        manager = CheckPointsManager1_0.Instance;

        if (manager == null)
            Debug.LogError("[CheckPoint2] CheckPointsManager3.Instance es null. ¿Se cargó el circuito correctamente?");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (atravesado || manager == null) return;
        if (manager.nextCheckPointToReach != this) return;

        atravesado = true;
        manager.CheckPointReached(this);
        Debug.Log($"Alcanzado el checkpoint {checkpointID}.");
    }

    public void resettrigger()
    {
        atravesado = false;
    }
}
