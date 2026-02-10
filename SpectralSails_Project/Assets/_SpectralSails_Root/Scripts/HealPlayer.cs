using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Curación")]
    public int healAmount = 1;

    [Header("Efectos Visuales")]
    [SerializeField] private GameObject healParticlesPrefab; // Prefab de partículas
    [SerializeField] private AudioClip healSound; // Sonido de curación (opcional)

    [Header("Animación del Pickup")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float rotationSpeed = 50f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Efecto de flotación
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotación continua
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHealth player = collision.GetComponent<PlayerHealth>();
        if (player != null)
        {
            // Curar al jugador
            player.Heal(healAmount);

            // Activar animación de curación en el jugador
            Animator animator = player.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Heal");
            }

            // Obtener HealthBarController y activar efecto
            HealthBarController healthBar = FindObjectOfType<HealthBarController>();
            if (healthBar != null)
            {
                healthBar.TriggerHealEffect();
            }

            // Instanciar partículas en la posición del jugador
            if (healParticlesPrefab != null)
            {
                GameObject particles = Instantiate(healParticlesPrefab, collision.transform.position, Quaternion.identity);
                Destroy(particles, 2f); // Destruir después de 2 segundos
            }

            // Reproducir sonido (opcional)
            if (healSound != null)
            {
                AudioSource.PlayClipAtPoint(healSound, transform.position);
            }

            // Destruir el pickup
            Destroy(gameObject);
        }
    }
}
