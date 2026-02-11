using UnityEngine;

public class JellyfishEnemy : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    private PlayerController playerController;

    [Header("Patrulla")]
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;
    public float pauseTime = 1f;
    public float waypointReachDistance = 0.2f; // ✅ Distancia para considerar que llegó

    [Header("Contacto y Daño")]
    public int contactDamage = 1;
    public float knockbackForce = 5f;
    public GameObject damageParticlePrefab;
    public float damageCooldown = 1f;

    [Header("🎨 Rotación del Sprite")]
    public bool rotateTowardsMovement = false;
    public bool flipSpriteInsteadOfRotate = true;

    [Header("🔧 DEBUG")]
    public bool ignoreWaterCheck = false;
    public bool showDebugLogs = false; // ✅ NUEVO

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

        // ✅ Configurar Rigidbody2D correctamente
        if (rb != null)
        {
            rb.gravityScale = 0; // Sin gravedad
            rb.linearDamping = 0; // Sin fricción
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // No rotar
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerController = player.GetComponent<PlayerController>();
            }
        }

        if (pointA == null || pointB == null)
        {
            CreateDefaultPatrolPoints();
        }

        // Empezar en punto A
        transform.position = pointA.position;
        targetPosition = pointB.position;

        if (showDebugLogs)
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

        // Solo actuar si el jugador está bajo el agua
        if (!ignoreWaterCheck && playerController != null && !playerController.IsUnderwater())
        {
            if (animator != null)
                animator.SetFloat("Speed", 0f);

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            return;
        }

        // ✅ SISTEMA DE PAUSAS
        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;

            if (pauseTimer <= 0f)
            {
                isPaused = false;
                if (showDebugLogs)
                    Debug.Log("Jellyfish reanuda movimiento");
            }

            if (animator != null)
                animator.SetFloat("Speed", 0f);

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            return;
        }

        // ✅ Verificar si llegó al objetivo
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

        if (showDebugLogs)
            Debug.Log($"Distancia al objetivo: {distanceToTarget:F2}");

        if (distanceToTarget < waypointReachDistance)
        {
            ReachedPoint();
        }

        // Actualizar animación
        if (animator != null)
        {
            animator.SetFloat("Speed", isPaused ? 0f : moveSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (isDead || isPaused) return;

        // ✅ MOVIMIENTO ÚNICO EN FIXEDUPDATE
        if (rb != null)
        {
            Vector2 currentPos = rb.position;
            Vector2 direction = (targetPosition - currentPos).normalized;

            // ✅ Actualizar orientación del sprite
            UpdateSpriteOrientation(direction);

            // ✅ Mover usando velocidad (más suave)
            rb.linearVelocity = direction * moveSpeed;

            if (showDebugLogs)
            {
                Debug.Log($"Moviéndose hacia {targetPosition}, velocidad: {rb.linearVelocity}");
            }
        }
    }

    private void UpdateSpriteOrientation(Vector2 direction)
    {
        if (direction.magnitude < 0.01f) return;

        if (rotateTowardsMovement)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else if (flipSpriteInsteadOfRotate)
        {
            if (Mathf.Abs(direction.x) > 0.01f)
            {
                if (direction.x > 0.01f)
                    transform.localScale = new Vector3(1, 1, 1);
                else if (direction.x < -0.01f)
                    transform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }

    private void ReachedPoint()
    {
        if (showDebugLogs)
            Debug.Log("Jellyfish alcanzó punto de patrulla");

        // ✅ Detener movimiento inmediatamente
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

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
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            DamagePlayer(collision.gameObject);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

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

        if (damageParticlePrefab != null)
        {
            Vector3 contactPoint = playerObj.transform.position;
            GameObject particles = Instantiate(damageParticlePrefab, contactPoint, Quaternion.identity);
            Destroy(particles, 2f);
        }

        PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage, transform.position);
        }

        PlayerController playerCtrl = playerObj.GetComponent<PlayerController>();
        if (playerCtrl != null)
        {
            playerCtrl.ApplyKnockback(transform.position, knockbackForce);
        }

        Debug.Log("¡Jellyfish hizo daño al jugador!");
    }

    public void DisableBehavior()
    {
        isDead = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA.position, pointB.position);

            Vector3 center = (pointA.position + pointB.position) / 2f;
            Vector3 direction = (pointB.position - pointA.position).normalized;
            DrawArrow(center, direction, Color.yellow);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pointA.position, 0.3f);
            Gizmos.DrawSphere(pointA.position, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pointB.position, 0.3f);
            Gizmos.DrawSphere(pointB.position, 0.1f);

            // Mostrar rango de detección de llegada
            if (Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Vector3 currentTarget = movingToB ? pointB.position : pointA.position;
                Gizmos.DrawWireSphere(currentTarget, waypointReachDistance);
            }

#if UNITY_EDITOR
            float distance = Vector3.Distance(pointA.position, pointB.position);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            UnityEditor.Handles.Label(
                center + Vector3.up * 0.5f,
                $"Dist: {distance:F1}u\nÁngulo: {angle:F0}°"
            );
#endif
        }
        else
        {
            Gizmos.color = Color.yellow;
            Vector3 left = transform.position + Vector3.left * 3f;
            Vector3 right = transform.position + Vector3.right * 3f;
            Gizmos.DrawLine(left, right);
            Gizmos.DrawWireSphere(left, 0.2f);
            Gizmos.DrawWireSphere(right, 0.2f);
        }
    }

    private void DrawArrow(Vector3 position, Vector3 direction, Color color)
    {
        Gizmos.color = color;

        Vector3 arrowEnd = position + direction * 0.5f;
        Gizmos.DrawLine(position, arrowEnd);

        Vector3 right = Quaternion.Euler(0, 0, 150) * direction * 0.3f;
        Vector3 left = Quaternion.Euler(0, 0, -150) * direction * 0.3f;

        Gizmos.DrawLine(arrowEnd, arrowEnd + right);
        Gizmos.DrawLine(arrowEnd, arrowEnd + left);
    }
}
