using UnityEngine;

public class TiburonAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player; // Arrastra aquí el jugador desde el Inspector
    public SpriteRenderer spriteRenderer; // Arrastra aquí el SpriteRenderer del tiburón

    [Header("Configuración de Movimiento")]
    public float detectionRange = 10f; // Rango para detectar al jugador
    public float attackRange = 2f; // Rango para atacar
    public float moveSpeed = 3f;

    [Header("Configuración de Ataque")]
    public float attackCooldown = 2f; // Tiempo entre ataques
    public int attackDamage = 1;

    [Header("Configuración 2D")]
    public bool facingRight = true; // ¿El sprite mira a la derecha por defecto?

    private Animator animator;
    private float lastAttackTime;
    private bool isDead = false;

    // Estados del enemigo
    private enum State
    {
        Idle,
        Chasing,
        Attacking
    }

    private State currentState = State.Idle;

    private void Start()
    {
        animator = GetComponent<Animator>();

        // Obtener SpriteRenderer si no está asignado
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("No se encontró SpriteRenderer en " + gameObject.name);
            }
        }

        // Si no asignaste el jugador en el Inspector, búscalo automáticamente
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogError("No se encontró al jugador. Asegúrate de que tenga el tag 'Player'");
        }

        lastAttackTime = -attackCooldown; // Para poder atacar inmediatamente
    }

    private void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Determinar el estado según la distancia
        if (distanceToPlayer <= attackRange)
        {
            currentState = State.Attacking;
        }
        else if (distanceToPlayer <= detectionRange)
        {
            currentState = State.Chasing;
        }
        else
        {
            currentState = State.Idle;
        }

        // Ejecutar comportamiento según el estado
        switch (currentState)
        {
            case State.Idle:
                HandleIdle();
                break;
            case State.Chasing:
                HandleChasing();
                break;
            case State.Attacking:
                HandleAttacking();
                break;
        }

        // Actualizar parámetros del Animator
        UpdateAnimator();
    }

    private void HandleIdle()
    {
        // El tiburón está en reposo
        animator.SetBool("IsMoving", false);
    }

    private void HandleChasing()
    {
        // Moverse hacia el jugador
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Voltear el sprite según la dirección (para 2D)
        FlipSprite(direction.x);

        animator.SetBool("IsMoving", true);
    }

    private void HandleAttacking()
    {
        // Dejar de moverse y atacar
        animator.SetBool("IsMoving", false);

        // Voltear sprite hacia el jugador
        Vector3 direction = (player.position - transform.position).normalized;
        FlipSprite(direction.x);

        // Atacar si ha pasado el cooldown
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    private void Attack()
    {
        // Reproducir animación de ataque
        animator.SetTrigger("Attack");

        // El daño se aplicará desde un Animation Event
        // (ver explicación más abajo)
    }

    // Este método se llama desde un Animation Event en la animación de ataque
    public void DealDamage()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            // Buscar componente de salud del jugador
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    // Voltear el sprite horizontalmente según la dirección
    private void FlipSprite(float directionX)
    {
        if (spriteRenderer == null) return;

        // Si el sprite mira a la derecha por defecto (facingRight = true)
        if (facingRight)
        {
            // Voltear si se mueve a la izquierda
            spriteRenderer.flipX = directionX < 0;
        }
        else
        {
            // Si el sprite mira a la izquierda por defecto
            // Voltear si se mueve a la derecha
            spriteRenderer.flipX = directionX > 0;
        }
    }

    private void UpdateAnimator()
    {
        // Aquí puedes añadir más parámetros si los necesitas
    }

    // Método para cuando el tiburón muere
    public void OnDeath()
    {
        isDead = true;
        animator.SetBool("IsMoving", false);

        // Desactivar el script para que no siga persiguiendo
        this.enabled = false;
    }

    // Visualización de rangos en el Editor
    private void OnDrawGizmosSelected()
    {
        // Rango de detección (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de ataque (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}