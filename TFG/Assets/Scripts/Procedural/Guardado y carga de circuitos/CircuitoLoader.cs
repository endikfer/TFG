using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CircuitoLoader : MonoBehaviour
{
    [Header("Referencia al generador")]
    public Generador2 generador;

    [Header("Prefabs disponibles")]
    [Tooltip("Los mismos prefabs que usa el Generador2, en el mismo orden.")]
    public PiezaCircuito[] piezasPrefab;
    public Transform circuitoParent;

    [Header("UI del selector")]
    public GameObject panelCarga;
    public Transform contenedorLista;       // ScrollView Content donde se instancian los botones
    public GameObject prefabBotonCircuito;  // Botón prefab con TMP_Text y Button
    public TMP_Text textoEstado;            // Feedback al usuario
    public Button botonCerrarPanel;

    [Header("Configuración")]
    public string carpetaGuardado = "CircuitosGuardados"; // Debe coincidir con CircuitoSaver

    [Header("Debug")]
    public bool mostrarLogs = true;

    // ── Modelos de datos (deben coincidir con CircuitoSaver) ──────────────

    [Serializable]
    private class DatosPieza
    {
        public string tipo;
        public float px, py, pz;
        public float rx, ry, rz;
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

    // ── Unity lifecycle ───────────────────────────────────────────────────

    private void Start()
    {
        panelCarga.SetActive(false);
        botonCerrarPanel.onClick.AddListener(() =>
        {
            panelCarga.SetActive(false);
            generador.Regenerar();
        });
    }

    // ── API pública ───────────────────────────────────────────────────────

    /// <summary>
    /// Abre el panel y muestra todos los circuitos disponibles.
    /// Llámalo desde un botón "Cargar circuito" en tu UI.
    /// </summary>
    public void AbrirPanelCarga()
    {
        generador.StopAllCoroutines();
        RefrescarLista();
        panelCarga.SetActive(true);
    }

    // ── Lista de circuitos ────────────────────────────────────────────────

    private void RefrescarLista()
    {
        // Limpiar botones anteriores
        foreach (Transform hijo in contenedorLista)
            Destroy(hijo.gameObject);

        string[] archivos = ObtenerArchivosCircuitos();

        if (archivos.Length == 0)
        {
            if (textoEstado != null)
                textoEstado.text = "No hay circuitos guardados.";
            return;
        }

        if (textoEstado != null)
            textoEstado.text = $"{archivos.Length} circuito(s) disponibles.";

        foreach (string ruta in archivos)
        {
            string nombre = Path.GetFileNameWithoutExtension(ruta);
            string rutaCopia = ruta; // Captura para el closure

            // Instanciar botón
            GameObject boton = Instantiate(prefabBotonCircuito, contenedorLista);
            boton.GetComponentInChildren<TMP_Text>().text = nombre;
            boton.GetComponent<Button>().onClick.AddListener(() => CargarCircuito(rutaCopia));
        }
    }

    private string[] ObtenerArchivosCircuitos()
    {
        string directorio = ObtenerDirectorioBase();

        if (!Directory.Exists(directorio))
            return Array.Empty<string>();

        string[] archivos = Directory.GetFiles(directorio, "*.json");
        // Más reciente primero (igual que CircuitoSaver.ObtenerCircuitosGuardados)
        Array.Sort(archivos, (a, b) =>
            File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));

        return archivos;
    }

    // ── Carga de circuito ─────────────────────────────────────────────────

    private void CargarCircuito(string rutaArchivo)
    {
        panelCarga.SetActive(false);

        if (!File.Exists(rutaArchivo))
        {
            Debug.LogError($"❌ Archivo no encontrado: {rutaArchivo}");
            return;
        }

        string json = File.ReadAllText(rutaArchivo);
        DatosCircuito datos = JsonUtility.FromJson<DatosCircuito>(json);

        if (datos == null || datos.piezas == null || datos.piezas.Count == 0)
        {
            Debug.LogError($"❌ JSON inválido o vacío: {rutaArchivo}");
            return;
        }

        // Usar LimpiarTodo del generador en vez de limpieza propia
        generador.StopAllCoroutines();
        generador.LimpiarTodo();

        StartCoroutine(ReconstruirCircuito(datos));
    }

    private IEnumerator ReconstruirCircuito(DatosCircuito datos)
    {
        List<PiezaCircuito> piezasInstanciadas = new List<PiezaCircuito>();
        int operacionesPorFrame = 15;
        int ops = 0;

        foreach (DatosPieza datosPieza in datos.piezas)
        {
            // Encontrar el prefab que coincide con el tipo
            PiezaCircuito prefab = BuscarPrefabPorTipo(datosPieza.tipo);

            if (prefab == null)
            {
                Debug.LogWarning($"⚠️ Prefab no encontrado para tipo '{datosPieza.tipo}'. Pieza ignorada.");
                continue;
            }

            PiezaCircuito nueva = Instantiate(prefab, circuitoParent);
            nueva.transform.position = new Vector3(datosPieza.px, datosPieza.py, datosPieza.pz);
            nueva.transform.eulerAngles = new Vector3(datosPieza.rx, datosPieza.ry, datosPieza.rz);
            piezasInstanciadas.Add(nueva);

            ops++;
            if (ops >= operacionesPorFrame)
            {
                ops = 0;
                yield return null;
            }
        }

        if (mostrarLogs)
        {
            Debug.Log($"✅ Circuito '{datos.nombre}' cargado:\n" +
                      $"   Piezas: {piezasInstanciadas.Count}/{datos.numeroPiezas}\n" +
                      $"   Distancia: {datos.distanciaTotal:F1}m\n" +
                      $"   Guardado el: {datos.fecha}");
        }

        // Notificar al resto del sistema (misma firma que al cerrar un circuito generado)
        CircuitoEventos.NotificarCircuitoCargado(piezasInstanciadas, datos.distanciaTotal);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private PiezaCircuito BuscarPrefabPorTipo(string tipoString)
    {
        // Parsear el enum desde el string guardado
        if (!Enum.TryParse(tipoString, out PiezaCircuito.TipoPieza tipo))
            return null;

        foreach (var prefab in piezasPrefab)
        {
            if (prefab.tipo == tipo)
                return prefab;
        }

        return null;
    }

    private void LimpiarCircuitoActual()
    {
        for (int i = circuitoParent.childCount - 1; i >= 0; i--)
            Destroy(circuitoParent.GetChild(i).gameObject);

        if (mostrarLogs)
            Debug.Log("🧹 Circuito anterior eliminado para cargar uno guardado.");
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