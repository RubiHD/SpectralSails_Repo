using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int Health;

    private void Start()
    {
        Health = maxHealth;
    }

    // Método principal con knockback
    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        Health -= damage;

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.ApplyKnockback(attackerPosition, 3f); // Puedes ajustar la fuerza del empujón

            if (Health <= 0)
            {
                controller.Die();
            }
        }
    }

    // Sobrecarga opcional para compatibilidad con llamadas antiguas
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position); // Usa la posición del jugador como fallback
    }

    public void Heal(int amount)
    {
        Health += amount;

        if (Health > maxHealth)
        {
            Health = maxHealth;
        }
    }
}
