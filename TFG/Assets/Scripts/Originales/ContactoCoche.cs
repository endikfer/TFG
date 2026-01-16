using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactoCoche : MonoBehaviour
{
    // Start is called before the first frame update


    //Hay qe quitar o activar dependendiendo del 

    [SerializeField] private AgentToDrive karAgent;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision col)
    {

        karAgent.salidaDePista = true;

    }

    private void OnCollisionExit(Collision collision)
    {
        karAgent.salidaDePista = false; 
    }

}






