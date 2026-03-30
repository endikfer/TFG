using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Gestiona el entrenamiento automático del agente ML sobre múltiples circuitos guardados.
///
/// FLUJO:
///   1. Al arrancar, escanea la carpeta de circuitos y carga uno aleatorio.
///   2. Cuenta episodios. Cada 'episodiosPorCircuito' episodios, señaliza que hay que
///      cambiar de circuito. El cambio ocurre al inicio del siguiente episodio (nunca
///      interrumpe un episodio en curso).
///   3. En cada episodio nuevo llama a RaceManager.ResetCarrera() para que
///      VueltasCompletadas vuelva a 0.
///
/// DEPENDENCIAS:
///   - CircuitoLoader    → carga física del circuito desde JSON
///   - RaceManager       → resetea el contador de vueltas
///   - Agente1_0         → notifica inicio de episodio vía evento
///
/// CONFIGURACIÓN EN INSPECTOR:
///   - carpetaGuardado        : misma que en CircuitoSaver / CircuitoLoader
///   - episodiosPorCircuito   : cada cuántos episodios se rota el circuito
///   - soloEntrenamiento      : si false, este script no hace nada (para no
///                              interferir con el modo manual / juego normal)
/// </summary>
public class EntrenamientoManager : MonoBehaviour
{
    // ─── SINGLETON ────────────────────────────────────────────────────────
    public static EntrenamientoManager Instance { get; private set; }

    // ─── CONFIGURACIÓN ────────────────────────────────────────────────────

    [Header("Activación")]
    [Tooltip("Desactiva este componente para el modo manual/juego. Solo activo durante entrenamientos.")]
    public bool soloEntrenamiento = true;

    [Header("Circuitos")]
    [Tooltip("Debe coincidir con la carpeta configurada en CircuitoSaver y CircuitoLoader.")]
    public string carpetaGuardado = "CircuitosGuardados";

    [Header("Rotación de circuitos")]
    [Tooltip("Número de episodios que se juegan en cada circuito antes de rotar.")]
    [Min(1)]
    public int episodiosPorCircuito = 10;

    [Header("Referencias")]
    [Tooltip("El CircuitoLoader que ya tienes en escena. Asignar en Inspector.")]
    public CircuitoLoader circuitoLoader;

    [Header("Debug")]
    public bool mostrarLogs = true;


    public bool usarCircuitoInicialDeEscena = true;

    // ─── ESTADO INTERNO ───────────────────────────────────────────────────

    private List<string> rutasCircuitos = new List<string>();
    private int indiceCircuitoActual = -1;   // -1 = ninguno cargado aún
    private int episodiosEnCircuitoActual = 0;
    private bool cambioCircuitoPendiente = false;
    private bool cargandoCircuito = false;

    // ─── Unity lifecycle ──────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (!soloEntrenamiento)
        {
            if (mostrarLogs)
                Debug.Log("[TrainingManager] Modo entrenamiento desactivado. Sin efecto.");
            enabled = false;
            return;
        }

        if (circuitoLoader == null)
        {
            Debug.LogError("[TrainingManager] CircuitoLoader no asignado en el Inspector.");
            enabled = false;
            return;
        }

        // Escanear circuitos disponibles
        EscanearCircuitos();

        if (rutasCircuitos.Count == 0)
        {
            Debug.LogError("[TrainingManager] No se encontraron circuitos guardados en: " + ObtenerDirectorioBase());
            enabled = false;
            return;
        }

        // Suscribirse al evento de inicio de episodio del agente
        Agente1_0.OnNuevoEpisodio += OnEpisodioIniciado;

