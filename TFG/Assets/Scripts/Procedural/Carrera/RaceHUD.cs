using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Muestra en pantalla el contador de vueltas de la carrera actual.
///
/// COMPORTAMIENTO:
///   - Al arrancar la escena el panel está OCULTO (SetActive false).
///   - Se hace visible en cuanto CircuitoInicializador dispara OnTodoListo,
///     es decir, cuando circuito + coche + checkpoints están completamente listos.
///   - A partir de ahí se actualiza en cada vuelta completada.
///   - Si se regenera el circuito, vuelve a ocultarse hasta que el nuevo
///     circuito esté listo de nuevo.
///
/// SETUP EN UNITY:
///   1. Crea un panel (GameObject vacío o Image) en el Canvas, esquina superior
///      izquierda. Dentro ponle un TMP_Text hijo.
///   2. Asigna el panel al campo 'panelVueltas' y el TMP_Text a 'textoVueltas'.
///   3. Añade este componente al mismo GameObject del RaceManager.
/// </summary>
public class RaceHUD : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Panel raíz que contiene el texto de vueltas. Se activa/desactiva según el estado.")]
    public GameObject panelVueltas;

    [Tooltip("Texto TMP donde se mostrará el contador de vueltas.")]
    public TMP_Text textoVueltas;

    [Header("Formato")]
    [Tooltip("Etiqueta que aparece antes del contador. Ej: 'Vuelta', 'Lap'.")]
    public string etiqueta = "Vuelta";

    // ─── Unity lifecycle ──────────────────────────────────────────────────

    private void Awake()
    {
        // Asegurarse de que el panel empieza oculto independientemente de lo que haya en escena
        OcultarPanel();
    }

    private void OnEnable()
    {
        CircuitoInicializador.OnTodoListo += OnTodoListo;
        CircuitoEventos.OnCircuitoCerrado += OnCircuitoNuevo;
        CircuitoEventos.OnCircuitoCargado += OnCircuitoNuevo;
    }

    private void OnDisable()
    {
        CircuitoInicializador.OnTodoListo -= OnTodoListo;
        CircuitoEventos.OnCircuitoCerrado -= OnCircuitoNuevo;
        CircuitoEventos.OnCircuitoCargado -= OnCircuitoNuevo;
    }

    // ─── Handlers ─────────────────────────────────────────────────────────

    /// <summary>
    /// Cuando empieza a generarse/cargarse un circuito nuevo, ocultamos el panel
    /// hasta que la inicialización completa termine.
    /// </summary>
    private void OnCircuitoNuevo(List<PiezaCircuito> piezas, float distancia)
    {
        OcultarPanel();
    }

    /// <summary>
    /// Circuito + coche + checkpoints listos: mostrar panel con el contador inicial.
    /// </summary>
    private void OnTodoListo()
    {
        // Esperar un frame para que RaceManager ya haya calculado VueltasNecesarias
        StartCoroutine(MostrarTrasUnFrame());
    }

    private IEnumerator MostrarTrasUnFrame()
    {
        yield return null;

        if (RaceManager.Instance != null)
        {
            // Mostramos la vuelta actual (completadas + 1), no las ya completadas
            ActualizarVueltas(RaceManager.Instance.VueltasCompletadas + 1,
                              RaceManager.Instance.VueltasNecesarias);
            MostrarPanel();
        }
    }

    // ─── API pública (llamada desde RaceManager) ──────────────────────────

    /// <summary>
    /// Actualiza el texto del contador. Llamado por RaceManager en cada cambio.
    /// </summary>
    public void ActualizarVueltas(int completadas, int necesarias)
    {
        if (textoVueltas == null) return;
        textoVueltas.text = $"{etiqueta}  {completadas} / {necesarias}";
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private void MostrarPanel()
    {
        if (panelVueltas != null)
            panelVueltas.SetActive(true);
    }

    private void OcultarPanel()
    {
        if (panelVueltas != null)
            panelVueltas.SetActive(false);
    }

    // RaceHUD
    public void MostrarHUD()
    {
        if (RaceManager.Instance != null)
        {
            ActualizarVueltas(RaceManager.Instance.VueltasCompletadas + 1,
                              RaceManager.Instance.VueltasNecesarias);
            MostrarPanel();
        }
    }
}