using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextController : MonoBehaviour
{

    [SerializeField] private AgentToDrive karAgent;
    [SerializeField] private PrometeoCarController karAgentControll;


    public float myVariable; // Variable que deseas mostrar en el texto superpuesto

    public float speed; 



    void Update()
    {

        myVariable = karAgent.GetCumulativeReward();
        speed = karAgentControll.carSpeed;
        // Actualiza el texto con el valor de la variable en cada fotograma

    }
}
