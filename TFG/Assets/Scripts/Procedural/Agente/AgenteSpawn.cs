using System.Collections.Generic;
using UnityEngine;

public class AgenteSpawn : MonoBehaviour
{
    [SerializeField] private GameObject cochePrefab; // arrastra el prefab del coche

    private void OnEnable()
    {
        CircuitoEventos.OnCircuitoListoParaAgente += OnCircuitoListo;
    }

    private void OnDisable()
    {
        CircuitoEventos.OnCircuitoListoParaAgente -= OnCircuitoListo;
    }

    private void OnCircuitoListo(List<PiezaCircuito> piezas)
    {
        // Buscar el spawnPoint en la pieza inicial
        Transform posicionesSalida = piezas[0].transform.Find("Posiciones de salida");
        Transform spawnPoint = posicionesSalida?.Find("P1");

        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        //Instantiate(cochePrefab, pos, rot);

        // Desuscribirse, solo instanciamos una vez
        CircuitoEventos.OnCircuitoListoParaAgente -= OnCircuitoListo;
    }
}
