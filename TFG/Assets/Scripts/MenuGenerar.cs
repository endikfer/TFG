using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controla la interfaz de usuario de la escena Generar.
///
/// FLUJO:
///   1. El usuario configura parámetros en el panel principal y pulsa Generar.
///   2. Se oculta el panel de parámetros y aparece el panel de nombre.
///   3. El usuario escribe el nombre base y pulsa Confirmar.
///   4. Se aplican los parámetros al Generador2, se comunica el nombre
///      al CircuitoSaver y arranca la generación.
///   5. Cancelar en el panel de nombre vuelve al panel de parámetros.
/// </summary>
public class MenuGenerar : MonoBehaviour
{
    // ── Referencias a sistemas ────────────────────────────────────────────

    [Header("Referencias")]
    public Generador2 generador;
    public CircuitoSaver circuitoSaver;

    // ── Panel principal (parámetros) ──────────────────────────────────────

    [Header("Panel de parámetros")]
    public GameObject panelParametros;

    public Slider sliderDistancia;
    public TMP_Text textoDistancia;
    public float distanciaMin = 100f;
    public float distanciaMax = 600f;

    public Slider sliderRectas;
    public TMP_Text textoRectas;
    public Slider sliderCurvaIzq;
    public TMP_Text textoCurvaIzq;
    public Slider sliderCurvaDer;
    public TMP_Text textoCurvaDer;

    public Toggle toggleLote;
    public GameObject panelCantidadLote;
    public TMP_InputField inputCantidadLote;

    public Button botonGenerar;
    public Button botonVolver;

    // ── Panel de nombre ───────────────────────────────────────────────────

    [Header("Panel de nombre")]
    public GameObject panelNombre;
    public TMP_InputField inputNombre;
    public TMP_Text textoPreviewLote;   // Preview "nombreBase_1, _2..." solo en modo lote
    public TMP_Text textoErrorNombre;   // Mensaje si el nombre ya existe
    public Button botonConfirmar;
    public Button botonCancelar;

    // ── Estado interno ────────────────────────────────────────────────────

    private bool actualizandoProbabilidades = false;

    // ── Unity lifecycle ───────────────────────────────────────────────────

    private void Start()
    {
        ConfigurarSliderDistancia();
        ConfigurarSlidersProbabilidad();
        ConfigurarToggleLote();
        ConfigurarBotones();

        // Estado inicial: solo panel de parámetros visible
        panelParametros.SetActive(true);
        panelNombre.SetActive(false);

        ActualizarVisibilidadLote(toggleLote.isOn);
    }

    // ── Configuración inicial ─────────────────────────────────────────────

    private void ConfigurarSliderDistancia()
    {
        sliderDistancia.minValue = distanciaMin;
        sliderDistancia.maxValue = distanciaMax;
        sliderDistancia.value = generador.distanciaObjetivo;

        ActualizarTextoDistancia(sliderDistancia.value);
        sliderDistancia.onValueChanged.AddListener(OnDistanciaCambiada);
    }

    private void ConfigurarSlidersProbabilidad()
    {
        sliderRectas.minValue = 0f; sliderRectas.maxValue = 1f;
        sliderCurvaIzq.minValue = 0f; sliderCurvaIzq.maxValue = 1f;
        sliderCurvaDer.minValue = 0f; sliderCurvaDer.maxValue = 1f;

        sliderRectas.value = generador.baseRectaProb;
        sliderCurvaIzq.value = generador.baseCurvaIzqProb;
        sliderCurvaDer.value = generador.baseCurvaDerProb;

        ActualizarTextosProbabilidad();

        sliderRectas.onValueChanged.AddListener(v =>
            OnProbabilidadCambiada(sliderRectas, sliderCurvaIzq, sliderCurvaDer));
        sliderCurvaIzq.onValueChanged.AddListener(v =>
            OnProbabilidadCambiada(sliderCurvaIzq, sliderRectas, sliderCurvaDer));
        sliderCurvaDer.onValueChanged.AddListener(v =>
            OnProbabilidadCambiada(sliderCurvaDer, sliderRectas, sliderCurvaIzq));
    }

    private void ConfigurarToggleLote()
    {
        toggleLote.isOn = generador.modoLote;
        toggleLote.onValueChanged.AddListener(ActualizarVisibilidadLote);

        inputCantidadLote.text = generador.circuitosObjetivo.ToString();
        inputCantidadLote.contentType = TMP_InputField.ContentType.IntegerNumber;
    }

    private void ConfigurarBotones()
    {
        botonGenerar.onClick.AddListener(OnBotonGenerar);
        botonVolver.onClick.AddListener(OnBotonVolver);
        botonConfirmar.onClick.AddListener(OnBotonConfirmar);
        botonCancelar.onClick.AddListener(OnBotonCancelar);
    }

    // ── Callbacks sliders ─────────────────────────────────────────────────

    private void OnDistanciaCambiada(float valor)
    {
        ActualizarTextoDistancia(valor);
    }

