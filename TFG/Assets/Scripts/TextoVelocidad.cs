using TMPro;
using UnityEngine;

public class TextoVelocidad : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarController carController;
    [SerializeField] private TMP_Text speedText;

    [Header("Display")]
    [SerializeField] private string prefix = "Velocidad";
    [SerializeField] private string unit = "km/h";

    void Update()
    {
        if (carController == null || speedText == null)
            return;

        float speed = Mathf.Abs(carController.carSpeed);
        speedText.text = $"{prefix}: {speed:F0} {unit}";
    }
}
