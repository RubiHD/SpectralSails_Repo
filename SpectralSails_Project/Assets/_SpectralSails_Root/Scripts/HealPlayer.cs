using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healAmount = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHealth player = collision.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.Heal(healAmount);

            // Activar animación de curación si el jugador tiene Animator
            Animator animator = player.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Heal");
            }

            Destroy(gameObject);
        }
    }
}
