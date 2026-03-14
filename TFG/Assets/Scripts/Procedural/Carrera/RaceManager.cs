using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la carrera completa: número de vueltas a completar antes de que
/// el agente reciba la recompensa final y termine el episodio.
///
/// CONFIGURACIÓN (Inspector):
///   modoCarrera = PorVueltas   → el agente debe completar 'vueltasObjetivo' vueltas.
///   modoCarrera = PorDistancia → se calcula automáticamente cuántas vueltas
///                                son necesarias para cubrir 'distanciaObjetivo' metros.
///
/// FLUJO:
///   1. Circuito generado/cargado  → OnCircuitoListo  → guarda longitud, calcula vueltas.
///   2. Meta cruzada (1 vuelta)    → VueltaCompletada → si faltan vueltas, resetea
///                                   checkpoints y da recompensa parcial. Si no, llama
///                                   a Agente1_0.ScoredAGoal().
///   3. Nuevo episodio             → ResetCarrera (llamado desde Agente1_0.OnEpisodeBegin).
/// </summary>
public class RaceManager : MonoBehaviour
{
    // ─── SINGLETON ───────────────────────────────────────────────────────
    public static RaceManager Instance { get; private set; }

    // ─── CONFIGURACIÓN ───────────────────────────────────────────────────

    public enum ModoCarrera { PorVueltas, PorDistancia }

    [Header("Modo de carrera")]
    [Tooltip("PorVueltas: fija el número de vueltas directamente.\n" +
             "PorDistancia: calcula las vueltas necesarias para cubrir la distancia total.")]
    public ModoCarrera modoCarrera = ModoCarrera.PorVueltas;

    [Header("Parámetros PorVueltas")]
    [Tooltip("Número de vueltas que debe completar el agente para terminar la carrera.")]
    [Min(1)]
    public int vueltasObjetivo = 3;

    [Header("Parámetros PorDistancia")]
    [Tooltip("Distancia total de carrera en metros. Las vueltas se calculan " +
             "dividiendo este valor entre la longitud del circuito.")]
    [Min(1f)]
    public float distanciaObjetivoCarrera = 600f;

    [Header("Recompensas")]
    [Tooltip("Recompensa adicional que recibe el agente al completar cada vuelta intermedia.")]
    public float recompensaPorVuelta = 10f;

    [Header("UI")]
    [Tooltip("Referencia al componente RaceHUD que muestra el contador en pantalla.")]
    public RaceHUD raceHUD;

    [Header("Debug")]
    public bool mostrarLogs = true;

    // ─── ESTADO ──────────────────────────────────────────────────────────

    /// <summary>Vueltas completadas en el episodio actual.</summary>
    public int VueltasCompletadas { get; private set; }

    /// <summary>Número de vueltas calculado para este circuito (puede cambiar entre circuitos).</summary>
    public int VueltasNecesarias { get; private set; }

    /// <summary>Longitud del circuito actual en metros.</summary>
    public float LongitudCircuito { get; private set; }

    // ─── Unity lifecycle ─────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        CircuitoEventos.OnCircuitoCerrado += OnCircuitoListo;
        CircuitoEventos.OnCircuitoCargado += OnCircuitoListo;
    }

    private void OnDestroy()
    {
        CircuitoEventos.OnCircuitoCerrado -= OnCircuitoListo;
        CircuitoEventos.OnCircuitoCargado -= OnCircuitoListo;

        if (Instance == this)
            Instance = null;
    }

    // ─── Evento: circuito disponible ──────────────────────────────────────

    private void OnCircuitoListo(List<PiezaCircuito> piezas, float distanciaTotal)
    {
        LongitudCircuito = distanciaTotal;

        // Calcular vueltas según el modo elegido
        if (modoCarrera == ModoCarrera.PorDistancia)
        {
            if (LongitudCircuito > 0f)
                VueltasNecesarias = Mathf.Max(1, Mathf.RoundToInt(distanciaObjetivoCarrera / LongitudCircuito));
            else
                VueltasNecesarias = vueltasObjetivo; // fallback si no hay longitud
        }
        else
        {
            VueltasNecesarias = vueltasObjetivo;
        }

        VueltasCompletadas = 0;

        // Mostrar "Vuelta 1 / N": la carrera empieza siempre en la vuelta 1
        raceHUD?.ActualizarVueltas(VueltasCompletadas + 1, VueltasNecesarias);

        if (mostrarLogs)
        {
            Debug.Log($"[RaceManager] Circuito listo.\n" +
                      $"  Longitud      : {LongitudCircuito:F1} m\n" +
                      $"  Modo          : {modoCarrera}\n" +
                      $"  Vueltas necesarias: {VueltasNecesarias}" +
                      (modoCarrera == ModoCarrera.PorDistancia
                          ? $"  (≈ {LongitudCircuito * VueltasNecesarias:F0} m totales)"
                          : ""));
        }
    }

    // ─── API pública ──────────────────────────────────────────────────────

    /// <summary>
    /// Llamado por Meta1_0 cuando el agente cruza la línea de meta habiendo
    /// completado todos los checkpoints de esa vuelta.
    /// </summary>
    public void VueltaCompletada()
    {
        VueltasCompletadas++;

        // Actualizar HUD: si es la última vuelta mostrar N/N, si es intermedia mostrar la siguiente
        int vueltaMostrada = Mathf.Min(VueltasCompletadas, VueltasNecesarias);
        raceHUD?.ActualizarVueltas(vueltaMostrada, VueltasNecesarias);

        if (mostrarLogs)
            Debug.Log($"[RaceManager] Vuelta {VueltasCompletadas}/{VueltasNecesarias} completada.");

        Agente1_0 agente = Agente1_0.Instance;
        CheckPointsManager1_0 manager = CheckPointsManager1_0.Instance;

        if (agente == null || manager == null) return;

        if (VueltasCompletadas >= VueltasNecesarias)
        {
            // ── CARRERA TERMINADA ─────────────────────────────────────────
            if (mostrarLogs)
                Debug.Log("[RaceManager] ¡CARRERA COMPLETADA! Llamando a ScoredAGoal.");

            agente.ScoredAGoal();
        }
        else
        {
            // ── VUELTA INTERMEDIA: recompensa + reset de checkpoints ──────
            agente.AddReward(recompensaPorVuelta);

            // CRÍTICO: resetear el flag 'atravesado' de cada checkpoint individualmente,
            // o en la siguiente vuelta ninguno responderá al trigger del coche.
            foreach (var cp in manager.checkpp.checkPoints)
                cp.ResetTrigger();

            // Resetear el índice y nextCheckPointToReach del manager
            manager.ResetCheckpoints();

            // Actualizar HUD mostrando la vuelta que empieza (completadas + 1)
            raceHUD?.ActualizarVueltas(VueltasCompletadas + 1, VueltasNecesarias);

            if (mostrarLogs)
                Debug.Log($"[RaceManager] Vuelta intermedia. Recompensa +{recompensaPorVuelta}. " +
                          $"Checkpoints reseteados. Quedan {VueltasNecesarias - VueltasCompletadas} vuelta(s).");
        }
    }

    /// <summary>
    /// Reinicia el contador de vueltas. Llamar desde Agente1_0.OnEpisodeBegin.
    /// </summary>
    public void ResetCarrera()
    {
        VueltasCompletadas = 0;

        // Mostrar "Vuelta 1 / N" al reiniciar episodio
        raceHUD?.ActualizarVueltas(VueltasCompletadas + 1, VueltasNecesarias);

        if (mostrarLogs)
            Debug.Log("[RaceManager] Carrera reiniciada.");
    }
}