using UnityEngine;
using System.Collections;

public class BossPirate : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 20;
    private int currentHealth;

    [Header("Phase Control")]
    public bool isEnraged = false;
    [SerializeField] private float enrageHealthThreshold = 0.5f; // 50%

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float enragedMoveSpeed = 3f;
    public float chaseRange = 6f;
    public float stopDistance = 2f;

    [Header("Sword Attack")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public int swordDamage = 1;
    public float swordCooldown = 2f;
    private float swordTimer = 0f;

    [Header("Barrel Attack (Fase Normal)")]
    public GameObject barrelPrefab;
    public Transform barrelSpawnPoint;
    public float barrelCooldown = 4f;
    private float barrelTimer = 0f;

    [Header("Ghost Orb Attack (Fase Enraged)")]
    public GameObject ghostOrbPrefab;
    public Transform leftHandSpawn;
    public Transform rightHandSpawn;
    public float ghostOrbCooldown = 3f;
    public float ghostOrbRange = 8f;
    private float ghostOrbTimer = 0f;

    [Header("Ghost Mouth Attack (Fase Enraged)")]
    public GameObject ghostMouthPrefab;
    public Transform mouthSpawnPoint;
    public float ghostMouthCooldown = 4f;
    public float ghostMouthMinRange = 3f;
    public float ghostMouthMaxRange = 6f;
    private float ghostMouthTimer = 0f;

    [Header("Sprites y Animaciones")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private RuntimeAnimatorController normalAnimatorController;
    [SerializeField] private RuntimeAnimatorController enragedAnimatorController;

    [Header("Efectos de Transformación")]
    [SerializeField] private GameObject transformationEffect; // Partículas
    [SerializeField] private AudioClip transformationSound;
    [SerializeField] private float transformationDuration = 2f;
    [SerializeField] private Color enragedTint = new Color(1f, 0.5f, 0.5f); // Tinte rojo

    [Header("UI")]
    public BossHealthUI bossHealthUI;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    private Transform player;
    private Rigidbody2D rb;
    private Vector2 moveDirection = Vector2.zero;
    private bool isTransforming = false;
    private bool canAttack = true;

    private void Start()
    {
        currentHealth = maxHealth;

        // Obtener componentes
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null)
            animator = GetComponent<Animator>();

        // Buscar jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Configurar UI
        if (bossHealthUI != null)
        {
            bossHealthUI.Show();
            bossHealthUI.SetHealth(1f);
        }
    }

    private void Update()
    {
        if (isTransforming) return;

        HandleMovementLogic();
        UpdateTimers();
        HandleAttacks();
    }

    private void FixedUpdate()
    {
        if (!isTransforming)
            HandleMovementPhysics();
    }

    private void UpdateTimers()
    {
        swordTimer -= Time.deltaTime;
        barrelTimer -= Time.deltaTime;
        ghostOrbTimer -= Time.deltaTime;
        ghostMouthTimer -= Time.deltaTime;
    }

    private void HandleMovementLogic()
    {
        if (player == null)
        {
            moveDirection = Vector2.zero;
            return;
        }

        float distanceX = Mathf.Abs(transform.position.x - player.position.x);

        // Demasiado lejos → idle
        if (distanceX > chaseRange)
        {
            moveDirection = Vector2.zero;
            UpdateAnimation(0f);
            return;
        }

        // Muy cerca → parar
        if (distanceX <= stopDistance)
        {
            moveDirection = Vector2.zero;
            UpdateAnimation(0f);
            return;
        }

        // En rango de persecución → moverse hacia el jugador
        Vector2 direction = new Vector2(player.position.x - transform.position.x, 0).normalized;
        moveDirection = direction;

        // Voltear sprite
        if (direction.x != 0)
            transform.localScale = new Vector3(direction.x > 0 ? 1 : -1, 1, 1);

        UpdateAnimation(1f);
    }

    private void HandleMovementPhysics()
    {
        if (moveDirection != Vector2.zero)
        {
            float currentSpeed = isEnraged ? enragedMoveSpeed : moveSpeed;
            Vector2 newPos = rb.position + moveDirection * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }

    private void HandleAttacks()
    {
        if (!canAttack || player == null) return;

        float distanceX = Mathf.Abs(transform.position.x - player.position.x);

        if (isEnraged)
        {
            HandleEnragedAttacks(distanceX);
        }
        else
        {
            HandleNormalAttacks(distanceX);
        }
    }

    private void HandleNormalAttacks(float distanceX)
    {
        // Espada si está cerca
        if (distanceX <= stopDistance && swordTimer <= 0f)
        {
            PerformSwordAttack();
        }
        // Barril si está lejos
        else if (distanceX > stopDistance && barrelTimer <= 0f)
        {
            PerformBarrelAttack();
        }
    }

    private void HandleEnragedAttacks(float distanceX)
    {
        // Espada si está MUY cerca
        if (distanceX <= stopDistance && swordTimer <= 0f)
        {
            PerformSwordAttack();
        }
        // Ghost Orb si está LEJOS
        else if (distanceX >= ghostOrbRange && ghostOrbTimer <= 0f)
        {
            PerformGhostOrbAttack();
        }
        // Ghost Mouth si está a DISTANCIA MEDIA
        else if (distanceX > ghostMouthMinRange && distanceX < ghostMouthMaxRange && ghostMouthTimer <= 0f)
        {
            PerformGhostMouthAttack();
        }
    }

    private void PerformSwordAttack()
    {
        swordTimer = isEnraged ? swordCooldown * 0.7f : swordCooldown;

        // Activar animación
        animator?.SetTrigger("SwordAttack");

        Debug.Log("Boss atacó con ESPADA");

        // El daño se aplicará desde un Animation Event
        // Ver método DealSwordDamage() más abajo
    }

    private void PerformBarrelAttack()
    {
        barrelTimer = barrelCooldown;

        // Activar animación
        animator?.SetTrigger("BarrelAttack");

        Debug.Log("Boss lanzó BARRIL");

        // El barril se instanciará desde un Animation Event
        // Ver método SpawnBarrel() más abajo
    }

    private void PerformGhostOrbAttack()
    {
        ghostOrbTimer = ghostOrbCooldown;

        // Activar animación
        animator?.SetTrigger("GhostOrbAttack");

        Debug.Log("Boss lanzó ORBES FANTASMA");

        // Los orbes se instanciarán desde un Animation Event
        // Ver método SpawnGhostOrbs() más abajo
    }

    private void PerformGhostMouthAttack()
    {
        ghostMouthTimer = ghostMouthCooldown;

        // Activar animación
        animator?.SetTrigger("GhostMouthAttack");

        Debug.Log("Boss lanzó BOCA FANTASMA");

        // La boca se instanciará desde un Animation Event
        // Ver método SpawnGhostMouth() más abajo
    }

    // ========================================
    // MÉTODOS PARA ANIMATION EVENTS
    // ========================================

    /// <summary>
    /// Llamar desde Animation Event en el frame del golpe de espada
    /// </summary>
    public void DealSwordDamage()
    {
        if (attackPoint == null) return;

        float range = isEnraged ? attackRange + 0.5f : attackRange;
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, range, LayerMask.GetMask("Player"));

        if (hit != null)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                int damage = isEnraged ? swordDamage + 1 : swordDamage;
                playerHealth.TakeDamage(damage, transform.position);
                Debug.Log($"Boss golpeó al jugador con espada. Daño: {damage}");
            }
        }
    }

    /// <summary>
    /// Llamar desde Animation Event cuando lanza el barril
    /// </summary>
    public void SpawnBarrel()
    {
        if (barrelPrefab == null || barrelSpawnPoint == null) return;

        float direction = player.position.x > transform.position.x ? 1f : -1f;

        GameObject barrel = Instantiate(barrelPrefab, barrelSpawnPoint.position, Quaternion.identity);
        Barrel barrelScript = barrel.GetComponent<Barrel>();
        if (barrelScript != null)
            barrelScript.SetDirection(direction);
    }

    /// <summary>
    /// Llamar desde Animation Event cuando lanza los orbes
    /// </summary>
    public void SpawnGhostOrbs()
    {
        if (ghostOrbPrefab == null || player == null) return;

        Vector3 targetPos = player.position;

        // Orbe desde mano izquierda
        if (leftHandSpawn != null)
        {
            GameObject orbLeft = Instantiate(ghostOrbPrefab, leftHandSpawn.position, Quaternion.identity);
            GhostOrb orbLeftScript = orbLeft.GetComponent<GhostOrb>();
            if (orbLeftScript != null)
                orbLeftScript.Initialize(targetPos);
        }

        // Orbe desde mano derecha
        if (rightHandSpawn != null)
        {
            GameObject orbRight = Instantiate(ghostOrbPrefab, rightHandSpawn.position, Quaternion.identity);
            GhostOrb orbRightScript = orbRight.GetComponent<GhostOrb>();
            if (orbRightScript != null)
                orbRightScript.Initialize(targetPos);
        }
    }

    /// <summary>
    /// Llamar desde Animation Event cuando lanza la boca
    /// </summary>
    public void SpawnGhostMouth()
    {
        if (ghostMouthPrefab == null || mouthSpawnPoint == null || player == null) return;

        Vector3 targetPos = player.position;

        GameObject mouth = Instantiate(ghostMouthPrefab, mouthSpawnPoint.position, Quaternion.identity);
        GhostMouth mouthScript = mouth.GetComponent<GhostMouth>();
        if (mouthScript != null)
            mouthScript.Initialize(targetPos);
    }

    // ========================================
    // SISTEMA DE DAÑO Y TRANSFORMACIÓN
    // ========================================

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Actualizar UI
        if (bossHealthUI != null)
        {
            float normalized = (float)currentHealth / maxHealth;
            bossHealthUI.SetHealth(normalized);
        }

        // Reproducir animación de golpe
        animator?.SetTrigger("Hit");

        // Verificar transformación
        float healthPercentage = (float)currentHealth / maxHealth;
        if (!isEnraged && healthPercentage <= enrageHealthThreshold)
        {
            StartCoroutine(EnterEnragedPhase());
        }

        // Verificar muerte
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator EnterEnragedPhase()
    {
        isEnraged = true;
        isTransforming = true;
        canAttack = false;

        Debug.Log("¡BOSS ENTRA EN FASE ENRAGED!");

        // Detener movimiento
        rb.linearVelocity = Vector2.zero;
        moveDirection = Vector2.zero;

        // Activar animación de transformación
        animator?.SetTrigger("Transform");

        // Efecto de partículas
        if (transformationEffect != null)
        {
            GameObject effect = Instantiate(transformationEffect, transform.position, Quaternion.identity, transform);
            Destroy(effect, transformationDuration);
        }

        // Sonido
        if (transformationSound != null)
        {
            AudioSource.PlayClipAtPoint(transformationSound, transform.position);
        }

        // Cambiar tinte del sprite progresivamente
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            float elapsed = 0f;

            while (elapsed < transformationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transformationDuration;
                spriteRenderer.color = Color.Lerp(originalColor, enragedTint, t);
                yield return null;
            }
        }

        // Cambiar Animator Controller si está configurado
        if (enragedAnimatorController != null && animator != null)
        {
            animator.runtimeAnimatorController = enragedAnimatorController;
        }

        // Esperar a que termine la animación
        yield return new WaitForSeconds(transformationDuration);

        isTransforming = false;
        canAttack = true;

        Debug.Log("Transformación completada. ¡BOSS ENRAGED!");
    }

    private void UpdateAnimation(float speed)
    {
        if (animator != null)
            animator.SetFloat("Speed", speed);
    }

    private void Die()
    {
        Debug.Log("¡BOSS DERROTADO!");

        // Desactivar comportamiento
        canAttack = false;
        rb.linearVelocity = Vector2.zero;

        // Animación de muerte
        animator?.SetTrigger("Death");

        // Ocultar UI
        if (bossHealthUI != null)
            bossHealthUI.Hide();

        // Destruir después de la animación
        Destroy(gameObject, 3f);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // Rango de espada
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);

            if (isEnraged)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(attackPoint.position, attackRange + 0.5f);
            }
        }

        // Rangos de ataque
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Rangos de ataques especiales (enraged)
        if (isEnraged)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, ghostMouthMinRange);
            Gizmos.DrawWireSphere(transform.position, ghostMouthMaxRange);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, ghostOrbRange);
        }
    }
}