using System;
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
///   1. Localiza el SpawnPoint del circuito.
///   2. Primera carga: instancia el coche una sola vez.
///      Cargas siguientes: solo reposiciona el coche existente (sin Destroy/Instantiate).
///   3. Reconstruye la lista de CheckPoints del manager a partir de las piezas.
///   4. Reinicia el agente ML (ResetCar).
///   5. Dispara OnTodoListo → RaceHUD muestra el panel.
/// </summary>
public class CircuitoInicializador : MonoBehaviour
{
    [Header("Prefab del coche / agente")]
    [Tooltip("Prefab que contiene el coche + Agente1_0. Se instancia UNA SOLA VEZ en la primera carga.")]
    public GameObject cochePrefab;

    [Header("Debug")]
    public bool mostrarLogs = true;

    // ── Evento estático ───────────────────────────────────────────────────
    /// <summary>
    /// Se dispara cuando circuito + coche + checkpoints están completamente listos.
    /// RaceHUD lo escucha para mostrar el panel de vueltas.
    /// </summary>
    public static event Action OnTodoListo;

    // Referencia al coche. Se asigna en la primera carga y nunca se destruye.
    private GameObject cocheInstancia;
    private bool cocheYaInstanciado = false;

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

        if (Agente1_0.Instance != null)
            Agente1_0.Instance.BloquearHastaCircuito();

        StartCoroutine(InicializarCircuito(piezas, distanciaTotal));
    }

    // ── Inicialización principal ──────────────────────────────────────────

    private IEnumerator InicializarCircuito(List<PiezaCircuito> piezas, float distanciaTotal)
    {
        // Esperamos un frame para que todos los Awake/Start de las piezas hayan corrido
        yield return null;

        // ── Paso 1: SpawnPoint ────────────────────────────────────────────
        Transform spawnPoint = BuscarSpawnPoint(piezas);

        if (spawnPoint == null)
        {
            Debug.LogError("[CircuitoInicializador] No se encontró ningún SpawnPoint en el circuito. " +
                           "Asegúrate de que la pieza de inicio tiene 'Posiciones de salida/P1'.");
            yield break;
        }

        if (mostrarLogs)
            Debug.Log($"[CircuitoInicializador] SpawnPoint encontrado: {spawnPoint.position}");

        // ── Paso 2: Primera carga → instanciar coche. Resto → nada aquí ──
        if (!cocheYaInstanciado)
        {
            if (cochePrefab != null)
            {
                // La rotación final la calcula EstablecerSpawnPoint internamente,
                // pero necesitamos una rotación inicial coherente para el Instantiate.
                Quaternion rotacionInicial = spawnPoint.rotation * Quaternion.Euler(0f, -90f, 0f);
                cocheInstancia = Instantiate(cochePrefab, spawnPoint.position, rotacionInicial);
                cocheYaInstanciado = true;

                if (mostrarLogs)
                    Debug.Log("[CircuitoInicializador] Coche instanciado por primera vez.");

                // Esperar a que Awake/Start del coche corran antes de continuar
                yield return null;
            }
            else
            {
                Debug.LogError("[CircuitoInicializador] cochePrefab no asignado. " +
                               "Asígnalo en el Inspector.");
                yield break;
            }
        }

        // ── Paso 3: Reconstruir CheckPoints en el manager ─────────────────
        ReconfigurarCheckpointManager(piezas);

        // ── Paso 4: Actualizar SpawnPoint en el agente y reposicionar ──────
        // Tanto en la primera carga como en las siguientes, simplemente
        // actualizamos las coordenadas de spawn y hacemos ResetCar.
        // Nunca se destruye ni se reinstancia el coche.
        if (Agente1_0.Instance != null)
        {
            Agente1_0.Instance.EstablecerSpawnPoint(spawnPoint);
            Agente1_0.Instance.NotificarCircuitoListo();
            Agente1_0.Instance.ResetCar();

            if (mostrarLogs)
                Debug.Log("[CircuitoInicializador] Coche reposicionado en nuevo SpawnPoint.");
        }
        else
        {
            Debug.LogWarning("[CircuitoInicializador] Agente1_0.Instance es null. " +
                             "El coche se inicializará solo en su Start().");
        }

        // ── Paso 5: Notificar que todo está listo ─────────────────────────
        OnTodoListo?.Invoke();

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

    private Transform BuscarP1EnPieza(Transform pieza)
    {
        Transform posicionesDeSalida = pieza.Find("Posiciones de salida");
        if (posicionesDeSalida == null) return null;
        return posicionesDeSalida.Find("P1");
    }

    /// <summary>
    /// Recolecta todos los CheckPoint1_0 presentes en las piezas instanciadas
    /// y los inyecta en el CheckPointsManager1_0.
    /// </summary>
    private void ReconfigurarCheckpointManager(List<PiezaCircuito> piezas)
    {
        CheckPointsManager1_0 manager = CheckPointsManager1_0.Instance;

        if (manager == null)
        {
            Debug.LogWarning("[CircuitoInicializador] CheckPointsManager1_0 no encontrado en escena.");
            return;
        }

        List<CheckPoint1_0> checkpointsEncontrados = new List<CheckPoint1_0>();

        foreach (var pieza in piezas)
        {
            CheckPoint1_0[] cps = pieza.GetComponentsInChildren<CheckPoint1_0>();
            checkpointsEncontrados.AddRange(cps);
        }

        if (checkpointsEncontrados.Count == 0)
        {
            Debug.LogWarning("[CircuitoInicializador] No se encontraron CheckPoint1_0 en las piezas.");
            return;
        }

        if (manager.checkpp != null)
        {
            manager.checkpp.checkPoints = checkpointsEncontrados;

            for (int i = 0; i < checkpointsEncontrados.Count; i++)
                checkpointsEncontrados[i].checkpointID = i;

            if (mostrarLogs)
                Debug.Log($"[CircuitoInicializador] {checkpointsEncontrados.Count} checkpoints inyectados.");
        }
        else
        {
            Debug.LogWarning("[CircuitoInicializador] manager.checkpp es null.");
        }

        manager.ResetCheckpoints();

        if (mostrarLogs)
            Debug.Log("[CircuitoInicializador] CheckPointsManager reiniciado.");
    }
}