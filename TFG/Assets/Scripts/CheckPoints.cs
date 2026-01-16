using System.Collections.Generic;
using UnityEngine;

public class CheckPoints : MonoBehaviour
{
    public List<CheckPoint> checkPoints;

    private void Awake()
    {
        checkPoints = new List<CheckPoint>(GetComponentsInChildren<CheckPoint>());
        Debug.Log(checkPoints.Count);
    }
}