    private void OnProbabilidadCambiada(Slider sliderMovido, Slider sliderA, Slider sliderB)
    {
        if (actualizandoProbabilidades) return;
        actualizandoProbabilidades = true;

        float restante = Mathf.Clamp01(1f - sliderMovido.value);
        float totalOtros = sliderA.value + sliderB.value;

        if (totalOtros > 0.0001f)
        {
            sliderA.value = restante * (sliderA.value / totalOtros);
            sliderB.value = restante * (sliderB.value / totalOtros);
        }
        else
        {
            sliderA.value = restante / 2f;
            sliderB.value = restante / 2f;
        }

        ActualizarTextosProbabilidad();
        actualizandoProbabilidades = false;
    }

    private void ActualizarVisibilidadLote(bool activo)
    {
        panelCantidadLote.SetActive(activo);
    }

    // ── Botón Generar: muestra panel de nombre ────────────────────────────

    private void OnBotonGenerar()
    {
        // Limpiar estado del panel de nombre
        inputNombre.text = "";
        if (textoErrorNombre != null) textoErrorNombre.gameObject.SetActive(false);

        // Mostrar preview solo en modo lote
        if (textoPreviewLote != null)
            textoPreviewLote.gameObject.SetActive(toggleLote.isOn);

        // Cambiar de panel
        panelParametros.SetActive(false);
        panelNombre.SetActive(true);
        inputNombre.Select();
    }

    // ── Botón Confirmar: valida nombre y arranca generación ───────────────

    private void OnBotonConfirmar()
    {
        string nombreBase = inputNombre.text.Trim();

        // Si no escribió nada, generar nombre automático con timestamp
        if (string.IsNullOrEmpty(nombreBase))
            nombreBase = $"circuito_{System.DateTime.Now:yyyyMMdd_HHmmss}";

        // Comunicar el nombre base al CircuitoSaver antes de generar
        circuitoSaver.EstablecerNombreBase(nombreBase);

        // Aplicar parámetros y lanzar generación
        AplicarParametrosAlGenerador();
        panelNombre.SetActive(false);
        generador.Regenerar();
    }

    // ── Botón Cancelar: vuelve al panel de parámetros ─────────────────────

    private void OnBotonCancelar()
    {
        panelNombre.SetActive(false);
        panelParametros.SetActive(true);
    }

    // ── Botón Volver: sale de la escena ───────────────────────────────────

    private void OnBotonVolver()
    {
        string origen = PlayerPrefs.GetString("EscenaOrigen", "Inicio");
        SceneManager.LoadScene(origen);
    }

    // ── Aplicar parámetros al Generador2 ──────────────────────────────────

    private void AplicarParametrosAlGenerador()
    {
        generador.distanciaObjetivo = sliderDistancia.value;
        generador.baseRectaProb = sliderRectas.value;
        generador.baseCurvaIzqProb = sliderCurvaIzq.value;
        generador.baseCurvaDerProb = sliderCurvaDer.value;
        generador.modoLote = toggleLote.isOn;

        if (toggleLote.isOn)
        {
            if (int.TryParse(inputCantidadLote.text, out int cantidad) && cantidad > 0)
                generador.circuitosObjetivo = cantidad;
            else
                generador.circuitosObjetivo = 1;
        }
    }

    // ── Helpers de texto ──────────────────────────────────────────────────

    private void ActualizarTextoDistancia(float valor)
    {
        if (textoDistancia != null)
            textoDistancia.text = $"{Mathf.RoundToInt(valor)} m";
    }

    private void ActualizarTextosProbabilidad()
    {
        if (textoRectas != null) textoRectas.text = $"{sliderRectas.value:F2}";
        if (textoCurvaIzq != null) textoCurvaIzq.text = $"{sliderCurvaIzq.value:F2}";
        if (textoCurvaDer != null) textoCurvaDer.text = $"{sliderCurvaDer.value:F2}";
    }

    // ── Preview nombre lote (llamado desde inputNombre.onValueChanged) ────

    public void ActualizarPreviewLote()
    {
        if (textoPreviewLote == null || !toggleLote.isOn) return;

        string base_ = inputNombre.text.Trim();
        if (string.IsNullOrEmpty(base_)) base_ = "circuito";

        int cantidad = 1;
        if (int.TryParse(inputCantidadLote.text, out int c) && c > 0) cantidad = c;

        if (cantidad <= 3)
        {
            string preview = "";
            for (int i = 1; i <= cantidad; i++)
                preview += $"{base_}_{i}  ";
            textoPreviewLote.text = $"Se guardarán como: {preview.Trim()}";
        }
        else
        {
            textoPreviewLote.text = $"Se guardarán como: {base_}_1, {base_}_2 ... {base_}_{cantidad}";
        }
    }

    public void MostrarPanelParametros()
    {
        panelNombre.SetActive(false);
        panelParametros.SetActive(true);
    }
}