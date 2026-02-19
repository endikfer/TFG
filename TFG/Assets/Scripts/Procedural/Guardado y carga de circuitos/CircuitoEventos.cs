using System;
using System.Collections.Generic;

/// <summary>
/// Sistema de eventos estático para comunicar el generador con otros sistemas.
/// No hereda de MonoBehaviour, es accesible desde cualquier script.
/// </summary>
public static class CircuitoEventos
{
    /// <summary>
    /// Se dispara cuando el generador logra cerrar un circuito correctamente.
    /// Devuelve la lista ordenada de piezas colocadas y la distancia total.
    /// </summary>
    public static event Action<List<PiezaCircuito>, float> OnCircuitoCerrado;

    public static event Action<List<PiezaCircuito>, float> OnCircuitoCargado;

    /// <summary>
    /// Llamado por el Generador2 al confirmar el cierre del circuito.
    /// </summary>
    public static void NotificarCircuitoCerrado(List<PiezaCircuito> piezas, float distanciaTotal)
    {
        OnCircuitoCerrado?.Invoke(piezas, distanciaTotal);
    }

    public static void NotificarCircuitoCargado(List<PiezaCircuito> piezas, float distanciaTotal)
    {
        OnCircuitoCargado?.Invoke(piezas, distanciaTotal);
    }
}
