using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpointss : MonoBehaviour
{
    public List<check1> checkPoints;
    
    private void Awake()
    {
        checkPoints = new List<check1>(GetComponentsInChildren<check1>());
        Debug.Log(checkPoints.Count);
    }
}
