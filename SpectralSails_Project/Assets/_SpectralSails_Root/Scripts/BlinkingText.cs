using UnityEngine;
using TMPro;
using System.Collections;


//Parpadeo del texto
public class BlinkingText : MonoBehaviour
{
    public TextMeshProUGUI texto;
    public float speed = 0.8f;

    private void Start()
    {
        StartCoroutine(Parpadear());
    }

    IEnumerator Parpadear()
    {
        while (true)
        {
            texto.enabled= !texto.enabled;
            yield return new WaitForSeconds(speed);
        }
    }


    //Pulsar cualquier tecla para iniciar

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            //cargar la siguiente escena
            UnityEngine.SceneManagement.SceneManager.LoadScene("Tutorial");
        }
    }



}
