using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioDeEscena : MonoBehaviour
{
    public string nombreDeLaEscena;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene(nombreDeLaEscena);
        }
    }
}
