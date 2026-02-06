using UnityEngine;

public class EnemyFollowControl : MonoBehaviour
{
    public Transform player;
    public float detectionRadius = 5.0f;
    public float attackRange = 0.8f;
    public float speed = 0.3f;
    public int damage = 1;
    public float attackCooldown = 1.5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private float lastAttackTime;
    private bool isDead = false;
    private bool isAttacking = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (isDead || isAttacking || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            movement = Vector2.zero;

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }

            animator?.SetBool("isWalking", false);
        }
        else if (distanceToPlayer <= detectionRadius)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            movement = new Vector2(direction.x, 0);

            animator?.SetBool("isWalking", true);

            if (direction.x != 0)
                transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
        }
        else
        {
            movement = Vector2.zero;
            animator?.SetBool("isWalking", false);
        }
    }

    private void FixedUpdate()
    {
        if (!isDead && !isAttacking)
        {
            Vector2 newPosition = rb.position + movement * speed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
    }

    private void Attack()
    {
        animator?.SetTrigger("Attack");
        isAttacking = true;

        // Retroceso leve para evitar que se quede pegado
        Vector2 pushBack = (transform.position - player.position).normalized * 0.2f;
        rb.MovePosition(rb.position + pushBack);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage, transform.position);
            }
        }

        Invoke(nameof(EndAttack), 0.5f); // Ajusta según la animación
    }

    private void EndAttack()
    {
        isAttacking = false;
    }

    public void DisableBehavior()
    {
        isDead = true;
        movement = Vector2.zero;

        // Detener completamente el Rigidbody2D
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Static; // Esto congela el cuerpo por completo

        animator?.SetBool("isWalking", false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
