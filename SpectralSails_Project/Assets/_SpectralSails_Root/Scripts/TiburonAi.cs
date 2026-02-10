using UnityEngine;

public class TiburonAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player; // Arrastra aquí el jugador desde el Inspector

    [Header("Configuración de Movimiento")]
    public float detectionRange = 10f; // Rango para detectar al jugador
    public float attackRange = 2f; // Rango para atacar
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    [Header("Configuración de Ataque")]
    public float attackCooldown = 2f; // Tiempo entre ataques
    public int attackDamage = 1;

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

        // Rotar hacia el jugador
        RotateTowards(player.position);

        animator.SetBool("IsMoving", true);
    }

    private void HandleAttacking()
    {
        // Dejar de moverse y atacar
        animator.SetBool("IsMoving", false);

        // Mirar al jugador
        RotateTowards(player.position);

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

    private void RotateTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0; // Mantener rotación solo en el plano horizontal

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
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