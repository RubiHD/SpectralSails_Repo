using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Health Bar")]
    public Transform healthBar;
    public Transform fill;

    [Header("Visibility Settings")]
    public float hideDelay = 2f; // Tiempo antes de ocultar la barra
    private float hideTimer;

    private void Start()
    {
        currentHealth = maxHealth;

        GameObject bar = Instantiate(Resources.Load<GameObject>("HealthBar"), transform);
        bar.transform.localPosition = new Vector3(0, 1f, 0);

        healthBar = bar.transform;
        fill = bar.transform.GetChild(0);

        UpdateHealthBar(); // ← IMPORTANTE

        healthBar.gameObject.SetActive(false);
    }


    private void Update()
    {
        // Ocultar barra si ha pasado el tiempo
        if (healthBar.gameObject.activeSelf)
        {
            hideTimer -= Time.deltaTime;

            if (hideTimer <= 0)
                healthBar.gameObject.SetActive(false);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        // Mostrar barra al recibir daño
        healthBar.gameObject.SetActive(true);
        hideTimer = hideDelay;

        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
    }

    private void UpdateHealthBar()
    {
        float ratio = (float)currentHealth / maxHealth;
        fill.localScale = new Vector3(ratio, 1, 1);
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}