        // Cargar el primer circuito
        if (!usarCircuitoInicialDeEscena)
            StartCoroutine(CargarCircuitoAleatorio(primeraCarga: true));
    }

    private void OnDestroy()
    {
        Agente1_0.OnNuevoEpisodio += OnEpisodioIniciado;

        if (Instance == this)
            Instance = null;
    }

    // ─── Lógica principal ─────────────────────────────────────────────────

    /// <summary>
    /// Llamado por Agente1_0 al inicio de cada episodio.
    /// Aquí reseteamos la carrera y, si toca, señalizamos el cambio de circuito.
    /// </summary>
    private void OnEpisodioIniciado()
    {
        // 1. Resetear contador de vueltas en cada episodio nuevo
        if (RaceManager.Instance != null)
            RaceManager.Instance.ResetCarrera();

        // 2. No actuar si ya estamos cargando un circuito
        if (cargandoCircuito) return;

        episodiosEnCircuitoActual++;

        if (mostrarLogs)
            Debug.Log($"[TrainingManager] Episodio {episodiosEnCircuitoActual}/{episodiosPorCircuito} " +
                      $"en circuito {indiceCircuitoActual + 1}/{rutasCircuitos.Count}.");

        // 3. ¿Toca cambiar de circuito?
        if (episodiosEnCircuitoActual >= episodiosPorCircuito)
        {
            cambioCircuitoPendiente = true;
            episodiosEnCircuitoActual = 0;
            StartCoroutine(CargarCircuitoAleatorio(primeraCarga: false));
        }
    }

    // ─── Carga de circuito ────────────────────────────────────────────────

    /// <summary>
    /// Elige un circuito aleatorio (distinto al actual si hay más de uno),
    /// y lo carga usando el pipeline existente de CircuitoLoader.
    /// </summary>
    private IEnumerator CargarCircuitoAleatorio(bool primeraCarga)
    {
        cargandoCircuito = true;

        // Elegir índice aleatorio distinto al actual
        int nuevoIndice = ElegirIndiceAleatorio();
        indiceCircuitoActual = nuevoIndice;
        string ruta = rutasCircuitos[nuevoIndice];

        if (mostrarLogs)
        {
            string nombreArchivo = Path.GetFileNameWithoutExtension(ruta);
            Debug.Log($"[TrainingManager] {(primeraCarga ? "Cargando circuito inicial" : "Rotando circuito")}: " +
                      $"'{nombreArchivo}' ({nuevoIndice + 1}/{rutasCircuitos.Count})");
        }

        // Usar el método público de CircuitoLoader para cargar el circuito.
        // Este método ya se encarga de: limpiar el circuito anterior, instanciar piezas,
        // y disparar CircuitoEventos.NotificarCircuitoCargado → que activa
        // CircuitoInicializador → que respawna el coche y reconfigura checkpoints.
        circuitoLoader.CargarCircuitoParaEntrenamiento(ruta);

        // Esperar a que CircuitoInicializador termine su corrutina de inicialización.
        // El evento OnCircuitoCargado es síncrono pero InicializarCircuito tiene yields,
        // así que esperamos unos frames para que todo esté listo.
        yield return new WaitForSeconds(0.5f);

        cambioCircuitoPendiente = false;
        cargandoCircuito = false;

        if (mostrarLogs)
            Debug.Log("[TrainingManager] Circuito listo para entrenamiento.");
    }

    private int ElegirIndiceAleatorio()
    {
        if (rutasCircuitos.Count == 1)
            return 0;

        int nuevoIndice;
        int intentos = 0;
        do
        {
            nuevoIndice = UnityEngine.Random.Range(0, rutasCircuitos.Count);
            intentos++;
        }
        while (nuevoIndice == indiceCircuitoActual && intentos < 10);

        return nuevoIndice;
    }

    // ─── Escaneo de carpeta ───────────────────────────────────────────────

    private void EscanearCircuitos()
    {
        rutasCircuitos.Clear();
        string directorio = ObtenerDirectorioBase();

        if (!Directory.Exists(directorio))
        {
            Debug.LogWarning($"[TrainingManager] Directorio no encontrado: {directorio}");
            return;
        }

        string[] archivos = Directory.GetFiles(directorio, "*.json");
        rutasCircuitos.AddRange(archivos);

        // Mezclar la lista para que el orden de rotación sea aleatorio
        for (int i = rutasCircuitos.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (rutasCircuitos[i], rutasCircuitos[j]) = (rutasCircuitos[j], rutasCircuitos[i]);
        }

        if (mostrarLogs)
            Debug.Log($"[TrainingManager] {rutasCircuitos.Count} circuitos encontrados en '{directorio}'.");
    }

    private string ObtenerDirectorioBase()
    {
#if UNITY_EDITOR
        string base_ = Path.Combine(Application.dataPath, "..", carpetaGuardado);
#else
        string base_ = Path.Combine(Application.persistentDataPath, carpetaGuardado);
#endif
        return Path.GetFullPath(base_);
    }
}