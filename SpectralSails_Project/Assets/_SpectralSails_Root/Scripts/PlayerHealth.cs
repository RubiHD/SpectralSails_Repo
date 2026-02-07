using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Salud")]
    public int maxHealth = 5;
    public int Health { get; private set; }

    [Header("Knockback")]
    [SerializeField] private float knockbackForceMultiplier = 1f; // Ajusta la intensidad del knockback

    [Header("Eventos (Opcional)")]
    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnDeath;

    private void Start()
    {
        Health = maxHealth;
        OnHealthChanged?.Invoke(Health, maxHealth);
    }

    // ✅ Método principal con knockback mejorado
    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        if (Health <= 0) return;

        Health -= damage;
        Health = Mathf.Max(Health, 0);

        Debug.Log($"Jugador recibió {damage} daño. Vida restante: {Health}/{maxHealth}");

        OnHealthChanged?.Invoke(Health, maxHealth);

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            // ✅ APLICAR KNOCKBACK CON SALTITO
            controller.ApplyKnockback(attackerPosition, knockbackForceMultiplier);

            if (Health <= 0)
            {
                OnDeath?.Invoke();
                controller.Die();
            }
        }
    }

    // Sobrecarga opcional para compatibilidad
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position);
    }

    public void Heal(int amount)
    {
        if (Health <= 0) return;

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
}