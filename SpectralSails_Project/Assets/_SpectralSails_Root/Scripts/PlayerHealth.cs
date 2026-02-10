using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Salud")]
    public int maxHealth = 5;
    public int Health { get; private set; }

    [Header("Knockback")]
    [SerializeField] private float knockbackForceMultiplier = 1f;

    [Header("Invulnerabilidad temporal")]
    [SerializeField] private float invulnerabilityDuration = 0.5f; // Tiempo de invulnerabilidad tras recibir daño
    private bool isInvulnerable = false;

    [Header("Eventos (Opcional)")]
    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnDeath;

    private PlayerController controller;
    private bool isDead = false;

    private void Start()
    {
        Health = maxHealth;
        OnHealthChanged?.Invoke(Health, maxHealth);
        controller = GetComponent<PlayerController>();

        if (controller == null)
        {
            Debug.LogError("PlayerController no encontrado en el GameObject!");
        }
    }

    // ✅ MÉTODO PRINCIPAL MEJORADO
    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        // ✅ Prevenir daño si está muerto o invulnerable
        if (isDead || isInvulnerable) return;

        Health -= damage;
        Health = Mathf.Max(Health, 0);

        Debug.Log($"Jugador recibió {damage} daño. Vida restante: {Health}/{maxHealth}");

        OnHealthChanged?.Invoke(Health, maxHealth);

        // ✅ COMPROBAR MUERTE PRIMERO
        if (Health <= 0)
        {
            HandleDeath();
        }
        else
        {
            // ✅ Solo aplicar knockback si sigue vivo
            if (controller != null)
            {
                controller.ApplyKnockback(attackerPosition, knockbackForceMultiplier);
            }

            // ✅ Activar invulnerabilidad temporal
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    // ✅ NUEVO: Método separado para manejar la muerte
    private void HandleDeath()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("¡Jugador ha muerto! Activando secuencia de muerte.");

        // ✅ Invocar evento de muerte
        OnDeath?.Invoke();

        // ✅ Llamar al método Die() del controller
        if (controller != null)
        {
            controller.Die();
        }
    }

    // ✅ NUEVO: Coroutine de invulnerabilidad
    private System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    // Sobrecarga opcional para compatibilidad
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position);
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        int previousHealth = Health;
        Health += amount;
        Health = Mathf.Min(Health, maxHealth);

        Debug.Log($"Jugador curado: +{Health - previousHealth}. Vida: {Health}/{maxHealth}");

        OnHealthChanged?.Invoke(Health, maxHealth);
    }

    public float GetHealthPercentage()
    {
        return (float)Health / (float)maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }
}