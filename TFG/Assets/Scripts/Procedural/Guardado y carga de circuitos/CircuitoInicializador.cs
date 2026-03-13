using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centraliza toda la inicialización que debe ocurrir DESPUÉS de que el circuito
/// esté colocado en escena, tanto si viene de generación como de carga de archivo.
///
/// Escucha:
///   - CircuitoEventos.OnCircuitoCerrado  → circuito recién generado
///   - CircuitoEventos.OnCircuitoCargado  → circuito cargado desde JSON
///
/// En ambos casos llama a InicializarCircuito(piezas, distancia), que:
///   1. Localiza o instancia el coche (SpawnPoint).
///   2. Reconstruye la lista de CheckPoints del manager a partir de las piezas.
///   3. Reinicia el CheckPointsManager.
///   4. Reinicia el agente ML.
/// </summary>
public class CircuitoInicializador : MonoBehaviour
{
    [Header("Prefab del coche / agente")]
    [Tooltip("Prefab que contiene el coche + Agente1_0. Se instancia en el SpawnPoint del circuito.")]
    public GameObject cochePrefab;

    [Header("Debug")]
    public bool mostrarLogs = true;

    // Referencia a la instancia actual del coche (se destruye y recrea en cada circuito)
    private GameObject cocheInstancia;

    // ── Unity lifecycle ───────────────────────────────────────────────────

    private void OnEnable()
    {
        CircuitoEventos.OnCircuitoCerrado += HandleCircuitoListo;
        CircuitoEventos.OnCircuitoCargado += HandleCircuitoListo;
    }

    private void OnDisable()
    {
        CircuitoEventos.OnCircuitoCerrado -= HandleCircuitoListo;
        CircuitoEventos.OnCircuitoCargado -= HandleCircuitoListo;
    }

    // ── Handler unificado ─────────────────────────────────────────────────

    private void HandleCircuitoListo(List<PiezaCircuito> piezas, float distanciaTotal)
    {
        if (mostrarLogs)
            Debug.Log($"[CircuitoInicializador] Circuito listo. Piezas: {piezas.Count}, " +
                      $"Distancia: {distanciaTotal:F1}m. Iniciando setup...");

        StartCoroutine(InicializarCircuito(piezas, distanciaTotal));
    }

    // ── Inicialización principal ──────────────────────────────────────────

    /// <summary>
    /// Punto de entrada único para toda la inicialización post-circuito.
    /// Añade aquí cualquier paso nuevo que necesites en el futuro.
    /// </summary>
    private IEnumerator InicializarCircuito(List<PiezaCircuito> piezas, float distanciaTotal)
    {
        // Esperamos un frame para que todos los Awake/Start de las piezas hayan corrido
        yield return null;

        // ── Paso 1: SpawnPoint ────────────────────────────────────────────
        Transform spawnPoint = BuscarSpawnPoint(piezas);

        if (spawnPoint == null)
        {
            Debug.LogError("[CircuitoInicializador] No se encontró ningún SpawnPoint en el circuito. " +
                           "Asegúrate de que la pieza de inicio tiene un hijo con Tag 'SpawnPoint'.");
            yield break;
        }

        if (mostrarLogs)
            Debug.Log($"[CircuitoInicializador] SpawnPoint encontrado: {spawnPoint.position}");

        // ── Paso 2: Destruir coche anterior e instanciar uno nuevo ────────
        if (cocheInstancia != null)
        {
            Destroy(cocheInstancia);
            yield return null; // Un frame para que OnDestroy del agente limpie el singleton
        }

        Quaternion rotacionFinal = spawnPoint.rotation * Quaternion.Euler(0f, -90f, 0f);

        if (cochePrefab != null)
        {
            cocheInstancia = Instantiate(cochePrefab, spawnPoint.position, rotacionFinal);
            if (mostrarLogs)
                Debug.Log("[CircuitoInicializador] Coche instanciado.");
        }
        else
        {
            // Si el coche ya está en escena (no es un prefab dinámico), reubicarlo
            if (Agente1_0.Instance != null)
            {
                Agente1_0.Instance.transform.SetPositionAndRotation(spawnPoint.position, rotacionFinal);
                if (mostrarLogs)
                    Debug.Log("[CircuitoInicializador] Coche reubicado en SpawnPoint.");
            }
            else
            {
                Debug.LogWarning("[CircuitoInicializador] cochePrefab no asignado y no hay Agente1_0 en escena.");
            }
        }

        // ── Paso 3: Reconstruir CheckPoints en el manager ─────────────────
        yield return null; // Dejar que el Agente1_0 recién instanciado ejecute su Awake/Start

        // Pasar el spawnPoint al agente directamente, ya que él mismo no puede buscarlo
        // de forma fiable (problemas de orden de eventos y referencias destruidas)
        if (Agente1_0.Instance != null)
            Agente1_0.Instance.EstablecerSpawnPoint(spawnPoint);
        else
            Debug.LogWarning("[CircuitoInicializador] Agente1_0.Instance es null al intentar pasar el SpawnPoint.");

        ReconfigurarCheckpointManager(piezas);

        // ── Paso 4: Forzar reinicio del episodio del agente ───────────────
        ReiniciarAgente();

        if (mostrarLogs)
            Debug.Log("[CircuitoInicializador] ✅ Inicialización completa.");
    }

