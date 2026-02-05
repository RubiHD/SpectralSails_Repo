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

    [Header("Ghost Orb Attack")]
    public GameObject ghostOrbPrefab;
    public Transform leftHandSpawn;
    public Transform rightHandSpawn;
    public float ghostOrbCooldown = 2f;

    private float ghostOrbTimer = 0f;
    public float ghostOrbRange = 8f;


    [Header("Ghost Mouth Attack")]
    public GameObject ghostMouthPrefab;
    public Transform mouthSpawnPoint;
    public float ghostMouthCooldown = 3f;
    public float ghostMouthMinRange = 3f;
    public float ghostMouthMaxRange = 6f;

    private float ghostMouthTimer = 0f;



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
        HandleMovementLogic();

        swordTimer -= Time.deltaTime;
        barrelTimer -= Time.deltaTime;
        ghostOrbTimer -= Time.deltaTime;
        ghostMouthTimer -= Time.deltaTime;

        float distanceX = Mathf.Abs(transform.position.x - player.position.x);

        if (isEnraged)
        {
            HandleEnragedAttacks(distanceX);
            return;
        }

        // Modo normal
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
        float range = isEnraged ? attackRange + 0.5f : attackRange;
        int damage = isEnraged ? swordDamage + 1 : swordDamage;

        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, range, LayerMask.GetMask("Player"));

        if (hit != null)
        {
            PlayerHealth player = hit.GetComponent<PlayerHealth>();
            if (player != null)
                player.TakeDamage(damage);
        }
    }




    private void EnterEnragedPhase()
    {
        isEnraged = true;
        Debug.Log("Boss is now ENRAGED!");
    }

    private void HandleEnragedAttacks(float distanceX)
    {
        if (distanceX <= stopDistance)
        {
            if (swordTimer <= 0f)
            {
                SwordAttack();
                swordTimer = swordCooldown * 0.7f;
            }
        }
        else if (distanceX >= ghostOrbRange)
        {
            if (ghostOrbTimer <= 0f)
            {
                GhostOrbAttack();
                ghostOrbTimer = ghostOrbCooldown;
            }
        }

        else if (distanceX > stopDistance && distanceX < ghostOrbRange)
        {
            if (ghostMouthTimer <= 0f)
            {
                GhostMouthAttack();
                ghostMouthTimer = ghostMouthCooldown;
            }
        }

        // Aquí luego puedes añadir un tercer ataque para el rango intermedio
    }

    private void GhostOrbAttack()
    {
        Vector3 targetPos = player.position;

        // Lanza desde la mano izquierda
        GameObject orbLeft = Instantiate(ghostOrbPrefab, leftHandSpawn.position, Quaternion.identity);
        orbLeft.GetComponent<GhostOrb>().Initialize(targetPos);

        // Lanza desde la mano derecha
        GameObject orbRight = Instantiate(ghostOrbPrefab, rightHandSpawn.position, Quaternion.identity);
        orbRight.GetComponent<GhostOrb>().Initialize(targetPos);
    }

    private void GhostMouthAttack()
    {
        Vector3 targetPos = player.position;

        GameObject mouth = Instantiate(ghostMouthPrefab, mouthSpawnPoint.position, Quaternion.identity);
        mouth.GetComponent<GhostMouth>().Initialize(targetPos);
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
