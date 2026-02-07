using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image healthBarFill; // La imagen con Image Type = Filled
    [SerializeField] private PlayerHealth playerHealth; // Referencia al script de salud

    [Header("Configuración")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f; // 30%

    private void Start()
    {
        // Si no asignaste el PlayerHealth manualmente, búscalo
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }

        UpdateHealthBar();
    }

    private void Update()
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (playerHealth == null || healthBarFill == null)
        {
            Debug.LogError("¡Falta asignar PlayerHealth o HealthBarFill en el Inspector!");
            return;
        }

        // Calcular el porcentaje de vida (0.0 a 1.0)
        float healthPercentage = (float)playerHealth.Health / (float)playerHealth.maxHealth;

        // Actualizar el Fill Amount
        healthBarFill.fillAmount = healthPercentage;

        // Cambiar color según la vida restante
        if (healthPercentage <= lowHealthThreshold)
        {
            healthBarFill.color = lowHealthColor;
        }
        else
        {
            healthBarFill.color = Color.Lerp(lowHealthColor, fullHealthColor,
                (healthPercentage - lowHealthThreshold) / (1f - lowHealthThreshold));
        }
    }
}