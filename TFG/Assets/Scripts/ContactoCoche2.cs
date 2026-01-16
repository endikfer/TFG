using UnityEngine;

public class ContactoCoche2 : MonoBehaviour
{
    [SerializeField] private Agente karAgent;
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
