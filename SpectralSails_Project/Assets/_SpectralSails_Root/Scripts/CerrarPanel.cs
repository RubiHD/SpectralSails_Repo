using UnityEngine;

public class CerrarPanel : MonoBehaviour
{
    public GameObject panel;

    public void OcultarPanel()
    {
        panel.SetActive(false);
    }
}
