using UnityEngine;

public class CocheContactoM : MonoBehaviour
{
    [SerializeField] private AgenteM karAgent;

    private void OnCollisionEnter(Collision col)
    {
        //karAgent.salidaDePista = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        //karAgent.salidaDePista = false;
    }
}
