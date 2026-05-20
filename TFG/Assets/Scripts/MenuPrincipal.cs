using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelMenuPrincipal;
    public GameObject panelJugar;

    [Header("Referencias")]
    public GameObject optionsCanvas;
    public MenuOpciones menuOpciones;

    private void Start()
    {
        panelMenuPrincipal.SetActive(true);
        panelJugar.SetActive(false);

        // Asignar callback: al cerrar opciones vuelve al menú principal
        if (menuOpciones != null)
        {
            menuOpciones.onCerrar.RemoveAllListeners();
            menuOpciones.onCerrar.AddListener(OnOpcionesCerradas);
        }
    }

    // ── Botones menú principal ────────────────────────────────────────────

    public void OnBotonJugar()
    {
        panelJugar.SetActive(true);
    }

    public void OnBotonOpciones()
    {
        panelJugar.SetActive(false);
        panelMenuPrincipal.SetActive(false);

        if (optionsCanvas != null)
            optionsCanvas.SetActive(true);

        menuOpciones?.AbrirOpciones();
    }

    public void OnBotonSalir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Botones subpanel jugar ────────────────────────────────────────────

    public void OnBotonEntrenar() => SceneManager.LoadScene("Entrenar");
    public void OnBotonGenerarCircuitos() => SceneManager.LoadScene("Generar");
    public void OnBotonEVE() => SceneManager.LoadScene("EVE");
    public void OnBotonPVE() => SceneManager.LoadScene("PVE");

    public void OnBotonVolverSubpanel()
    {
        panelJugar.SetActive(false);
    }

    // ── Callback de opciones ──────────────────────────────────────────────

    private void OnOpcionesCerradas()
    {
        if (optionsCanvas != null)
            optionsCanvas.SetActive(false);

        panelMenuPrincipal.SetActive(true);
    }
}