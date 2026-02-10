using UnityEngine;

public class SharkEnemy : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    private PlayerController playerController;

    [Header("Detección y Movimiento")]
    public float detectionRadius = 8.0f;
    public float attackRange = 1.5f;
    public float speed = 2.5f;
    public float acceleration = 5f; // Aceleración para movimiento más fluido

    [Header("Combate")]
    public int damage = 1;
    public float attackCooldown = 2f;
    public float attackDashSpeed = 6f; // Velocidad del dash de ataque
    public float attackDashDuration = 0.3f; // Duración del dash

    [Header("Patrullaje (Idle)")]
    public float idleWanderRadius = 3f;
    public float idleWanderSpeed = 0.8f;
    public float changeDirectionTime = 3f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 currentVelocity; // Para smoothing
    private Animator animator;
    private float lastAttackTime;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isDashing = false;

    // Patrullaje
    private Vector3 startPosition;
    private Vector2 wanderDirection;
    private float wanderTimer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;

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

        // Inicializar patrullaje
        wanderDirection = Random.insideUnitCircle.normalized;
        wanderTimer = changeDirectionTime;
    }

    private void Update()
    {
        if (isDead || player == null) return;

        // ⚠️ Solo atacar si el jugador está bajo el agua
        if (playerController != null && !playerController.IsUnderwater())
        {
            // Si el jugador sale del agua, volver a idle/patrullar
            IdleWander();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (isAttacking || isDashing)
        {
            // Durante ataque no hacer nada en Update
            return;
        }
        else if (distanceToPlayer <= attackRange)
        {
            // ⚔️ RANGO DE ATAQUE
            movement = Vector2.zero;

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }

            animator?.SetBool("isSwimming", false);
        }
        else if (distanceToPlayer <= detectionRadius)
        {
            // 👁️ PERSEGUIR AL JUGADOR
            Vector2 direction = (player.position - transform.position).normalized;
            movement = direction;

            animator?.SetBool("isSwimming", true);

            // Voltear según dirección horizontal
            if (direction.x != 0)
                transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
        }
        else
        {
            // 😴 PATRULLAR (IDLE)
            IdleWander();
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (isDashing)
        {
            // Durante el dash, mantener velocidad constante
            return;
        }

        if (!isAttacking)
        {
            // Movimiento suave con aceleración
            Vector2 targetVelocity = movement * speed;
            rb.linearVelocity = Vector2.SmoothDamp(
                rb.linearVelocity,
                targetVelocity,
                ref currentVelocity,
                1f / acceleration
            );
        }
        else
        {
            // Durante ataque (no dash), detenerse
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void IdleWander()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f)
        {
            // Cambiar dirección aleatoria
            wanderDirection = Random.insideUnitCircle.normalized;
            wanderTimer = changeDirectionTime;
        }

        // Mantener dentro del radio de patrullaje
        Vector2 offsetFromStart = (Vector2)transform.position - (Vector2)startPosition;
        if (offsetFromStart.magnitude > idleWanderRadius)
        {
            // Volver hacia el punto de inicio
            wanderDirection = -offsetFromStart.normalized;
        }

        movement = wanderDirection;

        animator?.SetBool("isSwimming", true);

        // Voltear según dirección
        if (wanderDirection.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(wanderDirection.x), 1, 1);

        // Velocidad reducida para patrullaje
        rb.linearVelocity = Vector2.Lerp(
            rb.linearVelocity,
            wanderDirection * idleWanderSpeed,
            acceleration * Time.deltaTime
        );
    }

    private void Attack()
    {
        animator?.SetTrigger("Attack");
        isAttacking = true;

        // ⚡ DASH DE ATAQUE hacia el jugador
        Vector2 dashDirection = (player.position - transform.position).normalized;
        StartCoroutine(AttackDashCoroutine(dashDirection));

        // El daño se aplica desde DealDamage() (Animation Event)
    }

    private System.Collections.IEnumerator AttackDashCoroutine(Vector2 direction)
    {
        isDashing = true;
        rb.linearVelocity = direction * attackDashSpeed;

        yield return new WaitForSeconds(attackDashDuration);

        // Frenar después del dash
        isDashing = false;
        rb.linearVelocity = Vector2.zero;

        // Esperar a que termine la animación de ataque
        yield return new WaitForSeconds(0.5f);

        isAttacking = false;
    }

    // ✅ LLAMADO DESDE ANIMATION EVENT
    public void DealDamage()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange * 1.5f) // Rango extendido para el dash
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                Debug.Log($"¡Tiburón golpeó al jugador! Daño: {damage}");
                ph.TakeDamage(damage, transform.position);
            }
        }
        else
        {
            Debug.Log($"Tiburón atacó pero el jugador esquivó ({distanceToPlayer:F2}m)");
        }
    }

    public void DisableBehavior()
    {
        isDead = true;
        movement = Vector2.zero;

        // Detener completamente
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Static; // ✅ Static = completamente inmóvil

        animator?.SetBool("isSwimming", false);

        // ❌ ELIMINAR ESTO:
        // StartCoroutine(FloatUpOnDeath());
    }

    private void OnDrawGizmosSelected()
    {
        // Radio de detección (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Rango de ataque (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Radio de patrullaje (azul)
        Gizmos.color = Color.cyan;
        Vector3 origin = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(origin, idleWanderRadius);
    }
}