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
    /// Se dispara cuando el circuito está listo para el entrenamiento.
    /// Devuelve la lista ordenada de piezas para que el Manager construya los checkpoints.
    /// </summary>
    public static event Action<List<PiezaCircuito>> OnCircuitoListoParaAgente;

    public static void NotificarCircuitoCerrado(List<PiezaCircuito> piezas, float distanciaTotal)
    {
        OnCircuitoCerrado?.Invoke(piezas, distanciaTotal);

        // Tras cerrar, avisar también al sistema de checkpoints/agente
        NotificarCircuitoListoParaAgente(piezas);
    }

    public static void NotificarCircuitoCargado(List<PiezaCircuito> piezas, float distanciaTotal)
    {
        OnCircuitoCargado?.Invoke(piezas, distanciaTotal);
        NotificarCircuitoListoParaAgente(piezas);
    }

    /// <summary>
    /// Llamado por el Generador2 tras cerrar el circuito, para avisar al CheckPointsManager.
    /// </summary>
    public static void NotificarCircuitoListoParaAgente(List<PiezaCircuito> piezas)
    {
        OnCircuitoListoParaAgente?.Invoke(piezas);
    }
}