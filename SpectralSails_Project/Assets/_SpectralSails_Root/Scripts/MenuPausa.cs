using UnityEngine;
using UnityEngine.SceneManagement;   // ← ESTO ES IMPRESCINDIBLE
using UnityEngine.InputSystem;


public class MenuPausa : MonoBehaviour
{
    public static bool juegoEnPausa = false;

    public GameObject panelPausa;
    public GameObject panelMuerte;

    private InputAction pauseAction;

    void Awake()
    {
        pauseAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/escape");
    }

    void OnEnable()
    {
        pauseAction.Enable();
    }

    void OnDisable()
    {
        pauseAction.Disable();
    }

    void Update()
    {
        if (pauseAction.triggered && !panelMuerte.activeSelf)
        {
            if (juegoEnPausa)
                Reanudar();
            else
                Pausar();
        }
    }

    public void Pausar()
    {
        panelPausa.SetActive(true);
        Time.timeScale = 0f;
        juegoEnPausa = true;
    }

    public void Reanudar()
    {
        panelPausa.SetActive(false);
        Time.timeScale = 1f;
        juegoEnPausa = false;
    }

    // 🔥 NUEVO: ahora espera 1.5 segundos antes de mostrar el panel de muerte
    public void MostrarPanelMuerte()
    {
        StartCoroutine(MostrarPanelMuerteConRetraso());
    }

    private System.Collections.IEnumerator MostrarPanelMuerteConRetraso()
    {
        yield return new WaitForSeconds(1.5f);

        panelMuerte.SetActive(true);
        Time.timeScale = 0f;
        juegoEnPausa = true;
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        juegoEnPausa = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f;
        juegoEnPausa = false;
        SceneManager.LoadScene("Main_Menu");
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}

