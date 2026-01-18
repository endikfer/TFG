using System.Collections.Generic;
using UnityEngine;

public class CheckPointsM : MonoBehaviour
{
    public List<CheckPointM> checkPoints;

    private void Awake()
    {
        checkPoints = new List<CheckPointM>(GetComponentsInChildren<CheckPointM>());
    }
}
