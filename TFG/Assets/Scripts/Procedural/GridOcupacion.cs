using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grid de ocupación espacial dinámico para el generador de circuitos.
/// 
/// Divide el plano XZ en celdas cuadradas de tamaño configurable.
/// Cada celda se identifica con un Vector2Int y se almacena en un HashSet,
/// por lo que el grid crece dinámicamente sin tamaño predefinido.
/// 
/// USO EN GENERADOR:
///   1. Llamar a Limpiar() al inicio de cada generación.
///   2. Al colocar una pieza definitivamente → MarcarPieza(pieza)
///   3. Al evaluar una candidata → EstaLibre(candidata)
///   4. Al hacer backtracking y destruir piezas → DesmarcarPieza(pieza)
/// </summary>
public class GridOcupacion : MonoBehaviour
{
    [Header("Configuración del grid")]
    [Tooltip("Tamaño de cada celda en metros. Ajustar según la escala de las piezas. " +
             "Valores recomendados: 1-3m para piezas pequeñas, 3-6m para piezas grandes.")]
    public float tamañoCelda = 2f;

    [Tooltip("Margen adicional en metros que se añade al Bounds de cada pieza al marcarla. " +
             "Evita que piezas contiguas se toquen en los bordes. Valor recomendado: 0.5-1m.")]
    public float margenExtra = 0.5f;

    [Tooltip("Activar para ver en la ventana Scene las celdas ocupadas (solo Editor).")]
    public bool mostrarGizmos = true;

    [Tooltip("Color de las celdas ocupadas en los Gizmos.")]
    public Color colorCeldaOcupada = new Color(1f, 0f, 0f, 0.25f);

    [Header("Debug")]
    public bool mostrarLogs = false;

    // HashSet dinámico: solo almacena celdas realmente ocupadas
    private HashSet<Vector2Int> celdasOcupadas = new HashSet<Vector2Int>();

    // Registro de qué celdas marcó cada pieza (necesario para desmarcar en backtracking)
    private Dictionary<PiezaCircuito, List<Vector2Int>> celdasPorPieza
        = new Dictionary<PiezaCircuito, List<Vector2Int>>();

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>
    /// Limpia el grid completamente. Llamar al inicio de cada generación.
    /// </summary>
    public void Limpiar()
    {
        celdasOcupadas.Clear();
        celdasPorPieza.Clear();

        if (mostrarLogs)
            Debug.Log("🗺️ GridOcupacion: grid limpiado.");
    }

    /// <summary>
    /// Marca las celdas que ocupa una pieza como ocupadas.
    /// Llamar después de confirmar que la pieza se coloca definitivamente.
    /// </summary>
    public void MarcarPieza(PiezaCircuito pieza)
    {
        List<Vector2Int> celdas = ObtenerCeldasDePieza(pieza, margenExtra);

        foreach (var celda in celdas)
            celdasOcupadas.Add(celda);

        // Registrar para poder desmarcar en backtracking
        celdasPorPieza[pieza] = celdas;

        if (mostrarLogs)
            Debug.Log($"🗺️ GridOcupacion: pieza {pieza.tipo} marcó {celdas.Count} celdas.");
    }

