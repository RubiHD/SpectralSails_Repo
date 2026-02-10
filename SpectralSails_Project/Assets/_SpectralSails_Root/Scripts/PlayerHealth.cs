using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Salud")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Referencias")]
    private PlayerController playerController;

    [Header("Configuración de Daño")]
    public float invulnerabilityTime = 1.5f; // Tiempo de invulnerabilidad después de recibir daño
    private float invulnerabilityTimer = 0f;

    [Header("Efectos Visuales")]
    public bool useFlashEffect = true;
    public float flashSpeed = 5f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Start()
    {
        currentHealth = maxHealth;
        playerController = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (playerController == null)
        {
            Debug.LogError("PlayerController no encontrado en " + gameObject.name);
        }

        if (showDebugLogs)
        {
            Debug.Log($"PlayerHealth inicializado. Vida máxima: {maxHealth}");
        }
    }

    private void Update()
    {
        // Reducir timer de invulnerabilidad
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;

            // Efecto de parpadeo mientras es invulnerable
            if (useFlashEffect && spriteRenderer != null)
            {
                float alpha = Mathf.PingPong(Time.time * flashSpeed, 1f);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            }
        }
        else if (spriteRenderer != null && spriteRenderer.color != originalColor)
        {
            // Restaurar color original cuando termine la invulnerabilidad
            spriteRenderer.color = originalColor;
        }
    }

    public void TakeDamage(int amount)
    {
        // Si está en invulnerabilidad, ignorar daño
        if (invulnerabilityTimer > 0f)
        {
            if (showDebugLogs)
                Debug.Log("Jugador es invulnerable, daño ignorado");
            return;
        }

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (showDebugLogs)
            Debug.Log($"Jugador recibió {amount} de daño. Vida restante: {currentHealth}/{maxHealth}");

        // Activar invulnerabilidad
        invulnerabilityTimer = invulnerabilityTime;

        // Aplicar knockback si hay PlayerController
        if (playerController != null)
        {
            // Buscar al tiburón más cercano para calcular dirección del knockback
            GameObject shark = GameObject.FindGameObjectWithTag("Enemy");
            if (shark != null)
            {
                playerController.ApplyKnockback(shark.transform.position, 1f);
            }
            else
            {
                // Si no hay tiburón, aplicar knockback genérico
                Vector2 knockbackDir = new Vector2(transform.localScale.x * -1, 0);
                playerController.ApplyKnockback(transform.position + (Vector3)knockbackDir, 1f);
            }
        }

        // Verificar si murió
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        if (showDebugLogs)
            Debug.Log($"Jugador curado por {amount}. Vida actual: {currentHealth}/{maxHealth}");
    }

    private void Die()
    {
        if (showDebugLogs)
            Debug.Log("PlayerHealth: Jugador ha muerto, llamando a PlayerController.Die()");

        if (playerController != null)
        {
            playerController.Die();
        }
        else
        {
            Debug.LogError("No se pudo llamar a Die() porque PlayerController es null");
        }
    }

    // Método público para obtener la vida actual
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // Método público para verificar si está vivo
    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    // Método para mostrar la barra de vida (si la implementas más adelante)
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
}