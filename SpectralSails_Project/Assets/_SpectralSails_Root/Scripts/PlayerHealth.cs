using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int Health;

    private void Start()
    {
        Health = maxHealth;
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        Health -= damage;

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null && !controller.isDead)
        {
            controller.ApplyKnockback(attackerPosition, 8f); // Ajusta la fuerza a tu gusto

            if (Health <= 0)
            {
                controller.Die();
            }
        }
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

