using UnityEngine;

public class InputGeneracion : MonoBehaviour
{
    [Header("Referencia al generador")]
    public Generador2 generador;

    [Header("Tecla de regeneración")]
    public KeyCode teclaRegenerar = KeyCode.R;

    void Update()
    {
        if (Input.GetKeyDown(teclaRegenerar))
        {
            generador.Regenerar();
        }
    }
}