    /// <summary>
    /// Versión especial de MarcarPieza para la pieza inicial.
    /// Marca todas sus celdas EXCEPTO las cercanas al puntoEntrada,
    /// dejando libre la zona donde debe conectar la última pieza de cierre.
    /// </summary>
    /// <param name="pieza">La pieza inicial del circuito.</param>
    /// <param name="radioExclusion">Radio en unidades mundo alrededor del puntoEntrada
    /// que se deja sin marcar. Recomendado: ancho de pieza * 0.75</param>
    public void MarcarPiezaInicial(PiezaCircuito pieza, float radioExclusion)
    {
        List<Vector2Int> todasLasCeldas = ObtenerCeldasDePieza(pieza, margenExtra);

        Vector3 posEntrada = pieza.puntoEntrada.position;

        // Filtrar: excluir celdas dentro del radio de exclusión alrededor del puntoEntrada
        List<Vector2Int> celdasFiltradas = new List<Vector2Int>();

        foreach (var celda in todasLasCeldas)
        {
            // Centro de la celda en espacio mundo (ignoramos Y)
            Vector3 centroCelda = new Vector3(
                (celda.x + 0.5f) * tamañoCelda,
                0f,
                (celda.y + 0.5f) * tamañoCelda
            );

            float distancia = Vector3.Distance(
                new Vector3(centroCelda.x, 0f, centroCelda.z),
                new Vector3(posEntrada.x, 0f, posEntrada.z)
            );

            if (distancia > radioExclusion)
                celdasFiltradas.Add(celda);
        }

        foreach (var celda in celdasFiltradas)
            celdasOcupadas.Add(celda);

        // Registrar las celdas filtradas (sin la zona de cierre)
        celdasPorPieza[pieza] = celdasFiltradas;

        if (mostrarLogs)
        {
            int excluidas = todasLasCeldas.Count - celdasFiltradas.Count;
            Debug.Log($"🗺️ GridOcupacion: pieza inicial marcó {celdasFiltradas.Count} celdas " +
                      $"({excluidas} excluidas cerca del puntoEntrada, radio={radioExclusion}).");
        }
    }

    /// <summary>
    /// Desmarca las celdas de una pieza. Llamar cuando se destruye una pieza en backtracking.
    /// IMPORTANTE: solo desmarca celdas que no comparte con otras piezas activas.
    /// </summary>
    public void DesmarcarPieza(PiezaCircuito pieza)
    {
        if (!celdasPorPieza.ContainsKey(pieza))
            return;

        List<Vector2Int> celdasADesmarcar = celdasPorPieza[pieza];

        // Reconstruir el conjunto de celdas que siguen ocupadas por otras piezas
        HashSet<Vector2Int> celdasDeOtrasPiezas = new HashSet<Vector2Int>();
        foreach (var kvp in celdasPorPieza)
        {
            if (kvp.Key == pieza) continue;
            foreach (var c in kvp.Value)
                celdasDeOtrasPiezas.Add(c);
        }

        // Solo desmarcar las que no están en otras piezas
        int desmarcadas = 0;
        foreach (var celda in celdasADesmarcar)
        {
            if (!celdasDeOtrasPiezas.Contains(celda))
            {
                celdasOcupadas.Remove(celda);
                desmarcadas++;
            }
        }

        celdasPorPieza.Remove(pieza);

        if (mostrarLogs)
            Debug.Log($"🗺️ GridOcupacion: pieza {pieza.tipo} desmarcó {desmarcadas} celdas.");
    }

