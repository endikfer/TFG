using UnityEngine;

public class InputGeneracion : MonoBehaviour
{
    [Header("Referencia al generador")]
    public Generador2 generador;

    [Header("Tecla de regeneración")]
    public KeyCode teclaRegenerar = KeyCode.R;

    [Header("Bloqueo de input")]
    [Tooltip("Mientras este panel esté activo, la tecla de regeneración se desactiva.")]
    public GameObject panelGuardado;

    [Tooltip("Mientras este panel esté activo, la tecla de regeneración se desactiva.")]
    public GameObject panelCarga;

    void Update()
    {
        if (panelGuardado != null && panelGuardado.activeSelf) return;
        if (panelCarga != null && panelCarga.activeSelf) return;

        if (Input.GetKeyDown(teclaRegenerar))
        {
            generador.Regenerar();
        }
    }
}