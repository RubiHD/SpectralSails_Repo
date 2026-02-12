using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string cinematicSceneName = "CinematicaEscena";
    [SerializeField] private float delayBeforeTransition = 1f;

    // Llama a este método cuando el jugador complete el nivel
    public void OnLevelComplete()
    {
        StartCoroutine(TransitionToCinematic());
    }

    private System.Collections.IEnumerator TransitionToCinematic()
    {
        // Esperar un momento antes de cambiar (opcional)
        yield return new WaitForSeconds(delayBeforeTransition);

        // Cargar la escena de cinemática
        SceneManager.LoadScene(cinematicSceneName);
    }

    // Ejemplo de uso con trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnLevelComplete();
        }
    }

    // O usar con un método público que llames desde otro script
}