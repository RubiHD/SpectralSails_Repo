using UnityEngine;

public class Spikes : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [SerializeField] private int damage = 1; // Daño que hacen los pinchos

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 1.5f; // Multiplicador de knockback
    [SerializeField] private bool applyKnockback = true; // ¿Aplicar knockback al tocar?

    [Header("Cooldown de Daño")]
    [SerializeField] private bool useDamageCooldown = true; // Evitar spam de daño
    [SerializeField] private float damageCooldown = 0.5f; // Tiempo entre cada golpe

    [Header("Efectos Visuales (Opcional)")]
    [SerializeField] private GameObject hitParticles; // Partículas al golpear
    [SerializeField] private AudioClip hitSound; // Sonido al golpear

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    // Control de cooldown por jugador
    private float lastDamageTime = -999f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si es el jugador
        if (other.CompareTag("Player"))
        {
            DamagePlayer(other.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Si el jugador sigue tocando los pinchos, seguir dañando
        if (other.CompareTag("Player") && !useDamageCooldown)
        {
            DamagePlayer(other.gameObject);
        }
    }

    private void DamagePlayer(GameObject player)
    {
        // Verificar cooldown
        if (useDamageCooldown && Time.time < lastDamageTime + damageCooldown)
        {
            return; // Aún en cooldown
        }

        // Obtener componentes del jugador
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        PlayerController playerController = player.GetComponent<PlayerController>();

        if (playerHealth == null)
        {
            Debug.LogWarning("¡PlayerHealth no encontrado en el jugador!");
            return;
        }

        // Aplicar daño
        if (applyKnockback && playerController != null)
        {
            // Daño con knockback
            playerHealth.TakeDamage(damage, transform.position);

            // Puedes ajustar el knockback si quieres que sea diferente
            // playerController.ApplyKnockback(transform.position, knockbackForce);
        }
        else
        {
            // Daño sin knockback
            playerHealth.TakeDamage(damage);
        }

        // Actualizar tiempo del último daño
        lastDamageTime = Time.time;

        // Efectos visuales
        SpawnEffects();

        if (showDebugLogs)
        {
            Debug.Log($"Pinchos golpearon al jugador. Daño: {damage}");
        }
    }

    private void SpawnEffects()
    {
        // Partículas
        if (hitParticles != null)
        {
            // Instanciar en el centro de los pinchos
            Instantiate(hitParticles, transform.position, Quaternion.identity);
        }

        // Sonido
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
    }

    // Método opcional para activar/desactivar los pinchos dinámicamente
    public void SetActive(bool active)
    {
        GetComponent<Collider2D>().enabled = active;
    }
}