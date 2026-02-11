using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    public static bool juegoEnPausa = false;

    public GameObject panelPausa;  // Panel al presionar ESC
    public GameObject panelMuerte; // Panel al morir

    void Update()
    {
        // Detectar tecla ESC (solo si no está muerto)
        if (Input.GetKeyDown(KeyCode.Escape) && !panelMuerte.activeSelf)
        {
            if (juegoEnPausa)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
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

    public void MostrarPanelMuerte()
    {
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
        SceneManager.LoadScene("MenuPrincipal"); // Cambia por tu escena de menú
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
