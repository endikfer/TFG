using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camaraConjunto : MonoBehaviour
{
    public List<Camera> camarass;

    private void Awake()
    {
        camarass = new List<Camera>(GetComponentsInChildren<Camera>());
        Debug.Log(camarass.Count);
    }
}
