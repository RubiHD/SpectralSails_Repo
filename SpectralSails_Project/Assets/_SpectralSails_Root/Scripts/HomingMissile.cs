using UnityEngine;

public class HomingMissile : MonoBehaviour
{
    [Header("Objetivo")]
    private Transform target;

    [Header("Movimiento Teledirigido")]
    [SerializeField] private float homingSpeed = 4f; // Velocidad durante fase teledirigida
    [SerializeField] private float homingDuration = 2f; // ✅ REDUCIDO: Solo 2 segundos teledirigido
    [SerializeField] private float rotationSpeed = 200f; // ✅ REDUCIDO: Gira más lento (más esquivable)

    [Header("Fase Lineal")]
    [SerializeField] private float straightSpeed = 6f; // ✅ NUEVO: Velocidad tras desactivar teledirigido

    [Header("Daño")]
    [SerializeField] private int damage = 1;

    [Header("Vida del Proyectil")]
    [SerializeField] private float lifeTime = 8f;

    [Header("Efectos Visuales")]
    [SerializeField] private bool rotateSprite = true;
    [SerializeField] private GameObject explosionPrefab; // Opcional: explosión al destruirse

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private Rigidbody2D rb;
    private float homingTimer;
    private bool isHoming = true;
    private Vector2 straightDirection; // Dirección cuando deja de ser teledirigido

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        homingTimer = homingDuration;

        // Rotación inicial hacia el objetivo
        if (target != null && rotateSprite)
        {
            Vector2 initialDirection = (target.position - transform.position).normalized;
            float initialAngle = Mathf.Atan2(initialDirection.y, initialDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, initialAngle);
        }

        Destroy(gameObject, lifeTime);

        if (showDebugLogs)
            Debug.Log("Misil lanzado - Fase teledirigida activa");
    }

    private void Update()
    {
        if (isHoming)
        {
            homingTimer -= Time.deltaTime;

            // ✅ Al acabar el tiempo, cambiar a movimiento lineal
            if (homingTimer <= 0f)
            {
                isHoming = false;
                straightDirection = rb.linearVelocity.normalized;

                if (showDebugLogs)
                    Debug.Log("Misil cambió a movimiento lineal - Ahora es más fácil de esquivar");
            }
        }
    }

    private void FixedUpdate()
    {
        if (isHoming && target != null)
        {
            // 🎯 FASE TELEDIRIGIDA (solo primeros 2 segundos)
            Vector2 targetPosition = target.position;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

            // Rotar hacia el objetivo
            if (rotateSprite)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                );

                // Moverse en la dirección del sprite
                rb.linearVelocity = transform.right * homingSpeed;
            }
            else
            {
                rb.linearVelocity = direction * homingSpeed;
            }
        }
        else
        {
            // ⚡ FASE LINEAL (después de 2 segundos)
            // Ahora solo va en línea recta - MÁS FÁCIL DE ESQUIVAR
            rb.linearVelocity = straightDirection * straightSpeed;

            // Mantener rotación
            if (rotateSprite)
            {
                float angle = Mathf.Atan2(straightDirection.y, straightDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);

                if (showDebugLogs)
                    Debug.Log("¡Misil impactó al jugador!");
            }

            DestroyMissile();
        }

        if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            if (showDebugLogs)
                Debug.Log("Misil chocó con terreno");

            DestroyMissile();
        }
    }

    private void DestroyMissile()
    {
        // Efectos de explosión opcionales
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}