    // ── Pasos individuales ────────────────────────────────────────────────

    /// <summary>
    /// Busca el Transform "P1" dentro del hijo "Posiciones de salida" de la pieza de inicio.
    /// Estructura esperada: PiezaInicio → "Posiciones de salida" → "P1"
    /// </summary>
    private Transform BuscarSpawnPoint(List<PiezaCircuito> piezas)
    {
        // Preferencia: pieza marcada como Inicio
        foreach (var pieza in piezas)
        {
            if (pieza.tipo == PiezaCircuito.TipoPieza.Inicio)
            {
                Transform sp = BuscarP1EnPieza(pieza.transform);
                if (sp != null) return sp;
            }
        }

        // Fallback: cualquier pieza que tenga la estructura "Posiciones de salida/P1"
        foreach (var pieza in piezas)
        {
            Transform sp = BuscarP1EnPieza(pieza.transform);
            if (sp != null) return sp;
        }

        Debug.LogWarning("[CircuitoInicializador] No se encontró 'Posiciones de salida/P1' en ninguna pieza.");
        return null;
    }

    /// <summary>
    /// Navega por nombre: pieza → hijo "Posiciones de salida" → hijo "P1"
    /// </summary>
    private Transform BuscarP1EnPieza(Transform pieza)
    {
        Transform posicionesDeSalida = pieza.Find("Posiciones de salida");
        if (posicionesDeSalida == null) return null;

        Transform p1 = posicionesDeSalida.Find("P1");
        return p1;
    }

    /// <summary>
    /// Recolecta todos los CheckPoint1_0 presentes en las piezas instanciadas
    /// y los inyecta en el CheckPointsManager1_0 para que funcione correctamente
    /// tanto con circuitos generados como cargados.
    /// </summary>
    private void ReconfigurarCheckpointManager(List<PiezaCircuito> piezas)
    {
        CheckPointsManager1_0 manager = CheckPointsManager1_0.Instance;

        if (manager == null)
        {
            Debug.LogWarning("[CircuitoInicializador] CheckPointsManager1_0 no encontrado en escena. " +
                             "¿Está incluido en el prefab del circuito o en la escena base?");
            return;
        }

        // Recolectar todos los checkpoints de las piezas en orden
        List<CheckPoint1_0> checkpointsEncontrados = new List<CheckPoint1_0>();

        foreach (var pieza in piezas)
        {
            CheckPoint1_0[] cps = pieza.GetComponentsInChildren<CheckPoint1_0>();
            checkpointsEncontrados.AddRange(cps);
        }

        if (checkpointsEncontrados.Count == 0)
        {
            Debug.LogWarning("[CircuitoInicializador] No se encontraron CheckPoint1_0 en las piezas del circuito.");
            return;
        }

        // Inyectar en el CheckPoints1_0 (contenedor de lista) que usa el manager
        if (manager.checkpp != null)
        {
            manager.checkpp.checkPoints = checkpointsEncontrados;

            // Asignar IDs correlativos
            for (int i = 0; i < checkpointsEncontrados.Count; i++)
                checkpointsEncontrados[i].checkpointID = i;

            if (mostrarLogs)
                Debug.Log($"[CircuitoInicializador] {checkpointsEncontrados.Count} checkpoints inyectados en el manager.");
        }
        else
        {
            Debug.LogWarning("[CircuitoInicializador] manager.checkpp es null. " +
                             "Asegúrate de que CheckPointsManager1_0 tiene CheckPoints1_0 asignado en el Inspector.");
        }

        // Reiniciar el manager con los nuevos checkpoints
        manager.ResetCheckpoints();

        if (mostrarLogs)
            Debug.Log("[CircuitoInicializador] CheckPointsManager reiniciado.");
    }

    /// <summary>
    /// Reinicia el episodio del agente ML si está disponible.
    /// </summary>
    private void ReiniciarAgente()
    {
        Agente1_0 agente = Agente1_0.Instance;

        if (agente == null)
        {
            Debug.LogWarning("[CircuitoInicializador] Agente1_0 no encontrado todavía. " +
                             "Si es un prefab dinámico, el agente se inicializará solo en su Start().");
            return;
        }

        agente.ResetCar();

        if (mostrarLogs)
            Debug.Log("[CircuitoInicializador] Agente reiniciado.");
    }
}