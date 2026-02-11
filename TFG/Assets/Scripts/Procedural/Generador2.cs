using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generador2 : MonoBehaviour
{
    [Header("Configuración de piezas")]
    public PiezaCircuito[] piezas;
    public Transform circuitoParent;

    [Header("Parámetros de generación por DISTANCIA")]
    public float distanciaObjetivo = 200f; // Distancia total del circuito
    public float distanciaReservadaParaCierre = 30f; // Distancia que reservamos para cerrar

    [Header("Tolerancias de cierre")]
    public float toleranciaDistancia = 1.0f;
    public float toleranciaAngulo = 15f; // grados

    [Header("Reintentos")]
    public int maxIntentosPorPieza = 15;
    public int maxReintentosCierre = 5;
    public int maxPiezasTotal = 200; // Límite de seguridad

    [Header("Reglas de variedad")]
    public int maxCurvasConsecutivas = 3;

    [Header("Debug")]
    public bool mostrarDebugGizmos = true;
    public bool mostrarLogs = true;

    // Estado interno
    private List<PiezaCircuito> piezasColocadas = new List<PiezaCircuito>();
    private Vector3 posicionInicial;
    private Vector3 direccionInicial;
    private float distanciaAcumulada = 0f;

    // Variables para comunicar resultados de corrutinas
    private bool resultadoFase1 = false;
    private bool resultadoFase2 = false;

    void Start()
    {
        StartCoroutine(GenerarCircuito());
    }

    IEnumerator GenerarCircuito()
    {
        // Validación
        if (piezas == null || piezas.Length == 0)
        {
            Debug.LogError("No hay piezas asignadas!");
            yield break;
        }

        if (distanciaObjetivo <= distanciaReservadaParaCierre)
        {
            Debug.LogError("distanciaObjetivo debe ser mayor que distanciaReservadaParaCierre");
            yield break;
        }

        // ========== FASE 1: GENERACIÓN ALEATORIA ==========
        if (mostrarLogs)
            Debug.Log("=== FASE 1: Generación aleatoria ===");

        yield return StartCoroutine(GenerarFaseAleatoria());

        if (!resultadoFase1)
        {
            Debug.LogError("❌ Falló la fase 1 de generación");
            yield break;
        }

        if (mostrarLogs)
            Debug.Log($"Fase 1 completada: {piezasColocadas.Count} piezas, " +
                     $"Distancia: {distanciaAcumulada:F1}m / {distanciaObjetivo:F1}m");

        // ========== FASE 2: CIERRE DEL CIRCUITO ==========
        if (mostrarLogs)
            Debug.Log("=== FASE 2: Cierre del circuito ===");

        bool circuitoCerrado = false;
        int intentosCierre = 0;

        while (!circuitoCerrado && intentosCierre < maxReintentosCierre)
        {
            yield return StartCoroutine(IntentarCerrarCircuito());
            circuitoCerrado = resultadoFase2;

            if (!circuitoCerrado)
            {
                intentosCierre++;
                if (mostrarLogs)
                    Debug.LogWarning($"Intento de cierre {intentosCierre} falló. Haciendo backtracking...");

                // Backtracking: elimina piezas hasta volver a la distancia de cierre
                HacerBacktracking(distanciaReservadaParaCierre);

                yield return null;
            }
        }

        // ========== RESULTADO FINAL ==========
        if (circuitoCerrado)
        {
            Debug.Log($"✅ ¡CIRCUITO CERRADO EXITOSAMENTE!\n" +
                     $"   Piezas: {piezasColocadas.Count}\n" +
                     $"   Distancia total: {distanciaAcumulada:F1}m\n" +
                     $"   Objetivo: {distanciaObjetivo:F1}m");
        }
        else
        {
            Debug.LogError($"❌ No se pudo cerrar el circuito después de {maxReintentosCierre} intentos\n" +
                          $"   Distancia alcanzada: {distanciaAcumulada:F1}m / {distanciaObjetivo:F1}m");
        }

        LimpiarPiezasDesactivadas();
    }

    /// <summary>
    /// FASE 1: Genera piezas aleatoriamente hasta dejar espacio para el cierre
    /// </summary>
    IEnumerator GenerarFaseAleatoria()
    {
        // Primera pieza (siempre recta si existe)
        PiezaCircuito primeraPieza = InstanciarPieza(ObtenerPiezaRecta() ?? piezas[0]);
        primeraPieza.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        piezasColocadas.Add(primeraPieza);

        // Guardamos inicio para referencia en fase 2
        posicionInicial = primeraPieza.puntoEntrada.position;
        direccionInicial = primeraPieza.puntoEntrada.right;

        // Acumular distancia de la primera pieza
        distanciaAcumulada = primeraPieza.longitud;

        // Distancia objetivo de la fase 1 (dejamos espacio para cierre)
        float distanciaFase1 = distanciaObjetivo - distanciaReservadaParaCierre;

        int piezasGeneradas = 1;
        int operacionesPorFrame = 10;
        int ops = 0;

        // Generar piezas hasta alcanzar la distancia objetivo (menos margen para cierre)
        while (distanciaAcumulada < distanciaFase1 && piezasGeneradas < maxPiezasTotal)
        {
            bool colocada = false;
            int intentos = 0;

            while (!colocada && intentos < maxIntentosPorPieza)
            {
                intentos++;

                // Elegir pieza respetando reglas de variedad
                PiezaCircuito prefab = ElegirPiezaAleatoria();
                PiezaCircuito nuevaPieza = InstanciarPieza(prefab);
                nuevaPieza.gameObject.SetActive(false);

                // Alinear con la última pieza colocada
                AlinearPieza(nuevaPieza, piezasColocadas[piezasColocadas.Count - 1]);

                // Verificar si se puede colocar sin colisiones
                if (PuedeColocar(nuevaPieza))
                {
                    nuevaPieza.gameObject.SetActive(true);
                    piezasColocadas.Add(nuevaPieza);
                    distanciaAcumulada += nuevaPieza.longitud;
                    piezasGeneradas++;
                    colocada = true;

                    if (mostrarLogs && piezasGeneradas % 10 == 0)
                    {
                        Debug.Log($"Progreso: {piezasGeneradas} piezas, " +
                                 $"{distanciaAcumulada:F1}m / {distanciaFase1:F1}m");
                    }
                }
                else
                {
                    Destroy(nuevaPieza.gameObject);
                }
            }

            if (!colocada)
            {
                Debug.LogWarning($"No se pudo colocar pieza después de {maxIntentosPorPieza} intentos. " +
                                $"Distancia alcanzada: {distanciaAcumulada:F1}m");
                yield return false;
            }

            // Yield cada pocas operaciones para no bloquear
            ops++;
            if (ops >= operacionesPorFrame)
            {
                ops = 0;
                yield return null;
            }
        }

        // Verificar si alcanzamos el límite de piezas
        if (piezasGeneradas >= maxPiezasTotal)
        {
            Debug.LogWarning($"Se alcanzó el límite de {maxPiezasTotal} piezas");
        }

        yield return true;
    }

    /// <summary>
    /// FASE 2: Intenta cerrar el circuito estratégicamente
    /// </summary>
    IEnumerator IntentarCerrarCircuito()
    {
        float distanciaRestante = distanciaObjetivo - distanciaAcumulada;
        int piezasIntentadas = 0;

        if (mostrarLogs)
        {
            Debug.Log($"Intentando cerrar. Distancia restante: {distanciaRestante:F1}m, " +
                     $"Gap lineal al inicio: {Vector3.Distance(piezasColocadas[^1].puntoSalida.position, posicionInicial):F1}m");
        }

        // Seguir añadiendo piezas hasta que el circuito cierre
        while (distanciaAcumulada < distanciaObjetivo && piezasIntentadas < maxPiezasTotal)
        {
            piezasIntentadas++;
            bool colocada = false;
            int intentos = 0;

            // Calcular si estamos muy cerca del objetivo (últimas piezas)
            float distanciaRestanteActual = distanciaObjetivo - distanciaAcumulada;
            bool estamosMuyCerca = distanciaRestanteActual < distanciaReservadaParaCierre * 0.3f;

            while (!colocada && intentos < maxIntentosPorPieza)
            {
                intentos++;

                // Elegir pieza que mejor se acerque al cierre
                PiezaCircuito prefab = ElegirPiezaParaCierre(estamosMuyCerca);

                if (prefab == null)
                {
                    Debug.LogWarning("No hay piezas disponibles para cerrar");
                    yield return false;
                }

                PiezaCircuito nuevaPieza = InstanciarPieza(prefab);
                nuevaPieza.gameObject.SetActive(false);

                AlinearPieza(nuevaPieza, piezasColocadas[piezasColocadas.Count - 1]);

                bool puedeColocar = PuedeColocar(nuevaPieza);

                // Si vamos a pasarnos de distancia con esta pieza, comprobar si cierra
                bool vamosAPasarnos = (distanciaAcumulada + nuevaPieza.longitud) > distanciaObjetivo;
                bool cierra = vamosAPasarnos ? CircuitoCierra(nuevaPieza) : true;

                if (puedeColocar && cierra)
                {
                    nuevaPieza.gameObject.SetActive(true);
                    piezasColocadas.Add(nuevaPieza);
                    distanciaAcumulada += nuevaPieza.longitud;
                    colocada = true;

                    // Si el circuito cerró, ¡éxito!
                    if (CircuitoCierra(nuevaPieza))
                    {
                        if (mostrarLogs)
                            Debug.Log($"¡Cierre detectado! Distancia final: {distanciaAcumulada:F1}m");
                        yield return true;
                    }

                    if (mostrarLogs)
                    {
                        float gapActual = Vector3.Distance(nuevaPieza.puntoSalida.position, posicionInicial);
                        Debug.Log($"Pieza de cierre añadida. Gap restante: {gapActual:F1}m, " +
                                 $"Distancia: {distanciaAcumulada:F1}m / {distanciaObjetivo:F1}m");
                    }
                }
                else
                {
                    Destroy(nuevaPieza.gameObject);
                }
            }

            if (!colocada)
            {
                Debug.LogWarning($"No se pudo colocar pieza de cierre después de {maxIntentosPorPieza} intentos");
                yield return false;
            }

            yield return null;
        }

        // Si llegamos aquí, no cerramos
        yield return false;
    }

    /// <summary>
    /// Elige una pieza que se acerque al punto inicial
    /// </summary>
    PiezaCircuito ElegirPiezaParaCierre(bool debeSerPerfecta)
    {
        PiezaCircuito ultimaColocada = piezasColocadas[piezasColocadas.Count - 1];
        PiezaCircuito mejorPieza = null;
        float mejorPuntuacion = float.MaxValue;

        // Calcular gap actual
        float gapActual = Vector3.Distance(ultimaColocada.puntoSalida.position, posicionInicial);

        foreach (var prefab in piezas)
        {
            // Crear instancia temporal para evaluar
            PiezaCircuito temp = InstanciarPieza(prefab);
            temp.gameObject.SetActive(false);

            AlinearPieza(temp, ultimaColocada);

            // Calcular qué tan cerca estaría del inicio
            float distancia = Vector3.Distance(temp.puntoSalida.position, posicionInicial);
            float angulo = Vector3.Angle(temp.puntoSalida.right, direccionInicial);

            // Puntuación: prioriza reducir distancia y alinear ángulo
            // Penaliza piezas que nos alejan del objetivo
            float puntuacion = distancia + (angulo * 0.1f);

            if (distancia > gapActual)
            {
                puntuacion += 100f; // Gran penalización si nos alejamos
            }

            // Si debe ser perfecta (última pieza), verificar tolerancias
            if (debeSerPerfecta)
            {
                if (distancia > toleranciaDistancia || angulo > toleranciaAngulo)
                {
                    Destroy(temp.gameObject);
                    continue; // No es válida para cerrar
                }
            }

            if (puntuacion < mejorPuntuacion)
            {
                mejorPuntuacion = puntuacion;
                mejorPieza = prefab;
            }

            Destroy(temp.gameObject);
        }

        // Si no encontramos pieza perfecta cuando se requiere, intentar con una aleatoria
        if (mejorPieza == null && !debeSerPerfecta)
        {
            mejorPieza = ElegirPiezaAleatoria();
        }

        return mejorPieza;
    }

    /// <summary>
    /// Verifica si el circuito se cierra correctamente
    /// </summary>
    bool CircuitoCierra(PiezaCircuito ultimaPieza)
    {
        float distancia = Vector3.Distance(ultimaPieza.puntoSalida.position, posicionInicial);
        float angulo = Vector3.Angle(ultimaPieza.puntoSalida.right, direccionInicial);

        bool cierra = distancia <= toleranciaDistancia && angulo <= toleranciaAngulo;

        if (mostrarDebugGizmos && mostrarLogs)
        {
            Debug.Log($"Comprobación cierre: Dist={distancia:F2}m (max {toleranciaDistancia}), " +
                     $"Ángulo={angulo:F2}° (max {toleranciaAngulo}°) -> {(cierra ? "✅" : "❌")}");
        }

        return cierra;
    }

    /// <summary>
    /// Elige pieza aleatoria respetando reglas de variedad
    /// </summary>
    PiezaCircuito ElegirPiezaAleatoria()
    {
        // Contar curvas consecutivas
        int curvasConsecutivas = 0;
        PiezaCircuito.TipoPieza ultimoTipo = PiezaCircuito.TipoPieza.Recta;

        if (piezasColocadas.Count > 0)
        {
            ultimoTipo = piezasColocadas[piezasColocadas.Count - 1].tipo;

            if (ultimoTipo != PiezaCircuito.TipoPieza.Recta)
            {
                for (int i = piezasColocadas.Count - 1; i >= 0; i--)
                {
                    if (piezasColocadas[i].tipo == ultimoTipo)
                        curvasConsecutivas++;
                    else
                        break;
                }
            }
        }

        // Filtrar piezas disponibles
        List<PiezaCircuito> disponibles = new List<PiezaCircuito>();

        foreach (var pieza in piezas)
        {
            // Si hay demasiadas curvas del mismo tipo, evitarlas
            if (curvasConsecutivas >= maxCurvasConsecutivas && pieza.tipo == ultimoTipo)
                continue;

            disponibles.Add(pieza);
        }

        if (disponibles.Count == 0)
            disponibles.AddRange(piezas); // Fallback

        return disponibles[Random.Range(0, disponibles.Count)];
    }

    /// <summary>
    /// Alinea una pieza nueva con la anterior
    /// </summary>
    void AlinearPieza(PiezaCircuito nueva, PiezaCircuito anterior)
    {
        // Calcular ángulo de rotación necesario
        Vector3 dirAnterior = anterior.puntoSalida.right;
        Vector3 dirNueva = nueva.puntoEntrada.right;

        float angulo = Vector3.SignedAngle(dirNueva, dirAnterior, Vector3.up);
        nueva.transform.Rotate(Vector3.up, angulo, Space.World);

        // Posicionar: conectar puntoSalida anterior con puntoEntrada nueva
        Vector3 offset = anterior.puntoSalida.position - nueva.puntoEntrada.position;
        nueva.transform.position += offset;
    }

    /// <summary>
    /// Verifica si una pieza se puede colocar sin colisiones
    /// </summary>
    bool PuedeColocar(PiezaCircuito pieza)
    {
        Collider[] nuevosColliders = pieza.GetComponentsInChildren<Collider>();
        Collider[] collidersExistentes = circuitoParent.GetComponentsInChildren<Collider>();

        foreach (var colA in nuevosColliders)
        {
            foreach (var colB in collidersExistentes)
            {
                // Ignorar si es el mismo transform
                if (colA.transform == colB.transform)
                    continue;

                // Comprobar penetración
                if (Physics.ComputePenetration(
                    colA, colA.transform.position, colA.transform.rotation,
                    colB, colB.transform.position, colB.transform.rotation,
                    out _, out _))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Elimina piezas hasta volver a tener X distancia restante para cerrar
    /// </summary>
    void HacerBacktracking(float distanciaALiberar)
    {
        float distanciaEliminada = 0f;
        int piezasEliminadas = 0;

        // Eliminar piezas hasta liberar la distancia necesaria
        while (distanciaEliminada < distanciaALiberar && piezasColocadas.Count > 1)
        {
            int idx = piezasColocadas.Count - 1;
            PiezaCircuito pieza = piezasColocadas[idx];

            distanciaEliminada += pieza.longitud;
            distanciaAcumulada -= pieza.longitud;

            Destroy(pieza.gameObject);
            piezasColocadas.RemoveAt(idx);
            piezasEliminadas++;
        }

        if (mostrarLogs)
        {
            Debug.Log($"Backtracking: eliminadas {piezasEliminadas} piezas, " +
                     $"liberados {distanciaEliminada:F1}m. " +
                     $"Nueva distancia: {distanciaAcumulada:F1}m");
        }
    }

    /// <summary>
    /// Instancia una pieza y la añade al parent
    /// </summary>
    PiezaCircuito InstanciarPieza(PiezaCircuito prefab)
    {
        return Instantiate(prefab, circuitoParent);
    }

    /// <summary>
    /// Obtiene la primera pieza recta disponible
    /// </summary>
    PiezaCircuito ObtenerPiezaRecta()
    {
        foreach (var pieza in piezas)
        {
            if (pieza.tipo == PiezaCircuito.TipoPieza.Recta)
                return pieza;
        }
        return null;
    }

    /// <summary>
    /// Limpia piezas desactivadas que quedaron de intentos fallidos
    /// </summary>
    void LimpiarPiezasDesactivadas()
    {
        for (int i = circuitoParent.childCount - 1; i >= 0; i--)
        {
            Transform child = circuitoParent.GetChild(i);
            if (!child.gameObject.activeSelf)
                Destroy(child.gameObject);
        }
    }

    // Gizmos para visualizar en editor
    void OnDrawGizmos()
    {
        if (!mostrarDebugGizmos || piezasColocadas.Count == 0)
            return;

        // Dibujar punto inicial
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(posicionInicial, 0.5f);
        Gizmos.DrawRay(posicionInicial, direccionInicial * 2f);

        // Dibujar última pieza y gap
        if (piezasColocadas.Count > 0)
        {
            PiezaCircuito ultima = piezasColocadas[piezasColocadas.Count - 1];
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ultima.puntoSalida.position, 0.5f);
            Gizmos.DrawRay(ultima.puntoSalida.position, ultima.puntoSalida.right * 2f);

            // Línea entre salida y entrada (gap)
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(ultima.puntoSalida.position, posicionInicial);

            // Texto con distancia
            float gap = Vector3.Distance(ultima.puntoSalida.position, posicionInicial);
            UnityEngine.GUIStyle style = new UnityEngine.GUIStyle();
            style.normal.textColor = Color.white;

#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                (ultima.puntoSalida.position + posicionInicial) / 2f,
                $"Gap: {gap:F1}m\nDist: {distanciaAcumulada:F1}m / {distanciaObjetivo:F1}m",
                style
            );
#endif
        }
    }
}
