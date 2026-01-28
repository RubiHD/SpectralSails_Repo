using UnityEngine;

public class BossPirate : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 20;
    private int currentHealth;

    [Header("Phase Control")]
    public bool isEnraged = false;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float chaseRange = 6f;
    public float stopDistance = 2f;

    [Header("Barrel Attack")]
    public GameObject barrelPrefab;
    public Transform barrelSpawnPoint;
    public float barrelCooldown = 4f;

    private float barrelTimer = 0f;

    [Header("Sword Attack")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public int swordDamage = 1;
    public float swordCooldown = 2f;

    private float swordTimer = 0f;



    private Transform player;
    private Rigidbody2D rb;

    private Vector2 moveDirection = Vector2.zero;

    [Header("Health Bar")]
    public BossHealthUI bossHealthUI;


    private void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();

        bossHealthUI.Show();
        bossHealthUI.SetHealth(1f); // vida completa

    }

    private void Update()
    {
        HandleMovementLogic();   // ← ESTO FALTABA

        swordTimer -= Time.deltaTime;
        barrelTimer -= Time.deltaTime;

        float distanceX = Mathf.Abs(transform.position.x - player.position.x);

        // If player is close → ONLY sword attack
        if (distanceX <= stopDistance)
        {
            barrelTimer = Mathf.Max(barrelTimer, 1f);

            if (swordTimer <= 0f)
            {
                SwordAttack();
                swordTimer = swordCooldown;
            }

            return;
        }

        // If player is far → ONLY barrel attack
        if (barrelTimer <= 0f)
        {
            BarrelAttack();
            barrelTimer = barrelCooldown;
        }
    }


    private void FixedUpdate()
    {
        HandleMovementPhysics();
    }

    private void HandleMovementLogic()
    {
        if (player == null)
        {
            moveDirection = Vector2.zero;
            return;
        }

        float distanceX = Mathf.Abs(transform.position.x - player.position.x);

        // Player too far → idle
        if (distanceX > chaseRange)
        {
            moveDirection = Vector2.zero;
            return;
        }

        // Player close enough → stop
        if (distanceX <= stopDistance)
        {
            moveDirection = Vector2.zero;
            return;
        }

        // Player in chase range → move toward player
        Vector2 direction = new Vector2(player.position.x - transform.position.x, 0).normalized;
        moveDirection = direction;
    }


    private void HandleMovementPhysics()
    {
        if (moveDirection != Vector2.zero)
        {
            Vector2 newPos = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (!isEnraged && currentHealth <= maxHealth / 2)
        {
            EnterEnragedPhase();
        }

        if (currentHealth <= 0)
        {
            Die();
        }

        float normalized = (float)currentHealth / maxHealth;
        bossHealthUI.SetHealth(normalized);

    }

    private void BarrelAttack()
    {
        float direction = player.position.x > transform.position.x ? 1f : -1f;

        GameObject barrel = Instantiate(barrelPrefab, barrelSpawnPoint.position, Quaternion.identity);
        barrel.GetComponent<Barrel>().SetDirection(direction);
    }


    private void SwordAttack()
    {
        // Animation later
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRange, LayerMask.GetMask("Player"));

        if (hit != null)
        {
            PlayerHealth player = hit.GetComponent<PlayerHealth>();
            if (player != null)
                player.TakeDamage(swordDamage);
        }
    }




    private void EnterEnragedPhase()
    {
        isEnraged = true;
        Debug.Log("Boss is now ENRAGED!");
    }

    private void Die()
    {
        Debug.Log("Boss defeated");
        Destroy(gameObject);

        bossHealthUI.Hide();

    }


    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }


}