    /// <summary>
    /// Comprueba si una pieza candidata (ya posicionada) puede colocarse sin solapar
    /// ninguna celda ocupada. NO modifica el estado del grid.
    /// 
    /// IMPORTANTE: la pieza debe estar posicionada en su lugar final antes de llamar esto
    /// (es decir, después de AlinearPieza). La pieza puede estar desactivada (SetActive false).
    /// </summary>
    public bool EstaLibre(PiezaCircuito candidata, PiezaCircuito piezaAnterior = null)
    {
        // Usamos margen reducido para la comprobación de candidatas,
        // así evitamos falsos positivos en los puntos de conexión entre piezas contiguas
        float margenComprobacion = margenExtra * 0.5f;

        List<Vector2Int> celdasCandidata = ObtenerCeldasDePieza(candidata, margenComprobacion);

        // Celdas de la pieza anterior: las excluimos de la comprobación
        // porque es normal que la nueva pieza toque la anterior en el punto de conexión
        HashSet<Vector2Int> celdasExcluidas = new HashSet<Vector2Int>();
        if (piezaAnterior != null && celdasPorPieza.ContainsKey(piezaAnterior))
        {
            foreach (var c in celdasPorPieza[piezaAnterior])
                celdasExcluidas.Add(c);
        }

        foreach (var celda in celdasCandidata)
        {
            if (celdasExcluidas.Contains(celda)) continue;

            if (celdasOcupadas.Contains(celda))
            {
                if (mostrarLogs)
                    Debug.Log($"🗺️ GridOcupacion: candidata {candidata.tipo} bloqueada en celda {celda}.");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Devuelve el número de celdas actualmente ocupadas. Útil para debug.
    /// </summary>
    public int CeldasOcupadas => celdasOcupadas.Count;

    // ── Lógica interna ────────────────────────────────────────────────────────

    /// <summary>
    /// Calcula las celdas del grid que ocupa una pieza usando el Bounds
    /// combinado de todos sus Renderers, expandido por el margen indicado.
    /// 
    /// Funciona aunque la pieza esté desactivada (SetActive false), porque
    /// accedemos directamente a renderer.bounds que usa la posición world.
    /// </summary>
    private List<Vector2Int> ObtenerCeldasDePieza(PiezaCircuito pieza, float margen)
    {
        // Calcular Bounds combinado de todos los Renderers de la pieza
        Bounds boundsTotal = new Bounds();
        bool inicializado = false;

        int layerDecoracion = LayerMask.NameToLayer("Decoracion");
        Renderer[] todosRenderers = pieza.GetComponentsInChildren<Renderer>(includeInactive: false);
        Renderer[] renderers = System.Array.FindAll(todosRenderers, r => r.gameObject.layer != layerDecoracion);

        foreach (var r in renderers)
        {
            if (!inicializado)
            {
                boundsTotal = r.bounds;
                inicializado = true;
            }
            else
            {
                boundsTotal.Encapsulate(r.bounds);
            }
        }

        if (!inicializado)
        {
            // Sin renderers: fallback usando posición del transform con celda mínima
            if (mostrarLogs)
                Debug.LogWarning($"🗺️ GridOcupacion: pieza {pieza.tipo} no tiene Renderers. Usando posición como fallback.");

            return new List<Vector2Int> { PosicionACelda(pieza.transform.position) };
        }

        // Expandir el bounds con el margen
        boundsTotal.Expand(margen * 2f);

        // Convertir el bounds al espacio de celdas y marcar todas las celdas dentro
        return ObtenerCeldasEnBounds(boundsTotal);
    }

    /// <summary>
    /// Dado un Bounds en espacio mundo, devuelve todas las celdas del grid que cubre.
    /// Solo usa los ejes X y Z (plano del suelo), ignora Y.
    /// </summary>
    private List<Vector2Int> ObtenerCeldasEnBounds(Bounds bounds)
    {
        List<Vector2Int> celdas = new List<Vector2Int>();

        // Convertir esquinas del bounds a coordenadas de celda
        int xMin = Mathf.FloorToInt((bounds.min.x) / tamañoCelda);
        int xMax = Mathf.FloorToInt((bounds.max.x) / tamañoCelda);
        int zMin = Mathf.FloorToInt((bounds.min.z) / tamañoCelda);
        int zMax = Mathf.FloorToInt((bounds.max.z) / tamañoCelda);

        for (int x = xMin; x <= xMax; x++)
        {
            for (int z = zMin; z <= zMax; z++)
            {
                celdas.Add(new Vector2Int(x, z));
            }
        }

        return celdas;
    }

    /// <summary>
    /// Convierte una posición mundo a coordenada de celda.
    /// </summary>
    private Vector2Int PosicionACelda(Vector3 posicion)
    {
        return new Vector2Int(
            Mathf.FloorToInt(posicion.x / tamañoCelda),
            Mathf.FloorToInt(posicion.z / tamañoCelda)
        );
    }

    // ── Gizmos de debug ───────────────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!mostrarGizmos || celdasOcupadas.Count == 0)
            return;

        Gizmos.color = colorCeldaOcupada;

        foreach (var celda in celdasOcupadas)
        {
            // Centro de la celda en espacio mundo (Y fijo en 0)
            Vector3 centro = new Vector3(
                (celda.x + 0.5f) * tamañoCelda,
                0.1f,
                (celda.y + 0.5f) * tamañoCelda
            );

            Vector3 tamaño = new Vector3(tamañoCelda, 0.05f, tamañoCelda);
            Gizmos.DrawCube(centro, tamaño);

            // Borde de la celda
            Gizmos.color = new Color(colorCeldaOcupada.r, colorCeldaOcupada.g, colorCeldaOcupada.b, 0.8f);
            Gizmos.DrawWireCube(centro, tamaño);
            Gizmos.color = colorCeldaOcupada;
        }
    }
#endif
}