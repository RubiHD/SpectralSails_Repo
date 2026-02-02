using UnityEngine;

public class ExitButton : MonoBehaviour
{
    public void Exit()
    {
        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }
}
