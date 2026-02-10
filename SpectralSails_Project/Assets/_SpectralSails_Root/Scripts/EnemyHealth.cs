using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Health Bar Settings")]
    public float healthBarYOffset = 1.5f;
    public float hideDelay = 2f;

    private GameObject healthBarUI;
    private Image fillImage;
    private float hideTimer;

    // Referencia al Animator
    private Animator animator;

    private void Start()
    {
        currentHealth = maxHealth;

        // Obtener el componente Animator
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No se encontró Animator en " + gameObject.name);
        }

        InitializeHealthBar();
    }

    private void InitializeHealthBar()
    {
        GameObject prefab = Resources.Load<GameObject>("EnemyHealthBar");
        if (prefab == null)
        {
            Debug.LogWarning("EnemyHealthBar prefab no encontrado en Resources");
            return;
        }

        healthBarUI = Instantiate(prefab, transform);
        healthBarUI.transform.localPosition = new Vector3(0, healthBarYOffset, 0);
        fillImage = healthBarUI.transform.Find("Background/Fill").GetComponent<Image>();
        healthBarUI.SetActive(false);
    }

    private void Update()
    {
        if (healthBarUI != null && healthBarUI.activeSelf)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
                healthBarUI.SetActive(false);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Actualizar barra de vida
        if (fillImage != null)
        {
            float ratio = (float)currentHealth / maxHealth;
            fillImage.fillAmount = ratio;
        }

        if (healthBarUI != null)
        {
            healthBarUI.SetActive(true);
            hideTimer = hideDelay;
        }

        // Reproducir animación de golpe
        if (animator != null && currentHealth > 0)
        {
            animator.SetTrigger("Hit");
        }

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        // Reproducir animación de muerte
        if (animator != null)
        {
            animator.SetTrigger("Death"); // ⚠️ Asegúrate que sea "Death"
        }

        EnemyDeathHandler deathHandler = GetComponent<EnemyDeathHandler>();
        if (deathHandler != null)
        {
            deathHandler.Die();
        }
        else
        {
            Destroy(gameObject, 2f);
        }
    }
}


