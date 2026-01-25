using UnityEngine;

public class PiezaCircuito : MonoBehaviour
{
    public enum TipoPieza { Recta, CurvaIzquierda, CurvaDerecha }
    public TipoPieza tipo;

    public Transform puntoEntrada;
    public Transform puntoSalida;
}
