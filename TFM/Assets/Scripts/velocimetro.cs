using UnityEngine;
using UnityEngine.UI;

public class velocimetro : MonoBehaviour
{
    private Text textoVelocimetro;
    private PrometeoCarController prometeoC;

    void Start()
    {
        //private AgentToDrive kartAgent;
        prometeoC = FindObjectOfType<PrometeoCarController>();
        textoVelocimetro = GetComponent<Text>();
    }

    void Update()
    {
        float velocidadActual = prometeoC.carSpeed;
        textoVelocimetro.text = "Velocidad: " + velocidadActual.ToString("0.0") + " km/h";
    }
}