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
        InicializarCarrera(distanciaTotal);
    }

    public void InicializarCarrera(float distanciaTotal)
    {
        LongitudCircuito = distanciaTotal;

        if (modoCarrera == ModoCarrera.PorDistancia)
        {
            if (LongitudCircuito > 0f)
                VueltasNecesarias = Mathf.Max(1, Mathf.RoundToInt(distanciaObjetivoCarrera / LongitudCircuito));
            else
                VueltasNecesarias = vueltasObjetivo;
        }
        else
        {
            VueltasNecesarias = vueltasObjetivo;
        }

        VueltasCompletadas = 0;

        if (mostrarLogs)
            Debug.Log($"[RaceManager] Carrera inicializada. Vueltas: {VueltasNecesarias}");
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
            manager.ResetCheckpoints();

            // Actualizar HUD mostrando la vuelta que empieza (completadas + 1)
            raceHUD?.ActualizarVueltas(VueltasCompletadas + 1, VueltasNecesarias);

            if (mostrarLogs)
                Debug.Log($"[RaceManager] Vuelta intermedia. Recompensa +{recompensaPorVuelta}. " +
                          $"Checkpoints reseteados. Quedan {VueltasNecesarias - VueltasCompletadas} vuelta(s).");
        }
    }

    /// <summary>
    /// Reinicia el contador de vueltas. Llamado por TrainingManager en cada nuevo episodio.
    /// Solo actualiza el HUD si VueltasNecesarias ya está calculado (> 0), para evitar
    /// mostrar "1/0" durante el arranque inicial antes de que llegue el primer circuito.
    /// </summary>
    public void ResetCarrera()
    {
        VueltasCompletadas = 0;

        // Guardia: si VueltasNecesarias es 0 el circuito aún no se ha cargado,
        // no actualizamos el HUD para evitar mostrar "Vuelta 1/0".
        if (VueltasNecesarias > 0)
            raceHUD?.ActualizarVueltas(VueltasCompletadas + 1, VueltasNecesarias);

        if (mostrarLogs)
            Debug.Log("[RaceManager] Carrera reiniciada.");
    }
}