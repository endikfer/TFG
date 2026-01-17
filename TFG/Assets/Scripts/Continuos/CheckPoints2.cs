using System.Collections.Generic;
using UnityEngine;

public class CheckPoints2 : MonoBehaviour
{
    public List<CheckPoint2> checkPoints;

    private void Awake()
    {
        checkPoints = new List<CheckPoint2>(GetComponentsInChildren<CheckPoint2>());
    }
}
