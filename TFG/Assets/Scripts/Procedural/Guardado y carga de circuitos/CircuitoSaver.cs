using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Escucha el evento OnCircuitoCerrado, muestra un panel para que el usuario
/// asigne un nombre y guarda el circuito en un archivo JSON.
/// - En el Editor guarda dentro del proyecto Unity (sincronizable con GitHub)
/// - En build guarda en Application.persistentDataPath (ruta estándar por plataforma)
/// </summary>
public class CircuitoSaver : MonoBehaviour
{
    [Header("Configuración de guardado")]
    [Tooltip("Subcarpeta donde se guardarán los circuitos.")]
    public string carpetaGuardado = "CircuitosGuardados";

    [Header("UI de nombrado")]
    [Tooltip("Panel que aparece al cerrar un circuito para pedir el nombre.")]
    public GameObject panelGuardado;

    [Tooltip("Campo de texto donde el usuario escribe el nombre.")]
    public TMP_InputField inputNombre;

    [Tooltip("Botón que confirma el guardado.")]
    public Button botonGuardar;

    [Tooltip("Texto que aparece cuando el nombre ya está en uso.")]
    public TMP_Text textoError;

    [Header("Debug")]
    public bool mostrarLogs = true;

    // Guardamos los datos pendientes hasta que el usuario confirme el nombre
    private List<PiezaCircuito> piezasPendientes;
    private float distanciaPendiente;

    // ── Modelos de datos serializables ──────────────────────────────────────

    [Serializable]
    private class DatosPieza
    {
        public string tipo;
        public float px, py, pz;   // posición world
        public float rx, ry, rz;   // rotación euler
    }

    [Serializable]
    private class DatosCircuito
    {
        public string nombre;
        public string fecha;
        public float distanciaTotal;
        public int numeroPiezas;
        public List<DatosPieza> piezas = new List<DatosPieza>();
    }

    // ── Unity lifecycle ─────────────────────────────────────────────────────

    private void Start()
    {
        // Conectar el botón al método de confirmación
        botonGuardar.onClick.AddListener(OnBotonGuardarPulsado);

        // Asegurarse de que el panel empieza oculto
        panelGuardado.SetActive(false);

        if (mostrarLogs)
            Debug.Log($"📁 Ruta de guardado activa:\n   {ObtenerDirectorioBase()}");
    }

    private void OnEnable()
    {
        CircuitoEventos.OnCircuitoCerrado += HandleCircuitoCerrado;
    }

    private void OnDisable()
    {
        CircuitoEventos.OnCircuitoCerrado -= HandleCircuitoCerrado;
    }

    // ── Handler del evento ──────────────────────────────────────────────────

    private void HandleCircuitoCerrado(List<PiezaCircuito> piezas, float distanciaTotal)
    {
        // Guardamos los datos y mostramos el panel para pedir nombre
        piezasPendientes = piezas;
        distanciaPendiente = distanciaTotal;

        inputNombre.text = "";  // Limpiar el campo por si había texto anterior
        if (textoError != null) textoError.gameObject.SetActive(false); // Ocultar error previo
        panelGuardado.SetActive(true);
        inputNombre.Select(); // Poner el foco en el campo de texto directamente
    }

    // ── Confirmación del usuario ────────────────────────────────────────────

    private void OnBotonGuardarPulsado()
    {
        string nombre = inputNombre.text.Trim();

        // Si el usuario no escribió nada, asignar timestamp automático
        if (string.IsNullOrEmpty(nombre))
            nombre = $"circuito_{DateTime.Now:yyyyMMdd_HHmmss}";

        // Comprobar si ya existe un archivo con ese nombre
        string rutaCompleta = Path.Combine(ObtenerDirectorioBase(), $"{nombre}.json");
        if (File.Exists(rutaCompleta))
        {
            // Mostrar mensaje de error y NO cerrar el panel
            if (textoError != null)
            {
                textoError.text = $"Ya existe un circuito con el nombre '{nombre}'. Elige otro nombre.";
                textoError.gameObject.SetActive(true);
            }
            if (mostrarLogs)
                Debug.LogWarning($"⚠️ Ya existe un archivo llamado '{nombre}.json'. Guardado cancelado.");
            return;
        }

        // Nombre disponible, proceder con el guardado
        if (textoError != null) textoError.gameObject.SetActive(false);
        panelGuardado.SetActive(false);
        GuardarCircuito(piezasPendientes, distanciaPendiente, nombre);
    }

    // ── Ruta de guardado ────────────────────────────────────────────────────

    /// <summary>
    /// Devuelve la ruta base según el contexto:
    /// - Editor:  raíz del proyecto Unity  →  se sincroniza con GitHub
    /// - Build:   persistentDataPath        →  ruta estándar por plataforma
    /// </summary>
    private string ObtenerDirectorioBase()
    {
#if UNITY_EDITOR
        string directorioBase = Path.Combine(Application.dataPath, "..", carpetaGuardado);
#else
        string directorioBase = Path.Combine(Application.persistentDataPath, carpetaGuardado);
#endif
        return Path.GetFullPath(directorioBase);
    }

    // ── Lógica de guardado ──────────────────────────────────────────────────

    private void GuardarCircuito(List<PiezaCircuito> piezas, float distanciaTotal, string nombre)
    {
        // Construir el modelo de datos
        DatosCircuito datos = new DatosCircuito
        {
            nombre = nombre,
            fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            distanciaTotal = distanciaTotal,
            numeroPiezas = piezas.Count
        };

        foreach (PiezaCircuito pieza in piezas)
        {
            Vector3 pos = pieza.transform.position;
            Vector3 euler = pieza.transform.eulerAngles;

            datos.piezas.Add(new DatosPieza
            {
                tipo = pieza.tipo.ToString(),
                px = pos.x,
                py = pos.y,
                pz = pos.z,
                rx = euler.x,
                ry = euler.y,
                rz = euler.z
            });
        }

        // Serializar a JSON
        string json = JsonUtility.ToJson(datos, prettyPrint: true);

        // Preparar directorio
        string directorioBase = ObtenerDirectorioBase();

        if (!Directory.Exists(directorioBase))
            Directory.CreateDirectory(directorioBase);

        // Si el usuario escribió un nombre se usa tal cual; si es timestamp ya es único
        string nombreArchivo = $"{nombre}.json";
        string rutaCompleta = Path.Combine(directorioBase, nombreArchivo);

        // Escribir
        File.WriteAllText(rutaCompleta, json);

        if (mostrarLogs)
        {
            Debug.Log($"💾 Circuito guardado:\n" +
                      $"   Nombre   : {nombre}\n" +
                      $"   Archivo  : {nombreArchivo}\n" +
                      $"   Ruta     : {rutaCompleta}\n" +
                      $"   Piezas   : {datos.numeroPiezas}\n" +
                      $"   Distancia: {distanciaTotal:F1}m");
        }
    }

    // ── Utilidad: listar circuitos guardados ────────────────────────────────

    /// <summary>
    /// Devuelve todas las rutas de circuitos guardados, ordenadas por fecha (más reciente primero).
    /// </summary>
    public string[] ObtenerCircuitosGuardados()
    {
        string directorio = ObtenerDirectorioBase();

        if (!Directory.Exists(directorio))
            return Array.Empty<string>();

        string[] archivos = Directory.GetFiles(directorio, "*.json");
        Array.Sort(archivos, (a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));

        return archivos;
    }
}