using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthBarBackground; // Opcional: fondo de la barra
    [SerializeField] private ParticleSystem healParticles; // Partículas en la UI

    [Header("Configuración de Color")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;

    [Header("Animación de Degradado")]
    [SerializeField] private bool useSmoothing = true;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private AnimationCurve smoothCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("⭐ EFECTO DE CURACIÓN")]
    [SerializeField] private Color healGlowColor = new Color(0f, 1f, 0.5f, 1f); // Verde brillante
    [SerializeField] private float healGlowDuration = 0.5f;
    [SerializeField] private float healPulseDuration = 0.3f;
    [SerializeField] private AnimationCurve healPulseCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.2f);
    [SerializeField] private bool spawnHealParticles = true;

    private float currentFillAmount;
    private float targetFillAmount;
    private Coroutine smoothCoroutine;
    private Coroutine healEffectCoroutine;
    private Color originalColor;

    private void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }

        float initialHealth = (float)playerHealth.Health / (float)playerHealth.maxHealth;
        currentFillAmount = initialHealth;
        targetFillAmount = initialHealth;

        originalColor = healthBarFill.color;

        UpdateHealthBar();
    }

    private void Update()
    {
        if (!useSmoothing)
        {
            UpdateHealthBar();
        }
        else
        {
            float newTargetHealth = (float)playerHealth.Health / (float)playerHealth.maxHealth;

            if (Mathf.Abs(newTargetHealth - targetFillAmount) > 0.001f)
            {
                targetFillAmount = newTargetHealth;

                if (smoothCoroutine != null)
                {
                    StopCoroutine(smoothCoroutine);
                }

                smoothCoroutine = StartCoroutine(SmoothHealthChange());
            }

            UpdateHealthColor();
        }
    }

    // ⭐ MÉTODO PÚBLICO PARA ACTIVAR EFECTO DE CURACIÓN
    public void TriggerHealEffect()
    {
        // Detener efecto anterior si existe
        if (healEffectCoroutine != null)
        {
            StopCoroutine(healEffectCoroutine);
        }

        healEffectCoroutine = StartCoroutine(HealEffectCoroutine());

        // Activar partículas si están asignadas
        if (spawnHealParticles && healParticles != null)
        {
            healParticles.Play();
        }
    }

    // ⭐ CORRUTINA DEL EFECTO DE CURACIÓN
    private IEnumerator HealEffectCoroutine()
    {
        // === FASE 1: PULSO DE ESCALA ===
        float elapsed = 0f;
        Vector3 originalScale = healthBarFill.transform.localScale;

        while (elapsed < healPulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / healPulseDuration;
            float pulseValue = healPulseCurve.Evaluate(t);

            healthBarFill.transform.localScale = originalScale * pulseValue;

            yield return null;
        }

        healthBarFill.transform.localScale = originalScale;

        // === FASE 2: BRILLO VERDE ===
        elapsed = 0f;
        Color startColor = healthBarFill.color;

        while (elapsed < healGlowDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / healGlowDuration;

            // Interpolar del color de curación al color normal
            healthBarFill.color = Color.Lerp(healGlowColor, startColor, t);

            // Opcional: hacer que el fondo también brille
            if (healthBarBackground != null)
            {
                float glowIntensity = Mathf.Sin(t * Mathf.PI); // Crea un pulso
                healthBarBackground.color = Color.Lerp(Color.white, healGlowColor, glowIntensity * 0.3f);
            }

            yield return null;
        }

        // Restaurar colores
        healthBarFill.color = startColor;
        if (healthBarBackground != null)
        {
            healthBarBackground.color = Color.white;
        }
    }

    private IEnumerator SmoothHealthChange()
    {
        float startFillAmount = currentFillAmount;
        float elapsed = 0f;
        float duration = 1f / smoothSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float curveValue = smoothCurve.Evaluate(t);
            currentFillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, curveValue);

            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = currentFillAmount;
            }

            yield return null;
        }

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

        float healthPercentage = (float)playerHealth.Health / (float)playerHealth.maxHealth;

        healthBarFill.fillAmount = healthPercentage;
        currentFillAmount = healthPercentage;
        targetFillAmount = healthPercentage;

        UpdateHealthColor();
    }

    private void UpdateHealthColor()
    {
        if (healthBarFill == null) return;

        float healthPercentage = currentFillAmount;

        if (healthPercentage <= lowHealthThreshold)
        {
            healthBarFill.color = lowHealthColor;
        }
        else
        {
            healthBarFill.color = Color.Lerp(lowHealthColor, fullHealthColor,
                (healthPercentage - lowHealthThreshold) / (1f - lowHealthThreshold));
        }

        originalColor = healthBarFill.color;
    }
}