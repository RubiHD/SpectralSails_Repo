using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Salud")]
    public int maxHealth = 5;
    public int Health { get; private set; } // Hacer Health de solo lectura desde fuera

    [Header("Eventos (Opcional)")]
    public UnityEvent<int, int> OnHealthChanged; // Evento: (vidaActual, vidaMaxima)
    public UnityEvent OnDeath;

    private void Start()
    {
        Health = maxHealth;
        OnHealthChanged?.Invoke(Health, maxHealth);
    }

    // Método principal con knockback
    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        if (Health <= 0) return; // Ya está muerto, ignorar daño adicional

        Health -= damage;
        Health = Mathf.Max(Health, 0); // Asegurar que no sea negativo

        Debug.Log($"Jugador recibió {damage} daño. Vida restante: {Health}/{maxHealth}");

        // Invocar evento de cambio de salud
        OnHealthChanged?.Invoke(Health, maxHealth);

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.ApplyKnockback(attackerPosition, 3f);

            if (Health <= 0)
            {
                OnDeath?.Invoke();
                controller.Die();
            }
        }
    }

    // Sobrecarga opcional para compatibilidad con llamadas antiguas
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position);
    }

    public void Heal(int amount)
    {
        if (Health <= 0) return; // No curar si está muerto

        int previousHealth = Health;
        Health += amount;
        Health = Mathf.Min(Health, maxHealth); // No exceder vida máxima

        Debug.Log($"Jugador curado: +{Health - previousHealth}. Vida: {Health}/{maxHealth}");

        // Invocar evento de cambio de salud
        OnHealthChanged?.Invoke(Health, maxHealth);
    }

    // Método útil para debugging
    public float GetHealthPercentage()
    {
        return (float)Health / (float)maxHealth;
    }
}