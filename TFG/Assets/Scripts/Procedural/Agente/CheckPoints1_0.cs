using System.Collections.Generic;
using UnityEngine;

public class CheckPoints1_0 : MonoBehaviour
{
    // Ya NO se rellena en Awake desde los hijos, la lista la construye
    // CheckPointsManager1_0 cuando llega el evento OnCircuitoListoParaAgente
    public List<CheckPoint1_0> checkPoints = new List<CheckPoint1_0>();
}