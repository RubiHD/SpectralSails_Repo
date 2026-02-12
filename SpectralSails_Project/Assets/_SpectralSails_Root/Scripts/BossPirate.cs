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
    public float uiActivationRange = 10f; // ✅ NUEVO - Rango para mostrar UI
    private bool hasShownUI = false; // ✅ NUEVO - Control de primera aparición

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

        if (isGhost)
        {
            SetupAsGhost();
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // ✅ CAMBIAR ESTO: UI empieza oculta
        if (bossHealthUI != null)
        {
            bossHealthUI.Hide(); // ✅ Oculta al inicio
            bossHealthUI.SetHealth(1f);
        }

        pauseTimer = pauseInterval;
    }

    // ✅ NUEVO MÉTODO: Configurar como fantasma
    private void SetupAsGhost()
    {
        // ✅ SOLUCIÓN: Usar un collider separado para el ataque
        // El collider principal es trigger (atravesable)
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

        // Configurar Rigidbody2D
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
        HandlePauses();
        HandleUIVisibility(); // ✅ NUEVO - Verificar si mostrar/ocultar UI
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
        Debug.Log("=== DEAL SWORD DAMAGE LLAMADO ===");

        if (attackPoint == null)
        {
            Debug.LogError("❌ attackPoint NO está asignado!");
            return;
        }

        float range = isEnraged ? attackRange + 0.5f : attackRange;
        Debug.Log($"Buscando jugador en rango {range} desde posición {attackPoint.position}");

        // ✅ VERSIÓN 1: Buscar por capa "Player"
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, range, LayerMask.GetMask("Player"));

        if (hit != null)
        {
            Debug.Log($"✅ Collider encontrado: {hit.name} en capa {LayerMask.LayerToName(hit.gameObject.layer)}");

            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                int damage = isEnraged ? swordDamage + 1 : swordDamage;
                playerHealth.TakeDamage(damage, transform.position);
                Debug.Log($"✅ Daño aplicado: {damage} a {hit.name}");
            }
            else
            {
                Debug.LogError($"❌ {hit.name} NO tiene componente PlayerHealth!");
            }
        }
        else
        {
            Debug.LogWarning("❌ NO se encontró ningún collider en el rango");

            // ✅ VERSIÓN 2 (FALLBACK): Buscar por tag
            Collider2D[] allColliders = Physics2D.OverlapCircleAll(attackPoint.position, range);
            Debug.Log($"Colliders totales encontrados: {allColliders.Length}");

            foreach (var col in allColliders)
            {
                Debug.Log($"  - {col.name} (Layer: {LayerMask.LayerToName(col.gameObject.layer)}, Tag: {col.tag})");

                if (col.CompareTag("Player"))
                {
                    Debug.Log("✅ Jugador encontrado por TAG!");
                    PlayerHealth ph = col.GetComponent<PlayerHealth>();
                    if (ph != null)
                    {
                        int damage = isEnraged ? swordDamage + 1 : swordDamage;
                        ph.TakeDamage(damage, transform.position);
                        Debug.Log($"✅ Daño aplicado por TAG: {damage}");
                        return;
                    }
                }
            }
        }
    }

    // ✅ MÉTODO CORREGIDO PARA BossPirate.cs
    // Reemplaza el método SpawnBarrel() existente

    public void SpawnBarrel()
    {
        Debug.Log("=== SPAWN BARREL LLAMADO ===");

        // ✅ VERIFICACIÓN 1: Prefab asignado
        if (barrelPrefab == null)
        {
            Debug.LogError("❌ barrelPrefab no está asignado en el Inspector!");
            return;
        }
        Debug.Log("✅ barrelPrefab asignado correctamente");

        // ✅ VERIFICACIÓN 2: Spawn point asignado
        if (barrelSpawnPoint == null)
        {
            Debug.LogError("❌ barrelSpawnPoint no está asignado en el Inspector!");
            return;
        }
        Debug.Log($"✅ barrelSpawnPoint asignado: {barrelSpawnPoint.position}");

        // ✅ VERIFICACIÓN 3: Jugador existe
        if (player == null)
        {
            Debug.LogError("❌ Player es null!");
            return;
        }
        Debug.Log($"✅ Player encontrado en: {player.position}");

        // ✅ CALCULAR DIRECCIÓN
        float direction = player.position.x > transform.position.x ? 1f : -1f;
        Debug.Log($"Dirección calculada: {direction} (Player X: {player.position.x}, Boss X: {transform.position.x})");

        // ✅ INSTANCIAR BARRIL
        GameObject barrel = Instantiate(barrelPrefab, barrelSpawnPoint.position, Quaternion.identity);

        if (barrel == null)
        {
            Debug.LogError("❌ Instantiate devolvió null!");
            return;
        }
        Debug.Log($"✅ Barril instanciado: {barrel.name} en posición {barrel.transform.position}");

        // ✅ CONFIGURAR DIRECCIÓN
        BarrelSimple barrelScript = barrel.GetComponent<BarrelSimple>();

        if (barrelScript == null)
        {
            Debug.LogError("❌ El prefab no tiene el componente Barrel!");
            Destroy(barrel);
            return;
        }

        Debug.Log("✅ Componente Barrel encontrado");
        barrelScript.SetDirection(direction);
        Debug.Log($"✅ Dirección {direction} aplicada al barril");

        Debug.Log("=== SPAWN BARREL COMPLETADO ===");
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

    // ✅ MÉTODO CORREGIDO PARA BossPirate.cs
    // Reemplaza el método SpawnGhostMouth() existente

    // ✅ MÉTODO SIMPLIFICADO PARA BossPirate.cs
    // Reemplaza tu método SpawnGhostMouth() actual

    public void SpawnGhostMouth()
    {
        if (ghostMouthPrefab == null || mouthSpawnPoint == null || player == null)
        {
            Debug.LogError("GhostMouth: Faltan referencias!");
            return;
        }

        Debug.Log("=== SPAWN GHOST MOUTH ===");

        // ✅ Obtener dirección del boss (1 = derecha, -1 = izquierda)
        float bossDirection = Mathf.Sign(transform.localScale.x);

        Debug.Log($"Boss mira hacia: {(bossDirection > 0 ? "DERECHA" : "IZQUIERDA")}");

        // ✅ Spawn en la posición del spawn point (o delante del boss si prefieres)
        Vector3 spawnPosition = mouthSpawnPoint.position;

        // O si quieres que aparezca delante del boss:
        // Vector3 spawnPosition = transform.position + new Vector3(bossDirection * 2f, 0f, 0f);

        Debug.Log($"Spawn position: {spawnPosition}");

        // ✅ Instanciar SIN rotación (Quaternion.identity)
        GameObject mouth = Instantiate(ghostMouthPrefab, spawnPosition, Quaternion.identity);

        GhostMouth mouthScript = mouth.GetComponent<GhostMouth>();
        if (mouthScript != null)
        {
            // ✅ Solo pasar la dirección para el flip en X
            mouthScript.Initialize(bossDirection);
            Debug.Log("✅ GhostMouth inicializado con flip en X");
        }
        else
        {
            Debug.LogError("❌ El prefab no tiene componente GhostMouth!");
        }
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

    // ✅ NUEVO MÉTODO: Mostrar/ocultar UI según distancia
    private void HandleUIVisibility()
    {
        if (player == null || bossHealthUI == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Mostrar UI si el jugador está cerca
        if (distanceToPlayer <= uiActivationRange && !hasShownUI)
        {
            bossHealthUI.Show();
            hasShownUI = true;
            Debug.Log("¡Barra de vida del boss activada!");
        }
        // Mantener visible mientras esté en rango
        else if (distanceToPlayer > uiActivationRange && hasShownUI)
        {
            // Opcional: ocultar si el jugador se aleja mucho
            // bossHealthUI.Hide();
            // hasShownUI = false;
        }
    }

    private void Die()
    {
        Debug.Log("¡BOSS DERROTADO!");

        canAttack = false;
        isAttacking = false;
        rb.linearVelocity = Vector2.zero;

        // Ocultar UI inmediatamente
        if (bossHealthUI != null)
            bossHealthUI.Hide();

        // Iniciar efecto de muerte
        StartCoroutine(DeathEffect());
    }

    // ✅ NUEVO: Efecto de muerte visual sin animación
    private IEnumerator DeathEffect()
    {
        float duration = 2f;
        float elapsed = 0f;

        // Obtener componentes
        SpriteRenderer sprite = spriteRenderer;
        Vector3 originalScale = transform.localScale;
        Color originalColor = sprite != null ? sprite.color : Color.white;

        // ✅ EFECTO 1: Partículas de desvanecimiento (opcional)
        GameObject particles = CreateDissolveParticles();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // ✅ EFECTO 2: Fade out (transparencia)
            if (sprite != null)
            {
                Color newColor = originalColor;
                newColor.a = Mathf.Lerp(originalColor.a, 0f, t);
                sprite.color = newColor;
            }

            // ✅ EFECTO 3: Escala reducida (se encoge)
            float scaleMultiplier = Mathf.Lerp(1f, 0.5f, t);
            transform.localScale = originalScale * scaleMultiplier;

            // ✅ EFECTO 4: Elevación lenta (flota hacia arriba)
            transform.position += Vector3.up * Time.deltaTime * 0.5f;

            // ✅ EFECTO 5: Rotación lenta
            transform.Rotate(0, 0, 50f * Time.deltaTime);

            yield return null;
        }

        // Destruir partículas si existen
        if (particles != null)
            Destroy(particles);

        // Destruir el boss
        Destroy(gameObject);
    }

    // ✅ NUEVO: Crear partículas de disolución
    private GameObject CreateDissolveParticles()
    {
        GameObject particlesObj = new GameObject("BossDissolveEffect");
        particlesObj.transform.position = transform.position;
        particlesObj.transform.SetParent(transform);

        ParticleSystem ps = particlesObj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration = 2f;
        main.loop = false;
        main.startLifetime = 1.5f;
        main.startSpeed = 2f;
        main.startSize = 0.3f;
        main.startColor = new Color(0.5f, 0f, 1f, 1f); // Púrpura fantasmal
        main.gravityModifier = -0.5f; // Flotar hacia arriba
        main.maxParticles = 50;

        var emission = ps.emission;
        emission.rateOverTime = 25;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
            new GradientColorKey(new Color(0.5f, 0f, 1f), 0f),      // Púrpura
            new GradientColorKey(new Color(1f, 1f, 1f), 0.5f),      // Blanco
            new GradientColorKey(new Color(0.5f, 0f, 1f), 1f)       // Púrpura
            },
            new GradientAlphaKey[] {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(0.5f, 0.5f),
            new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        ps.Play();

        return particlesObj;
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