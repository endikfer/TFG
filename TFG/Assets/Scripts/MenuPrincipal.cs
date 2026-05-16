using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelMenuPrincipal;
    public GameObject panelJugar;
    public GameObject panelOpciones;

    [Header("Referencias")]
    public MenuOpciones menuOpciones;

    // ── Unity lifecycle ───────────────────────────────────────────────────

    private void Start()
    {
        // Asegurarse de que solo el menú principal es visible al arrancar
        panelMenuPrincipal.SetActive(true);
        panelJugar.SetActive(false);
        panelOpciones.SetActive(false);
    }

    // ── Botones menú principal ────────────────────────────────────────────

    /// <summary>
    /// Botón "Jugar": muestra el subpanel con los modos de juego.
    /// </summary>
    public void OnBotonJugar()
    {
        panelJugar.SetActive(true);
    }

    /// <summary>
    /// Botón "Opciones": abre el panel de opciones.
    /// </summary>
    public void OnBotonOpciones()
    {
        panelJugar.SetActive(false); // Cierra el subpanel si estaba abierto
        menuOpciones.AbrirOpciones();
    }

    /// <summary>
    /// Botón "Salir": cierra el juego.
    /// </summary>
    public void OnBotonSalir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Botones subpanel jugar ────────────────────────────────────────────

    public void OnBotonEntrenar()
    {
        SceneManager.LoadScene("Entrenar");
    }

    public void OnBotonGenerarCircuitos()
    {
        SceneManager.LoadScene("Generar");
    }

    public void OnBotonEVE()
    {
        SceneManager.LoadScene("EVE");
    }

    public void OnBotonPVE()
    {
        SceneManager.LoadScene("PVE");
    }

    /// <summary>
    /// Botón "Volver" del subpanel: cierra el subpanel sin seleccionar nada.
    /// </summary>
    public void OnBotonVolverSubpanel()
    {
        panelJugar.SetActive(false);
    }
}