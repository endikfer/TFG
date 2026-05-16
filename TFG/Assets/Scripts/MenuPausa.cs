using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    [Header("Canvases")]
    public GameObject pauseCanvas;
    public GameObject optionsCanvas;

    [Header("Paneles")]
    public GameObject pausePanel;
    public GameObject confirmPanel;
    public GameObject optionsPanel;

    [Header("Referencias")]
    public MenuOpciones menuOpciones;

    [Header("Configuración")]
    public string escenaMenu = "MainMenu";
    public KeyCode teclaPausa = KeyCode.Escape;

    private bool pausado = false;
    private float timeScaleAnterior = 1f;

    private void Start()
    {
        pauseCanvas.SetActive(false);

        // Asignar callback: al cerrar opciones vuelve al panel de pausa
        if (menuOpciones != null)
        {
            menuOpciones.onCerrar.RemoveAllListeners();
            menuOpciones.onCerrar.AddListener(OnOpcionesCerradas);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(teclaPausa))
        {
            // Si el canvas de opciones está abierto, el Escape lo cierra a él
            if (optionsCanvas != null && optionsCanvas.activeSelf)
            {
                menuOpciones?.SalirSinGuardar();
                return;
            }

            // Si no, gestiona la pausa normalmente
            if (pausado) Reanudar();
            else Pausar();
        }
    }

    // ── Pausa / Reanuda ──────────────────────────────────────────────────

    public void Pausar()
    {
        timeScaleAnterior = Time.timeScale;
        Time.timeScale = 0f;
        pausado = true;

        pauseCanvas.SetActive(true);
        pausePanel.SetActive(true);
        confirmPanel.SetActive(false);

        if (optionsCanvas != null)
            optionsCanvas.SetActive(false);
    }

    public void Reanudar()
    {
        Time.timeScale = timeScaleAnterior;
        pausado = false;

        pauseCanvas.SetActive(false);
        pausePanel.SetActive(false);
        confirmPanel.SetActive(false);
    }

    // ── Botones del PausePanel ───────────────────────────────────────────

    public void BotonVolver() => Reanudar();

    public void BotonOpciones()
    {
        pausePanel.SetActive(false);

        if (optionsCanvas != null)
            optionsCanvas.SetActive(true);
        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        menuOpciones?.AbrirOpciones();
    }

    public void BotonSalir()
    {
        pausePanel.SetActive(false);
        confirmPanel.SetActive(true);
    }

    // ── Botones del ConfirmPanel ─────────────────────────────────────────

    public void BotonConfirmNo()
    {
        confirmPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void BotonConfirmSi()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaMenu);
    }

    // ── Callback de opciones ─────────────────────────────────────────────

    private void OnOpcionesCerradas()
    {
        if (optionsCanvas != null)
            optionsCanvas.SetActive(false);

        pausePanel.SetActive(true);
    }
}