using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image healthBarFill; // La imagen con Image Type = Filled
    [SerializeField] private PlayerHealth playerHealth; // Referencia al script de salud

    [Header("Configuración de Color")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f; // 30%

    [Header("Animación de Degradado")] // ✅ NUEVO
    [SerializeField] private bool useSmoothing = true; // Activar/desactivar animación
    [SerializeField] private float smoothSpeed = 5f; // Velocidad del degradado (mayor = más rápido)
    [SerializeField] private AnimationCurve smoothCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Curva de suavizado

    private float currentFillAmount; // Valor actual de la barra (para animación)
    private float targetFillAmount;  // Valor objetivo de la barra
    private Coroutine smoothCoroutine; // Referencia a la coroutine activa

    private void Start()
    {
        // Si no asignaste el PlayerHealth manualmente, búscalo
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }

        // ✅ Inicializar valores
        float initialHealth = (float)playerHealth.Health / (float)playerHealth.maxHealth;
        currentFillAmount = initialHealth;
        targetFillAmount = initialHealth;

        UpdateHealthBar();
    }

    private void Update()
    {
        if (!useSmoothing)
        {
            // Modo sin animación (actualización instantánea)
            UpdateHealthBar();
        }
        else
        {
            // Modo con animación (usar smoothing)
            float newTargetHealth = (float)playerHealth.Health / (float)playerHealth.maxHealth;

            // Solo iniciar animación si el objetivo cambió
            if (Mathf.Abs(newTargetHealth - targetFillAmount) > 0.001f)
            {
                targetFillAmount = newTargetHealth;

                // Detener animación anterior si existe
                if (smoothCoroutine != null)
                {
                    StopCoroutine(smoothCoroutine);
                }

                // Iniciar nueva animación
                smoothCoroutine = StartCoroutine(SmoothHealthChange());
            }

            // Actualizar color siempre
            UpdateHealthColor();
        }
    }

    // ✅ NUEVA COROUTINE: Anima el cambio de vida suavemente
    private IEnumerator SmoothHealthChange()
    {
        float startFillAmount = currentFillAmount;
        float elapsed = 0f;
        float duration = 1f / smoothSpeed; // Convertir velocidad a duración

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Aplicar curva de suavizado
            float curveValue = smoothCurve.Evaluate(t);

            // Interpolar entre valor actual y objetivo
            currentFillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, curveValue);

            // Actualizar la barra
            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = currentFillAmount;
            }

            yield return null;
        }

        // Asegurar que llegue exactamente al objetivo
        currentFillAmount = targetFillAmount;
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentFillAmount;
        }
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
        currentFillAmount = healthPercentage;
        targetFillAmount = healthPercentage;

        UpdateHealthColor();
    }

    private void UpdateHealthColor()
    {
        if (healthBarFill == null) return;

        // Usar currentFillAmount para el color (así el color sigue la animación)
        float healthPercentage = currentFillAmount;

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