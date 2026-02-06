using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image healthFill;

    public void SetMaxHealth(int maxHealth)
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = 1f;
        }
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }
}
