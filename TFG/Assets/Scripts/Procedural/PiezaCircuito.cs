using UnityEngine;

public class PiezaCircuito : MonoBehaviour
{
    public enum TipoPieza { Recta, CurvaIzquierda, CurvaDerecha, Inicio }
    public TipoPieza tipo;

    [Header("Puntos de la pieza")]
    public Transform puntoEntrada;
    public Transform puntoSalida;

    [Header("Puntos intermedios para curvas")]
    public Transform[] puntosCurva;

    [Header("Longitud calculada")]
    public float longitud;

    void Awake()
    {
        CalcularLongitud();
    }

    /// <summary>
    /// Calcula la longitud total de la pieza sumando distancias entre los puntos de la curva.
    /// Si no hay puntos intermedios, usa la distancia lineal entre entrada y salida.
    /// </summary>
    public void CalcularLongitud()
    {
        longitud = 0f;

        if (puntosCurva != null && puntosCurva.Length > 1)
        {
            // suma la distancia entre cada par de puntos consecutivos
            for (int i = 0; i < puntosCurva.Length - 1; i++)
            {
                longitud += Vector3.Distance(puntosCurva[i].position, puntosCurva[i + 1].position);
            }
        }
        else
        {
            // si no hay puntos intermedios, usa distancia lineal
            longitud = Vector3.Distance(puntoEntrada.position, puntoSalida.position);
        }
    }
}
