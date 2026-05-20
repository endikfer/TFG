using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

    [Header("Callback al cerrar")]
    public UnityEvent onCerrar;

    private Resolution[] _resoluciones;

    private int _pantallaGuardada;
    private int _resolucionGuardada;
    private int _calidadGuardada;
    private float _volumenGuardado;

    private const int PANTALLA_DEFAULT = 1;
    private const int RESOLUCION_DEFAULT = -1;
    private const int CALIDAD_DEFAULT = 2;
    private const float VOLUMEN_DEFAULT = 1f;

    private void Start()
    {
        panelOpciones.SetActive(false);
    }

    public void AbrirOpciones()
    {
        CargarValoresGuardados();
        CargarResoluciones();
        CargarCalidades();
        AplicarValoresAUI();
        panelOpciones.SetActive(true);
    }

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

    public void SalirSinGuardar()
    {
        AplicarPantalla(_pantallaGuardada);
        AplicarResolucion(_resolucionGuardada);
        AplicarCalidad(_calidadGuardada);
        AplicarVolumen(_volumenGuardado);

        panelOpciones.SetActive(false);
        onCerrar?.Invoke();
    }

    public void Restablecer()
    {
        int resolucionDefault = ObtenerIndiceResolucionActual();

        AplicarPantalla(PANTALLA_DEFAULT);
        AplicarResolucion(resolucionDefault);
        AplicarCalidad(CALIDAD_DEFAULT);
        AplicarVolumen(VOLUMEN_DEFAULT);

        dropdownPantalla.value = PANTALLA_DEFAULT;
        dropdownResolucion.value = resolucionDefault;
        dropdownCalidad.value = CALIDAD_DEFAULT;
        sliderVolumen.value = VOLUMEN_DEFAULT;

        dropdownPantalla.RefreshShownValue();
        dropdownResolucion.RefreshShownValue();
        dropdownCalidad.RefreshShownValue();
    }

    public void OnPantallaCambiada() => AplicarPantalla(dropdownPantalla.value);
    public void OnResolucionCambiada() => AplicarResolucion(dropdownResolucion.value);
    public void OnCalidadCambiada() => AplicarCalidad(dropdownCalidad.value);
    public void OnVolumenCambiado() => AplicarVolumen(sliderVolumen.value);

    private void CargarValoresGuardados()
    {
        _pantallaGuardada = PlayerPrefs.GetInt("Pantalla", PANTALLA_DEFAULT);
        _calidadGuardada = PlayerPrefs.GetInt("Calidad", CALIDAD_DEFAULT);
        _volumenGuardado = PlayerPrefs.GetFloat("Volumen", VOLUMEN_DEFAULT);
    }

    private void CargarResoluciones()
    {
        _resoluciones = Screen.resolutions;
        dropdownResolucion.ClearOptions();

        List<string> opciones = new List<string>();
        HashSet<string> vistas = new HashSet<string>();
        List<Resolution> resolucionesFiltradas = new List<Resolution>();

        foreach (Resolution res in _resoluciones)
        {
            string clave = res.width + "x" + res.height;
            if (vistas.Contains(clave)) continue;
            vistas.Add(clave);
            resolucionesFiltradas.Add(res);
            opciones.Add(res.width + " x " + res.height);
        }

        _resoluciones = resolucionesFiltradas.ToArray();
        dropdownResolucion.AddOptions(opciones);

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
        dropdownCalidad.AddOptions(new List<string>(QualitySettings.names));
    }

    private void AplicarValoresAUI()
    {
        dropdownPantalla.value = _pantallaGuardada;
        dropdownCalidad.value = _calidadGuardada;
        sliderVolumen.value = _volumenGuardado;

        dropdownPantalla.RefreshShownValue();
        dropdownCalidad.RefreshShownValue();

        AplicarPantalla(_pantallaGuardada);
        AplicarCalidad(_calidadGuardada);
        AplicarVolumen(_volumenGuardado);
    }

    private void AplicarPantalla(int valor) => Screen.fullScreen = valor == 1;

    private void AplicarResolucion(int indice)
    {
        if (_resoluciones == null || indice < 0 || indice >= _resoluciones.Length) return;
        Resolution res = _resoluciones[indice];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    private void AplicarCalidad(int indice) => QualitySettings.SetQualityLevel(indice);
    private void AplicarVolumen(float valor) => AudioListener.volume = valor;

    private int ObtenerIndiceResolucionActual()
    {
        if (_resoluciones == null) return 0;

        for (int i = 0; i < _resoluciones.Length; i++)
        {
            if (_resoluciones[i].width == Screen.currentResolution.width &&
                _resoluciones[i].height == Screen.currentResolution.height)
                return i;
        }

        return _resoluciones.Length - 1;
    }
}