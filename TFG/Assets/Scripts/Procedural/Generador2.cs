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
    [Range(10f, 40f)]
    public float porcentajeReservadoParaCierre = 20f; // Porcentaje de la distancia total reservado para cerrar

    private float distanciaReservadaParaCierre; // Se calcula automáticamente

    [Header("Tolerancias de cierre")]
    public float toleranciaDistancia = 1.0f;
    public float toleranciaAngulo = 15f; // grados

    [Header("Reintentos")]
    public int maxIntentosPorPieza = 15;
    public int maxReintentosCierre = 5;
    public int maxPiezasTotal = 200; // Límite de seguridad
    public int maxReintentosGlobales = 5; // Reintentos completos desde cero

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
    private bool llegaFase2 = false; // Indica si realmente se llegó a la Fase 2

    // Sistema de memoria de intentos fallidos
    private HashSet<string> combinacionesIntentadas = new HashSet<string>();
    private int intentosCierreSinProgreso = 0;
    private const int maxIntentosSinProgreso = 20;

    void Start()
    {
        StartCoroutine(GenerarCircuitoConReintentos());
    }

    IEnumerator GenerarCircuitoConReintentos()
    {
        int intentoGlobal = 0;
        bool exito = false;

        while (!exito && intentoGlobal < maxReintentosGlobales)
        {
            if (intentoGlobal > 0)
            {
                Debug.Log($"\n{'=',-60}\nREINTENTO GLOBAL {intentoGlobal + 1}/{maxReintentosGlobales}\n{'=',-60}");
                LimpiarTodo();
            }

            yield return StartCoroutine(GenerarCircuito());

            // Verificar si tuvo éxito
            if (piezasColocadas.Count > 0 && CircuitoCierra(piezasColocadas[piezasColocadas.Count - 1]))
            {
                intentoGlobal++;
                exito = true;
                Debug.Log($"✅ ¡ÉXITO EN INTENTO GLOBAL {intentoGlobal}!");
            }
            else if (piezasColocadas.Count > 0 && llegaFase2)
            {
                // Solo cuenta como intento si llegó a la Fase 2
                intentoGlobal++;
                if (intentoGlobal < maxReintentosGlobales)
                {
                    Debug.LogWarning($"❌ Intento global {intentoGlobal} falló. Reiniciando desde cero...");
                    yield return new WaitForSeconds(0.5f); // Pequeña pausa
                }
            }
            else
            {
                // No llegó a Fase 2, no cuenta como intento
                Debug.LogWarning("⚠️ No llegó a Fase 2 (circuito inviable desde Fase 1). Reintentando sin contar como intento...");
                yield return new WaitForSeconds(0.5f);
            }
        }

        if (!exito)
        {
            Debug.LogError($"❌❌❌ FALLO TOTAL después de {intentoGlobal} intentos globales válidos");
        }
    }

    void LimpiarTodo()
    {
        // Destruir todas las piezas
        foreach (var pieza in piezasColocadas)
        {
            if (pieza != null)
                Destroy(pieza.gameObject);
        }

        // Limpiar cualquier pieza desactivada
        for (int i = circuitoParent.childCount - 1; i >= 0; i--)
        {
            Destroy(circuitoParent.GetChild(i).gameObject);
        }

        // Resetear estado
        piezasColocadas.Clear();
        distanciaAcumulada = 0f;
        combinacionesIntentadas.Clear();
        intentosCierreSinProgreso = 0;
        llegaFase2 = false;

        Debug.Log("🧹 Estado limpiado para nuevo intento");
    }

    IEnumerator GenerarCircuito()
    {
        // Resetear flag de Fase 2
        llegaFase2 = false;

        // Calcular distancia de cierre basada en el porcentaje
        CalcularDistanciaDeCarrera();

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

        // ========== VALIDACIÓN DE VIABILIDAD ==========
        float gapLineal = Vector3.Distance(piezasColocadas[^1].puntoSalida.position, posicionInicial);
        float distanciaRestante = distanciaObjetivo - distanciaAcumulada;

        if (mostrarLogs)
        {
            Debug.Log($"=== VALIDACIÓN DE VIABILIDAD ===");
            Debug.Log($"Gap lineal al inicio: {gapLineal:F1}m");
            Debug.Log($"Distancia restante por generar: {distanciaRestante:F1}m");
        }

        if (gapLineal > distanciaRestante)
        {
            Debug.LogWarning($"⚠️ CIRCUITO INVIABLE: Gap ({gapLineal:F1}m) > Distancia restante ({distanciaRestante:F1}m)");
            Debug.LogWarning("No es posible cerrar el circuito con la distancia disponible. Reiniciando...");
            yield break; // Salir y reintentar desde cero (no marca llegaFase2 = true)
        }

        if (mostrarLogs)
            Debug.Log($"✅ Circuito viable. Gap/Distancia: {(gapLineal / distanciaRestante * 100f):F1}%");

        // ========== FASE 2: CIERRE DEL CIRCUITO ==========
        llegaFase2 = true; // Marcar que sí llegamos a Fase 2

        if (mostrarLogs)
            Debug.Log("=== FASE 2: Cierre del circuito ===");

        bool circuitoCerrado = false;
        int intentosCierre = 0;

        while (!circuitoCerrado && intentosCierre < maxReintentosCierre)
        {
            // Resetear memoria de combinaciones para cada intento de cierre
            combinacionesIntentadas.Clear();
            intentosCierreSinProgreso = 0;

            yield return StartCoroutine(IntentarCerrarCircuito());
            circuitoCerrado = resultadoFase2;

            if (!circuitoCerrado)
            {
                intentosCierre++;
                if (mostrarLogs)
                    Debug.LogWarning($"Intento de cierre {intentosCierre}/{maxReintentosCierre} falló. Haciendo backtracking...");

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
    /// Calcula la distancia reservada para el cierre basándose en el porcentaje configurado
    /// </summary>
    void CalcularDistanciaDeCarrera()
    {
        distanciaReservadaParaCierre = distanciaObjetivo * (porcentajeReservadoParaCierre / 100f);

        if (mostrarLogs)
        {
            Debug.Log($"📊 Configuración de distancias:");
            Debug.Log($"   Distancia total objetivo: {distanciaObjetivo:F1}m");
            Debug.Log($"   Porcentaje para cierre: {porcentajeReservadoParaCierre:F1}%");
            Debug.Log($"   Distancia reservada para cierre: {distanciaReservadaParaCierre:F1}m");
            Debug.Log($"   Distancia fase aleatoria: {(distanciaObjetivo - distanciaReservadaParaCierre):F1}m");
        }
    }

    /// <summary>
    /// FASE 1: Genera piezas aleatoriamente hasta dejar espacio para el cierre
    /// </summary>
    IEnumerator GenerarFaseAleatoria()
    {
        resultadoFase1 = false; // Inicializar resultado

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
                resultadoFase1 = false;
                yield break;
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

        resultadoFase1 = true;
    }

    /// <summary>
    /// FASE 2: Intenta cerrar el circuito estratégicamente
    /// </summary>
    IEnumerator IntentarCerrarCircuito()
    {
        resultadoFase2 = false; // Inicializar resultado

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

            // VALIDACIÓN CONTINUA: Verificar si todavía es posible cerrar
            float gapActualTotal = Vector3.Distance(piezasColocadas[^1].puntoSalida.position, posicionInicial);
            float distanciaRestanteActual = distanciaObjetivo - distanciaAcumulada;

            if (gapActualTotal > distanciaRestanteActual * 1.5f) // Margen del 50%
            {
                if (mostrarLogs)
                    Debug.LogWarning($"⚠️ Gap ({gapActualTotal:F1}m) demasiado grande para distancia restante ({distanciaRestanteActual:F1}m). Abortando cierre.");
                resultadoFase2 = false;
                yield break;
            }

            bool colocada = false;
            int intentos = 0;

            // Lista de piezas que ya probamos en ESTA posición específica
            HashSet<int> piezasProbadasAqui = new HashSet<int>();

            // Calcular si estamos muy cerca del objetivo (últimas piezas)
            bool estamosMuyCerca = distanciaRestanteActual < distanciaReservadaParaCierre * 0.3f;

            while (!colocada && intentos < maxIntentosPorPieza)
            {
                intentos++;

                // Elegir pieza que mejor se acerque al cierre, EVITANDO las ya probadas
                PiezaCircuito prefab = ElegirPiezaParaCierre(estamosMuyCerca, piezasProbadasAqui);

                if (prefab == null)
                {
                    if (mostrarLogs)
                        Debug.LogWarning($"No hay más piezas disponibles para probar en esta posición (probadas: {piezasProbadasAqui.Count}/{piezas.Length})");
                    resultadoFase2 = false;
                    yield break;
                }

                // Marcar que probamos esta pieza en esta posición
                int indicePrefab = System.Array.IndexOf(piezas, prefab);
                piezasProbadasAqui.Add(indicePrefab);

                // Generar clave única para esta combinación (últimas 3 piezas + nueva)
                string claveCombinacion = GenerarClaveCombinacion(prefab);

                // Si ya probamos esta combinación exacta antes, saltarla
                if (combinacionesIntentadas.Contains(claveCombinacion))
                {
                    if (mostrarLogs)
                        Debug.Log($"Combinación ya intentada, saltando...");
                    continue;
                }

                combinacionesIntentadas.Add(claveCombinacion);

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
                    intentosCierreSinProgreso = 0; // Resetear contador de sin progreso

                    // Si el circuito cerró, ¡éxito!
                    if (CircuitoCierra(nuevaPieza))
                    {
                        if (mostrarLogs)
                            Debug.Log($"¡Cierre detectado! Distancia final: {distanciaAcumulada:F1}m");
                        resultadoFase2 = true;
                        yield break;
                    }

                    if (mostrarLogs)
                    {
                        float gapActual = Vector3.Distance(nuevaPieza.puntoSalida.position, posicionInicial);
                        Debug.Log($"Pieza de cierre añadida: {nuevaPieza.tipo}. Gap: {gapActual:F1}m, " +
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
                intentosCierreSinProgreso++;

                if (mostrarLogs)
                    Debug.LogWarning($"No se pudo colocar pieza de cierre después de {maxIntentosPorPieza} intentos. " +
                                   $"Sin progreso: {intentosCierreSinProgreso}/{maxIntentosSinProgreso}");

                // Si llevamos muchos intentos sin colocar ninguna pieza, abandonar este intento
                if (intentosCierreSinProgreso >= maxIntentosSinProgreso)
                {
                    Debug.LogWarning("Demasiados intentos sin progreso, haciendo backtracking...");
                    resultadoFase2 = false;
                    yield break;
                }

                resultadoFase2 = false;
                yield break;
            }

            yield return null;
        }

        // Si llegamos aquí, no cerramos
        resultadoFase2 = false;
    }

    /// <summary>
    /// Elige una pieza que se acerque al punto inicial, evitando las ya probadas
    /// </summary>
    PiezaCircuito ElegirPiezaParaCierre(bool debeSerPerfecta, HashSet<int> piezasYaProbadas)
    {
        PiezaCircuito ultimaColocada = piezasColocadas[piezasColocadas.Count - 1];

        // Calcular gap actual
        float gapActual = Vector3.Distance(ultimaColocada.puntoSalida.position, posicionInicial);

        // Crear lista de candidatos con sus puntuaciones
        List<(PiezaCircuito pieza, float puntuacion, int indice)> candidatos =
            new List<(PiezaCircuito, float, int)>();

        for (int i = 0; i < piezas.Length; i++)
        {
            // Saltar piezas ya probadas en esta posición
            if (piezasYaProbadas.Contains(i))
                continue;

            var prefab = piezas[i];

            // Crear instancia temporal para evaluar
            PiezaCircuito temp = InstanciarPieza(prefab);
            temp.gameObject.SetActive(false);

            AlinearPieza(temp, ultimaColocada);

            // Calcular qué tan cerca estaría del inicio
            float distancia = Vector3.Distance(temp.puntoSalida.position, posicionInicial);
            float angulo = Vector3.Angle(temp.puntoSalida.right, direccionInicial);

            // Puntuación: prioriza reducir distancia y alinear ángulo
            float puntuacion = distancia + (angulo * 0.1f);

            // Penaliza piezas que nos alejan del objetivo
            if (distancia > gapActual)
            {
                puntuacion += 50f;
            }

            // Bonificación por variedad: favorece piezas de tipo diferente a las últimas
            if (piezasColocadas.Count >= 2)
            {
                var ultimoTipo = piezasColocadas[piezasColocadas.Count - 1].tipo;
                var penultimoTipo = piezasColocadas[piezasColocadas.Count - 2].tipo;

                if (prefab.tipo != ultimoTipo)
                    puntuacion -= 5f; // Bonificación por variedad

                if (prefab.tipo == ultimoTipo && prefab.tipo == penultimoTipo)
                    puntuacion += 10f; // Penalización por repetición
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

            candidatos.Add((prefab, puntuacion, i));
            Destroy(temp.gameObject);
        }

        if (candidatos.Count == 0)
        {
            // No quedan piezas válidas
            return null;
        }

        // Ordenar por puntuación (menor es mejor)
        candidatos.Sort((a, b) => a.puntuacion.CompareTo(b.puntuacion));

        // ESTRATEGIA MEJORADA: En vez de elegir siempre la mejor,
        // elegimos aleatoriamente entre las top 3 mejores para añadir variedad
        int topN = Mathf.Min(3, candidatos.Count);
        int indiceAleatorio = Random.Range(0, topN);

        var elegida = candidatos[indiceAleatorio];

        if (mostrarLogs)
        {
            Debug.Log($"Elegida pieza {elegida.pieza.tipo} (puntuación: {elegida.puntuacion:F1}, " +
                     $"ranking: {indiceAleatorio + 1}/{topN})");
        }

        return elegida.pieza;
    }

    /// <summary>
    /// Genera una clave única basada en las últimas piezas colocadas + la nueva
    /// </summary>
    string GenerarClaveCombinacion(PiezaCircuito nuevaPieza)
    {
        // Usamos las últimas 3 piezas para generar la clave
        int profundidad = Mathf.Min(3, piezasColocadas.Count);
        string clave = "";

        for (int i = piezasColocadas.Count - profundidad; i < piezasColocadas.Count; i++)
        {
            clave += piezasColocadas[i].tipo.ToString() + "-";
        }

        clave += nuevaPieza.tipo.ToString();

        return clave;
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