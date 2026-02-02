using UnityEngine;

public class OpenSettings : MonoBehaviour
{
    public GameObject settingsPanel;

    public void AbrirPanel()
    {
        settingsPanel.SetActive(true);
    }

    public void CerrarPanel()
    {
        settingsPanel.SetActive(false);
    }
}
