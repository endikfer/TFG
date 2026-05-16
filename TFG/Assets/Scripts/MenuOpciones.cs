using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuOpciones : MonoBehaviour
{
    [Header("UI - Pantalla")]
    public TMP_Dropdown dropdownPantalla;
    public TMP_Dropdown dropdownResolucion;
    public TMP_Dropdown dropdownCalidad;

    [Header("UI - Audio")]
    public Slider sliderVolumen;

    [Header("Panel")]
    public GameObject panelOpciones;

    // Resoluciones disponibles del monitor
    private Resolution[] _resoluciones;

    // Valores guardados (para revertir si se sale sin guardar)
    private int _pantallaGuardada;
    private int _resolucionGuardada;
    private int _calidadGuardada;
    private float _volumenGuardado;

    // Valores por defecto
    private const int PANTALLA_DEFAULT = 1;      // 1 = pantalla completa
    private const int RESOLUCION_DEFAULT = -1;   // -1 = resolución actual del monitor
    private const int CALIDAD_DEFAULT = 2;       // 2 = Medium
    private const float VOLUMEN_DEFAULT = 1f;    // 1 = 100%

    // ── Unity lifecycle ───────────────────────────────────────────────────

    private void Start()
    {
        panelOpciones.SetActive(false);
    }

    // ── API pública ───────────────────────────────────────────────────────

    /// <summary>
    /// Llamado por el botón "Opciones" del menú principal.
    /// Carga los valores guardados y abre el panel.
    /// </summary>
    public void AbrirOpciones()
    {
        CargarValoresGuardados();
        CargarResoluciones();
        CargarCalidades();
        AplicarValoresAUI();
        panelOpciones.SetActive(true);
    }

    /// <summary>
    /// Llamado por el botón "Guardar".
    /// Guarda los valores actuales en PlayerPrefs.
    /// </summary>
    public void Guardar()
    {
        _pantallaGuardada = dropdownPantalla.value;
        _resolucionGuardada = dropdownResolucion.value;
        _calidadGuardada = dropdownCalidad.value;
        _volumenGuardado = sliderVolumen.value;

        PlayerPrefs.SetInt("Pantalla", _pantallaGuardada);
        PlayerPrefs.SetInt("Resolucion", _resolucionGuardada);
        PlayerPrefs.SetInt("Calidad", _calidadGuardada);
        PlayerPrefs.SetFloat("Volumen", _volumenGuardado);
        PlayerPrefs.Save();

        Debug.Log("✅ Opciones guardadas.");
    }

    /// <summary>
    /// Llamado por el botón "Salir" del panel de opciones.
    /// Revierte los cambios no guardados y cierra el panel.
    /// </summary>
    public void SalirSinGuardar()
    {
        // Revertir cambios en tiempo real a los valores guardados
        AplicarPantalla(_pantallaGuardada);
        AplicarResolucion(_resolucionGuardada);
        AplicarCalidad(_calidadGuardada);
        AplicarVolumen(_volumenGuardado);

        panelOpciones.SetActive(false);
    }

    /// <summary>
    /// Llamado por el botón "Restablecer".
    /// Vuelve a los valores por defecto en UI y en tiempo real,
    /// pero NO guarda hasta que el usuario pulse Guardar.
    /// </summary>
    public void Restablecer()
    {
        // Calcular índice de resolución nativa del monitor
        int resolucionDefault = ObtenerIndiceResolucionActual();

        // Aplicar en tiempo real
        AplicarPantalla(PANTALLA_DEFAULT);
        AplicarResolucion(resolucionDefault);
        AplicarCalidad(CALIDAD_DEFAULT);
        AplicarVolumen(VOLUMEN_DEFAULT);

        // Actualizar UI
        dropdownPantalla.value = PANTALLA_DEFAULT;
        dropdownResolucion.value = resolucionDefault;
        dropdownCalidad.value = CALIDAD_DEFAULT;
        sliderVolumen.value = VOLUMEN_DEFAULT;

        dropdownPantalla.RefreshShownValue();
        dropdownResolucion.RefreshShownValue();
        dropdownCalidad.RefreshShownValue();

        Debug.Log("🔄 Opciones restablecidas a valores por defecto.");
    }

    // ── Callbacks de UI (asignar en el Inspector a OnValueChanged) ────────

    public void OnPantallaCambiada()
    {
        AplicarPantalla(dropdownPantalla.value);
    }

    public void OnResolucionCambiada()
    {
        AplicarResolucion(dropdownResolucion.value);
    }

    public void OnCalidadCambiada()
    {
        AplicarCalidad(dropdownCalidad.value);
    }

    public void OnVolumenCambiado()
    {
        AplicarVolumen(sliderVolumen.value);
    }

    // ── Carga inicial ─────────────────────────────────────────────────────

    private void CargarValoresGuardados()
    {
        _pantallaGuardada = PlayerPrefs.GetInt("Pantalla", PANTALLA_DEFAULT);
        _calidadGuardada = PlayerPrefs.GetInt("Calidad", CALIDAD_DEFAULT);
        _volumenGuardado = PlayerPrefs.GetFloat("Volumen", VOLUMEN_DEFAULT);
        // La resolución se carga después de poblar el dropdown
    }

    private void CargarResoluciones()
    {
        _resoluciones = Screen.resolutions;

        dropdownResolucion.ClearOptions();

        List<string> opciones = new List<string>();
        HashSet<string> vistas = new HashSet<string>();

        // Lista temporal sin duplicados (misma res con distinto Hz)
        List<Resolution> resolucionesFiltradas = new List<Resolution>();

        foreach (Resolution res in _resoluciones)
        {
            string clave = res.width + "x" + res.height;
            if (vistas.Contains(clave)) continue;
            vistas.Add(clave);
            resolucionesFiltradas.Add(res);
            opciones.Add(res.width + " x " + res.height);
        }

        // Guardar la lista filtrada para usarla al aplicar
        _resoluciones = resolucionesFiltradas.ToArray();

        dropdownResolucion.AddOptions(opciones);

        // Seleccionar la resolución guardada o la actual si no hay guardada
        int indiceGuardado = PlayerPrefs.GetInt("Resolucion", -1);
        if (indiceGuardado < 0 || indiceGuardado >= _resoluciones.Length)
            indiceGuardado = ObtenerIndiceResolucionActual();

        _resolucionGuardada = indiceGuardado;
        dropdownResolucion.value = indiceGuardado;
        dropdownResolucion.RefreshShownValue();
    }

    private void CargarCalidades()
    {
        dropdownCalidad.ClearOptions();

        // Usar los nombres definidos en Project Settings → Quality
        List<string> opciones = new List<string>(QualitySettings.names);
        dropdownCalidad.AddOptions(opciones);
    }

    private void AplicarValoresAUI()
    {
        dropdownPantalla.value = _pantallaGuardada;
        dropdownCalidad.value = _calidadGuardada;
        sliderVolumen.value = _volumenGuardado;

        dropdownPantalla.RefreshShownValue();
        dropdownCalidad.RefreshShownValue();

        // Aplicar en tiempo real al abrir
        AplicarPantalla(_pantallaGuardada);
        AplicarCalidad(_calidadGuardada);
        AplicarVolumen(_volumenGuardado);
    }

    // ── Aplicación en tiempo real ─────────────────────────────────────────

    private void AplicarPantalla(int valor)
    {
        // 0 = Ventana, 1 = Pantalla completa
        bool esCompleta = valor == 1;
        Screen.fullScreen = esCompleta;
    }

    private void AplicarResolucion(int indice)
    {
        if (_resoluciones == null || indice < 0 || indice >= _resoluciones.Length)
            return;

        Resolution res = _resoluciones[indice];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    private void AplicarCalidad(int indice)
    {
        QualitySettings.SetQualityLevel(indice);
    }

    private void AplicarVolumen(float valor)
    {
        AudioListener.volume = valor;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private int ObtenerIndiceResolucionActual()
    {
        if (_resoluciones == null) return 0;

        for (int i = 0; i < _resoluciones.Length; i++)
        {
            if (_resoluciones[i].width == Screen.currentResolution.width &&
                _resoluciones[i].height == Screen.currentResolution.height)
            {
                return i;
            }
        }

        // Si no encuentra coincidencia, devolver el último (suele ser el más alto)
        return _resoluciones.Length - 1;
    }
}