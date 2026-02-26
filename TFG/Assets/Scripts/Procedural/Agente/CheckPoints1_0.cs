using System.Collections.Generic;
using UnityEngine;

public class CheckPoints1_0 : MonoBehaviour
{
    public List<CheckPoint1_0> checkPoints;

    private void Awake()
    {
        checkPoints = new List<CheckPoint1_0>(GetComponentsInChildren<CheckPoint1_0>());
    }
}
