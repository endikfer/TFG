using System.Collections.Generic;
using UnityEngine;

public class CheckPoints3 : MonoBehaviour
{
    public List<CheckPoint3> checkPoints;

    private void Awake()
    {
        checkPoints = new List<CheckPoint3>(GetComponentsInChildren<CheckPoint3>());
    }
}
