using UnityEngine;

public class JellyfishEnemy : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    private PlayerController playerController;

    [Header("Patrulla")]
    public Transform pointA; // Punto inicial de patrulla
    public Transform pointB; // Punto final de patrulla
    public float moveSpeed = 2f;
    public float pauseTime = 1f; // Tiempo de pausa en cada extremo

    [Header("Contacto y Daño")]
    public int contactDamage = 1;
    public float knockbackForce = 5f;
    public GameObject damageParticlePrefab; // Partículas de daño eléctrico
    public float damageCooldown = 1f; // Tiempo entre daños

    [Header("🔧 DEBUG")]
    public bool ignoreWaterCheck = false;

    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private bool movingToB = true;
    private bool isPaused = false;
    private float pauseTimer = 0f;
    private bool isDead = false;
    private float lastDamageTime = 0f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Buscar al jugador
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerController = player.GetComponent<PlayerController>();
            }
        }

        // Configurar puntos de patrulla
        if (pointA == null || pointB == null)
        {
            // Crear puntos por defecto si no están asignados
            CreateDefaultPatrolPoints();
        }

        // Empezar en punto A
        transform.position = pointA.position;
        targetPosition = pointB.position;

        Debug.Log($"Jellyfish iniciada. PointA: {pointA.position}, PointB: {pointB.position}");
    }

    private void CreateDefaultPatrolPoints()
    {
        GameObject pointsParent = new GameObject($"{gameObject.name}_PatrolPoints");
        pointsParent.transform.position = transform.position;

        GameObject pA = new GameObject("PointA");
        pA.transform.parent = pointsParent.transform;
        pA.transform.position = transform.position + Vector3.left * 3f;
        pointA = pA.transform;

        GameObject pB = new GameObject("PointB");
        pB.transform.parent = pointsParent.transform;
        pB.transform.position = transform.position + Vector3.right * 3f;
        pointB = pB.transform;

        Debug.LogWarning($"Puntos de patrulla creados automáticamente para {gameObject.name}");
    }

    private void Update()
    {
        if (isDead) return;

        // ✅ Solo actuar si el jugador está bajo el agua (opcional)
        if (!ignoreWaterCheck && playerController != null && !playerController.IsUnderwater())
        {
            // Si el jugador no está bajo el agua, pausar el enemigo
            if (animator != null)
                animator.SetFloat("Speed", 0f);
            return;
        }

        // Lógica de patrulla
        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPaused = false;
            }

            if (animator != null)
                animator.SetFloat("Speed", 0f);

            return;
        }

        // Moverse hacia el objetivo
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Actualizar animación de movimiento
        if (animator != null)
        {
            animator.SetFloat("Speed", moveSpeed);
        }

        // Voltear sprite según dirección
        if (direction.x > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        // Verificar si llegó al objetivo
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            ReachedPoint();
        }
    }

    private void FixedUpdate()
    {
        if (isDead || isPaused) return;

        // Movimiento con física (si usas Rigidbody2D)
        if (rb != null)
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
    }

    private void ReachedPoint()
    {
        // Cambiar dirección
        if (movingToB)
        {
            targetPosition = pointA.position;
            movingToB = false;
        }
        else
        {
            targetPosition = pointB.position;
            movingToB = true;
        }

        // Pausar
        if (pauseTime > 0f)
        {
            isPaused = true;
            pauseTimer = pauseTime;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }
    }

    // ⚡ DAÑO POR CONTACTO
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        // Verificar si es el jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            DamagePlayer(collision.gameObject);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        // Daño continuo mientras está en contacto
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                DamagePlayer(collision.gameObject);
            }
        }
    }

    private void DamagePlayer(GameObject playerObj)
    {
        lastDamageTime = Time.time;

        // ⚡ Instanciar partículas de daño
        if (damageParticlePrefab != null)
        {
            Vector3 contactPoint = playerObj.transform.position;
            GameObject particles = Instantiate(damageParticlePrefab, contactPoint, Quaternion.identity);
            Destroy(particles, 2f); // Destruir partículas después de 2 segundos
        }

        // Aplicar daño al jugador
        PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
        }

        // Aplicar knockback al jugador
        PlayerController playerCtrl = playerObj.GetComponent<PlayerController>();
        if (playerCtrl != null)
        {
            playerCtrl.ApplyKnockback(transform.position, knockbackForce);
        }

        Debug.Log("¡Jellyfish hizo daño al jugador!");
    }

    // ✅ COMPATIBILIDAD CON EnemyDeathHandler
    public void DisableBehavior()
    {
        isDead = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }

        // Desactivar collider para evitar más daño
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    // ✅ GIZMOS PARA VISUALIZAR PATRULLA
    private void OnDrawGizmosSelected()
    {
        if (pointA != null && pointB != null)
        {
            // Línea de patrulla
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA.position, pointB.position);

            // Puntos de patrulla
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pointA.position, 0.3f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pointB.position, 0.3f);
        }
        else
        {
            // Vista previa de patrulla por defecto
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.left * 3f, transform.position + Vector3.right * 3f);
        }
    }
}
