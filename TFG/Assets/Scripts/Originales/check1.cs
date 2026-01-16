using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class check1 : MonoBehaviour
{
    [SerializeField] private CheckPointManager manager;

    private bool atravesado = false; // 🔹 Evita que se llame más de una vez

    public int checkpointID;

    private void OnTriggerExit(Collider other)
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
