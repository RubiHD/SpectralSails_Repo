using UnityEngine;
using System.Collections;

public class BossPirate : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 20;
    private int currentHealth;

    [Header("Phase Control")]
    public bool isEnraged = false;
    [SerializeField] private float enrageHealthThreshold = 0.5f;

    [Header("Movement")]
    public float moveSpeed = 1.2f; // ✅ Reducido de 2f
    public float enragedMoveSpeed = 1.8f; // ✅ Reducido de 3f
    public float chaseRange = 8f; // ✅ Aumentado de 6f
    public float stopDistance = 2.5f; // ✅ Aumentado de 2f

    [Header("⭐ PAUSAS ESTRATÉGICAS")]
    public bool usePauses = true;
    public float pauseDuration = 1.5f; // Tiempo que se queda quieto
    public float pauseInterval = 3f; // Cada cuánto tiempo pausa
    private float pauseTimer = 0f;
    private bool isPaused = false;

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
    [SerializeField] private GameObject transformationEffect;
    [SerializeField] private AudioClip transformationSound;
    [SerializeField] private float transformationDuration = 2f;
    [SerializeField] private Color enragedTint = new Color(1f, 0.5f, 0.5f);

    [Header("👻 CONFIGURACIÓN FANTASMA")]
    public bool isGhost = true; // ✅ NUEVO - El boss es atravesable
    public float ghostAlpha = 0.7f; // ✅ NUEVO - Transparencia

    [Header("UI")]
    public BossHealthUI bossHealthUI;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    private Transform player;
    private Rigidbody2D rb;
    private Collider2D mainCollider;
    private Vector2 moveDirection = Vector2.zero;
    private bool isTransforming = false;
    private bool canAttack = true;
    private bool isAttacking = false; // ✅ NUEVO - Para evitar movimiento durante ataque

    private void Start()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<Collider2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null)
            animator = GetComponent<Animator>();

        // ✅ CONFIGURAR COMO FANTASMA
        if (isGhost)
        {
            SetupAsGhost();
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (bossHealthUI != null)
        {
            bossHealthUI.Show();
            bossHealthUI.SetHealth(1f);
        }

        pauseTimer = pauseInterval;
    }

    // ✅ NUEVO MÉTODO: Configurar como fantasma
    private void SetupAsGhost()
    {
        // Hacer el collider atravesable (trigger)
        if (mainCollider != null)
        {
            mainCollider.isTrigger = true;
        }

        // Aplicar transparencia
        if (spriteRenderer != null)
        {
            Color currentColor = spriteRenderer.color;
            spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, ghostAlpha);
        }

        // Configurar Rigidbody2D para que no empuje al jugador
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Debug.Log("Boss configurado como fantasma (atravesable)");
    }

    private void Update()
    {
        if (isTransforming) return;

        UpdateTimers();
        HandlePauses(); // ✅ NUEVO
        HandleMovementLogic();
        HandleAttacks();
    }

    private void FixedUpdate()
    {
        if (!isTransforming && !isPaused && !isAttacking)
            HandleMovementPhysics();
    }

    private void UpdateTimers()
    {
        swordTimer -= Time.deltaTime;
        barrelTimer -= Time.deltaTime;
        ghostOrbTimer -= Time.deltaTime;
        ghostMouthTimer -= Time.deltaTime;

        // ✅ Timer de pausas
        if (usePauses && !isPaused && !isAttacking)
        {
            pauseTimer -= Time.deltaTime;
        }
    }

    // ✅ NUEVO MÉTODO: Sistema de pausas
    private void HandlePauses()
    {
        if (!usePauses || isAttacking) return;

        if (pauseTimer <= 0f && !isPaused)
        {
            StartCoroutine(PauseMovement());
        }
    }

    private IEnumerator PauseMovement()
    {
        isPaused = true;
        moveDirection = Vector2.zero;
        UpdateAnimation(0f);

        Debug.Log("Boss en pausa estratégica");

        yield return new WaitForSeconds(pauseDuration);

        isPaused = false;
        pauseTimer = pauseInterval;

        Debug.Log("Boss reanuda movimiento");
    }

    private void HandleMovementLogic()
    {
        if (player == null || isPaused || isAttacking)
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
        if (!canAttack || player == null || isPaused) return;

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
        else if (distanceX > stopDistance && distanceX <= chaseRange && barrelTimer <= 0f)
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
        else if (distanceX >= ghostMouthMinRange && distanceX <= ghostMouthMaxRange && ghostMouthTimer <= 0f)
        {
            PerformGhostMouthAttack();
        }
    }

    private void PerformSwordAttack()
    {
        swordTimer = isEnraged ? swordCooldown * 0.7f : swordCooldown;
        StartCoroutine(AttackRoutine("SwordAttack", 0.8f)); // ✅ Usar coroutine
    }

    private void PerformBarrelAttack()
    {
        barrelTimer = barrelCooldown;
        StartCoroutine(AttackRoutine("BarrelAttack", 1.0f)); // ✅ Usar coroutine
    }

    private void PerformGhostOrbAttack()
    {
        ghostOrbTimer = ghostOrbCooldown;
        StartCoroutine(AttackRoutine("GhostOrbAttack", 1.0f)); // ✅ Usar coroutine
    }

    private void PerformGhostMouthAttack()
    {
        ghostMouthTimer = ghostMouthCooldown;
        StartCoroutine(AttackRoutine("GhostMouthAttack", 1.2f)); // ✅ Usar coroutine
    }

    // ✅ NUEVO: Coroutine para bloquear movimiento durante ataque
    private IEnumerator AttackRoutine(string triggerName, float duration)
    {
        isAttacking = true;
        moveDirection = Vector2.zero;
        UpdateAnimation(0f);

        animator?.SetTrigger(triggerName);
        Debug.Log($"Boss ejecutó: {triggerName}");

        yield return new WaitForSeconds(duration);

        isAttacking = false;
    }

    // ========================================
    // MÉTODOS PARA ANIMATION EVENTS
    // ========================================

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

    public void SpawnBarrel()
    {
        if (barrelPrefab == null || barrelSpawnPoint == null || player == null) return;

        float direction = player.position.x > transform.position.x ? 1f : -1f;

        GameObject barrel = Instantiate(barrelPrefab, barrelSpawnPoint.position, Quaternion.identity);
        Barrel barrelScript = barrel.GetComponent<Barrel>();
        if (barrelScript != null)
            barrelScript.SetDirection(direction);
    }

    public void SpawnGhostOrbs()
    {
        if (ghostOrbPrefab == null || player == null) return;

        Vector3 targetPos = player.position;

        if (leftHandSpawn != null)
        {
            GameObject orbLeft = Instantiate(ghostOrbPrefab, leftHandSpawn.position, Quaternion.identity);
            GhostOrb orbLeftScript = orbLeft.GetComponent<GhostOrb>();
            if (orbLeftScript != null)
                orbLeftScript.Initialize(targetPos);
        }

        if (rightHandSpawn != null)
        {
            GameObject orbRight = Instantiate(ghostOrbPrefab, rightHandSpawn.position, Quaternion.identity);
            GhostOrb orbRightScript = orbRight.GetComponent<GhostOrb>();
            if (orbRightScript != null)
                orbRightScript.Initialize(targetPos);
        }
    }

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

        if (bossHealthUI != null)
        {
            float normalized = (float)currentHealth / maxHealth;
            bossHealthUI.SetHealth(normalized);
        }

        animator?.SetTrigger("Hit");

        float healthPercentage = (float)currentHealth / maxHealth;
        if (!isEnraged && healthPercentage <= enrageHealthThreshold)
        {
            StartCoroutine(EnterEnragedPhase());
        }

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

        rb.linearVelocity = Vector2.zero;
        moveDirection = Vector2.zero;

        animator?.SetTrigger("Transform");

        if (transformationEffect != null)
        {
            GameObject effect = Instantiate(transformationEffect, transform.position, Quaternion.identity, transform);
            Destroy(effect, transformationDuration);
        }

        if (transformationSound != null)
        {
            AudioSource.PlayClipAtPoint(transformationSound, transform.position);
        }

        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            float elapsed = 0f;

            while (elapsed < transformationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transformationDuration;

                // ✅ Mantener transparencia de fantasma
                Color targetColor = new Color(enragedTint.r, enragedTint.g, enragedTint.b, ghostAlpha);
                spriteRenderer.color = Color.Lerp(originalColor, targetColor, t);

                yield return null;
            }
        }

        if (enragedAnimatorController != null && animator != null)
        {
            animator.runtimeAnimatorController = enragedAnimatorController;
        }

        yield return new WaitForSeconds(transformationDuration);

        isTransforming = false;
        canAttack = true;
        pauseTimer = pauseInterval; // ✅ Reiniciar timer de pausas

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

        canAttack = false;
        rb.linearVelocity = Vector2.zero;

        animator?.SetTrigger("Death");

        if (bossHealthUI != null)
            bossHealthUI.Hide();

        Destroy(gameObject, 3f);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

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