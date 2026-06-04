using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Gestiona el panel de resultado de la escena Generar.
///
/// Se activa automáticamente cuando:
///   - Modo normal: CircuitoEventos.OnCircuitoCerrado
///   - Modo lote en progreso: Generador2.OnProgresoLote
///   - Modo lote completado: Generador2.OnLoteCompletado
///
/// SETUP EN UNITY:
///   1. Crea un GameObject vacío y añádele este script.
///   2. Asigna todas las referencias del Inspector.
///   3. El canvasResultado empieza desactivado; este script lo activa cuando toca.
/// </summary>
public class MenuResultado : MonoBehaviour
{
    [Header("Referencias")]
    public Generador2 generador;
    public MenuGenerar generadorUI;

    [Header("Canvas de resultado")]
    public GameObject canvasResultado;

    [Header("Textos")]
    public TMP_Text textoTitulo;
    public TMP_Text textoInfo;

    [Header("Barra de progreso (solo lote)")]
    public Slider sliderProgreso;

    [Header("Botones")]
    public Button botonGenerarOtro;
    public Button botonVolverMenu;

    // ── Unity lifecycle ───────────────────────────────────────────────────

    private void Start()
    {
        canvasResultado.SetActive(false);

        botonGenerarOtro.onClick.AddListener(OnBotonGenerarOtro);
        botonVolverMenu.onClick.AddListener(OnBotonVolverMenu);
    }

    private void OnEnable()
    {
        CircuitoEventos.OnCircuitoCerrado += OnCircuitoNormalCompletado;
        Generador2.OnProgresoLote += OnProgresoLote;
        Generador2.OnLoteCompletado += OnLoteCompletado;
    }

    private void OnDisable()
    {
        CircuitoEventos.OnCircuitoCerrado -= OnCircuitoNormalCompletado;
        Generador2.OnProgresoLote -= OnProgresoLote;
        Generador2.OnLoteCompletado -= OnLoteCompletado;
    }

    // ── Handlers de eventos ───────────────────────────────────────────────

    private void OnCircuitoNormalCompletado(List<PiezaCircuito> piezas, float distancia)
    {
        // Solo actuar si NO estamos en modo lote
        if (generador.modoLote) return;

        textoTitulo.text = "¡Circuito generado!";
        textoInfo.text = $"{Mathf.RoundToInt(distancia)} m  ·  {piezas.Count} piezas";

        MostrarBarraProgreso(false);
        ActivarBotones(true);
        canvasResultado.SetActive(true);
    }

    private void OnProgresoLote(int hechos, int total)
    {
        textoTitulo.text = "Generando lote...";
        textoInfo.text = $"{hechos} / {total} circuitos";

        if (sliderProgreso != null)
        {
            sliderProgreso.minValue = 0;
            sliderProgreso.maxValue = total;
            sliderProgreso.value = hechos;
        }

        MostrarBarraProgreso(true);
        ActivarBotones(false);
        canvasResultado.SetActive(true);
    }

    private void OnLoteCompletado(int guardados, int total)
    {
        textoTitulo.text = "¡Lote completado!";
        textoInfo.text = $"{guardados} / {total} circuitos guardados";

        if (sliderProgreso != null)
            sliderProgreso.value = total;

        MostrarBarraProgreso(false);
        ActivarBotones(true);
    }

    // ── Botones ───────────────────────────────────────────────────────────

    private void OnBotonGenerarOtro()
    {
        canvasResultado.SetActive(false);
        generadorUI.MostrarPanelParametros();
    }

    private void OnBotonVolverMenu()
    {
        string origen = PlayerPrefs.GetString("EscenaOrigen", "Inicio");
        SceneManager.LoadScene(origen);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void MostrarBarraProgreso(bool visible)
    {
        if (sliderProgreso != null)
            sliderProgreso.gameObject.SetActive(visible);
    }

    private void ActivarBotones(bool activos)
    {
        botonGenerarOtro.interactable = activos;
        botonVolverMenu.interactable = activos;
    }
}