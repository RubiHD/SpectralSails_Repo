using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
   public void CargarEscena(string Tutorial)
    {
        SceneManager.LoadScene(Tutorial);
    }

    public void Reset()
    {
        Scene escenaActual = SceneManager.GetActiveScene();
        SceneManager.LoadScene(escenaActual.name);
    }
}
