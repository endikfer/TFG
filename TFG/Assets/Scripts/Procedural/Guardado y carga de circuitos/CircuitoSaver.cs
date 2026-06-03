using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

/// <summary>
/// Escucha el evento OnCircuitoCerrado y guarda el circuito en JSON.
///
/// En la escena Generar el nombre lo establece GeneradorUI antes de lanzar
/// la generación mediante EstablecerNombreBase(). CircuitoSaver ya no
/// muestra ningún panel propio: toda la UI de nombre la gestiona GeneradorUI.
///
/// - Editor: guarda en la raíz del proyecto (sincronizable con GitHub)
/// - Build:  guarda en Application.persistentDataPath
/// </summary>
public class CircuitoSaver : MonoBehaviour
{
    [Header("Configuración de guardado")]
    public string carpetaGuardado = "CircuitosGuardados";

    [Header("Modo lote")]
    [Tooltip("Cuando está activo el circuito se guarda automáticamente con nombreBase_N.")]
    public bool modoLoteActivo = false;

    [Header("Debug")]
    public bool mostrarLogs = true;

    // Nombre base establecido desde GeneradorUI antes de lanzar la generación
    private string nombreBase = "";

    // Contador para modo lote
    private int contadorLote = 0;

    // ── Modelos de datos serializables ──────────────────────────────────────

    [Serializable]
    private class DatosPieza
    {
        public string tipo;
        public string nombrePrefab;
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

    // ── Unity lifecycle ─────────────────────────────────────────────────────

    private void Start()
    {
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

    // ── API pública ─────────────────────────────────────────────────────────

    /// <summary>
    /// Llamado por GeneradorUI antes de lanzar la generación.
    /// Establece el nombre base y resetea el contador de lote.
    /// </summary>
    public void EstablecerNombreBase(string nombre)
    {
        nombreBase = nombre.Trim();
        contadorLote = 0;

        if (mostrarLogs)
            Debug.Log($"[CircuitoSaver] Nombre base establecido: '{nombreBase}'");
    }

    // ── Handler del evento ──────────────────────────────────────────────────

    private void HandleCircuitoCerrado(List<PiezaCircuito> piezas, float distanciaTotal)
    {
        string nombre;

        if (modoLoteActivo)
        {
            // Modo lote: nombreBase_N con el primer número libre
            contadorLote++;
            nombre = GenerarNombreLote(nombreBase, contadorLote);
        }
        else
        {
            // Modo normal: usar el nombre base directamente
            // Si ya existe, añadir timestamp para evitar colisión
            nombre = ResolverNombreUnico(nombreBase);
        }

        GuardarCircuito(piezas, distanciaTotal, nombre);
    }

    // ── Resolución de nombres ───────────────────────────────────────────────

    /// <summary>
    /// Para modo normal: si el nombre ya existe añade un sufijo numérico.
    /// </summary>
    private string ResolverNombreUnico(string base_)
    {
        if (string.IsNullOrEmpty(base_))
            base_ = $"circuito_{DateTime.Now:yyyyMMdd_HHmmss}";

        string directorio = ObtenerDirectorioBase();
        string candidato = base_;
        int intento = 1;

        while (File.Exists(Path.Combine(directorio, $"{candidato}.json")))
        {
            candidato = $"{base_}_{intento}";
            intento++;
        }

        return candidato;
    }

    /// <summary>
    /// Para modo lote: nombreBase_N con el primer número libre a partir de contador.
    /// </summary>
    private string GenerarNombreLote(string base_, int contador)
    {
        if (string.IsNullOrEmpty(base_))
            base_ = "circuito";

        string directorio = ObtenerDirectorioBase();
        string candidato = $"{base_}_{contador}";

        // Si por alguna razón ya existe ese número, buscar el siguiente libre
        while (File.Exists(Path.Combine(directorio, $"{candidato}.json")))
        {
            contador++;
            candidato = $"{base_}_{contador}";
        }

        return candidato;
    }

    // ── Ruta de guardado ────────────────────────────────────────────────────

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
                nombrePrefab = pieza.gameObject.name.Replace("(Clone)", "").Trim(),
                px = pos.x,
                py = pos.y,
                pz = pos.z,
                rx = euler.x,
                ry = euler.y,
                rz = euler.z
            });
        }

        string json = JsonUtility.ToJson(datos, prettyPrint: true);
        string directorioBase = ObtenerDirectorioBase();

        if (!Directory.Exists(directorioBase))
            Directory.CreateDirectory(directorioBase);

        string rutaCompleta = Path.Combine(directorioBase, $"{nombre}.json");
        File.WriteAllText(rutaCompleta, json);

        if (mostrarLogs)
        {
            Debug.Log($"💾 Circuito guardado:\n" +
                      $"   Nombre   : {nombre}\n" +
                      $"   Ruta     : {rutaCompleta}\n" +
                      $"   Piezas   : {datos.numeroPiezas}\n" +
                      $"   Distancia: {distanciaTotal:F1}m");
        }
    }

    // ── Utilidad pública ────────────────────────────────────────────────────

    public string[] ObtenerCircuitosGuardados()
    {
        string directorio = ObtenerDirectorioBase();

        if (!Directory.Exists(directorio))
            return Array.Empty<string>();

        string[] archivos = Directory.GetFiles(directorio, "*.json");
        Array.Sort(archivos, (a, b) =>
            File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));

        return archivos;
    }
}