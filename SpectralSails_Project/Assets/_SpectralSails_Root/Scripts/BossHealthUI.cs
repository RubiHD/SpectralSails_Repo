using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public Image fillImage;
    public GameObject uiRoot;

    public void SetHealth(float normalizedValue)
    {
        fillImage.fillAmount = normalizedValue;
    }

    public void Show()
    {
        uiRoot.SetActive(true);
    }

    public void Hide()
    {
        uiRoot.SetActive(false);
    }
}
