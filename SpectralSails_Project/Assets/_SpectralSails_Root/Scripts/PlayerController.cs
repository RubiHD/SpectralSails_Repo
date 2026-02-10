using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject interactPromptUI;
    private Animator animator;

    [Header("Movimiento")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpingPower = 10f;
    private float horizontal;

    [Header("Wall Jump")]
    [SerializeField] private Vector2 wallJumpForce = new Vector2(12f, 12f);

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Dash")]
    [SerializeField] private float dashForce = 12f;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float dashCooldown = 0.5f;
    private bool isDashing = false;
    private bool canDash = true;

    [Header("🌊 MOVIMIENTO BAJO EL AGUA")]
    [SerializeField] private float waterGravity = 0.5f; // Gravedad reducida bajo el agua
    [SerializeField] private float waterJumpForce = 8f; // Fuerza del "aleteo" estilo Flappy Bird
    [SerializeField] private float waterHorizontalSpeed = 3f; // Velocidad horizontal reducida
    [SerializeField] private float waterMaxFallSpeed = -3f; // Velocidad máxima de caída
    [SerializeField] private float waterDrag = 2f; // Resistencia del agua
    [SerializeField] private LayerMask waterLayer; // Layer del agua
    private bool isUnderwater = false;
    private float normalGravity = 1f;

    [Header("Detección")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Knockback")]
    [SerializeField] private float knockbackHorizontalForce = 3f;
    [SerializeField] private float knockbackVerticalForce = 4f;
    [SerializeField] private float knockbackDuration = 0.2f;
    [SerializeField] private AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Interacción")]
    public DialogueUI dialogueUI;
    private IInteractable currentInteractable;
    public bool canMove = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    [SerializeField] private bool isGroundedDebug;
    [SerializeField] private float verticalVelocityDebug;

    // Estados
    private bool isTouchingWall;
    private bool isStickingToWall = false;
    private bool wallJumping = false;
    private bool canStickToWall = true;
    private bool isClimbingLadder = false;
    private bool hasDied = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        normalGravity = rb.gravityScale; // Guardar gravedad normal

        if (groundCheck == null)
        {
            Debug.LogError("¡GROUNDCHECK NO ASIGNADO!");
        }

        if (animator == null)
        {
            Debug.LogError("¡ANIMATOR NO ENCONTRADO!");
        }
    }

    private void Update()
    {
        if (hasDied) return;
        if (!canMove) return;

        // ✅ DEBUG COMPLETO
        if (showDebugLogs && isUnderwater)
        {
            Debug.Log($"UNDERWATER - isUnderwater: {isUnderwater}, Animator isUnderwater: {animator.GetBool("isUnderwater")}");
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"Estado actual: {state.fullPathHash}, Normalizado: {state.normalizedTime}");
        }

        UpdateAnimations();

        if (isUnderwater)
        {
            UpdateUnderwaterMovement();
            return;
        }

        // --- LÓGICA TERRESTRE NORMAL ---
        if (horizontal > 0.01f && !isStickingToWall)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (horizontal < -0.01f && !isStickingToWall)
            transform.localScale = new Vector3(-1f, 1f, 1f);

        if (isDashing || wallJumping || isClimbingLadder)
            return;

        bool grounded = IsGrounded();
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);

        isGroundedDebug = grounded;
        verticalVelocityDebug = rb.linearVelocity.y;

        if (!grounded && isTouchingWall && !wallJumping && canStickToWall)
            StickToWall();
        else if ((grounded || !isTouchingWall) && isStickingToWall)
            UnstickFromWall();

        coyoteTimeCounter = grounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;

        if (showDebugLogs)
        {
            Debug.Log($"Grounded: {grounded} | VelY: {rb.linearVelocity.y:F2}");
        }
    }

    private void FixedUpdate()
    {
        if (isDashing || wallJumping || isClimbingLadder || hasDied)
            return;

        // 🌊 Física bajo el agua
        if (isUnderwater)
        {
            ApplyUnderwaterPhysics();
            return;
        }

        // Física terrestre normal
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
    }

    // 🌊 ============================================
    // SISTEMA DE MOVIMIENTO BAJO EL AGUA
    // ============================================

    private void UpdateUnderwaterMovement()
    {
        // Voltear según dirección horizontal
        if (horizontal > 0.01f)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (horizontal < -0.01f)
            transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    private void ApplyUnderwaterPhysics()
    {
        // Movimiento horizontal más lento
        float targetVelocityX = horizontal * waterHorizontalSpeed;
        rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);

        // Limitar velocidad de caída (simula resistencia del agua)
        if (rb.linearVelocity.y < waterMaxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, waterMaxFallSpeed);
        }

        // Aplicar drag (resistencia)
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            rb.linearVelocity.y * (1f - waterDrag * Time.fixedDeltaTime)
        );
    }

    private void EnterWater()
    {
        if (isUnderwater)
        {
            Debug.LogWarning("⚠️ Ya estaba bajo el agua!");
            return;
        }

        Debug.Log("🌊 === ENTRANDO AL AGUA ===");
        Debug.Log($"Gravedad antes: {rb.gravityScale}");

        isUnderwater = true;
        rb.gravityScale = waterGravity;

        Debug.Log($"Gravedad después: {rb.gravityScale}");
        Debug.Log($"isUnderwater: {isUnderwater}");

        // Desactivar mecánicas terrestres
        if (isStickingToWall)
            UnstickFromWall();

        canDash = false;
        isStickingToWall = false;
        wallJumping = false;

        // Activar animación de agua
        if (animator != null)
        {
            animator.SetBool("isUnderwater", true);
            animator.SetBool("isTouchingWall", false);
            animator.SetBool("isDashing", false);
            Debug.Log("✅ Animator: isUnderwater = true");
        }
        else
        {
            Debug.LogWarning("⚠️ Animator es null!");
        }
    }

    private void ExitWater()
    {
        if (!isUnderwater)
        {
            Debug.LogWarning("⚠️ Ya estaba fuera del agua!");
            return;
        }

        Debug.Log("🏝️ === SALIENDO DEL AGUA ===");
        Debug.Log($"Gravedad antes: {rb.gravityScale}");

        isUnderwater = false;
        rb.gravityScale = normalGravity;
        canDash = true;

        Debug.Log($"Gravedad después: {rb.gravityScale}");
        Debug.Log($"isUnderwater: {isUnderwater}");

        // Desactivar animación de agua
        if (animator != null)
        {
            animator.SetBool("isUnderwater", false);
            Debug.Log("✅ Animator: isUnderwater = false");
        }
    }

    // ✅ Detectar entrada/salida del agua
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasDied) return;

        // 🔍 DEBUG - Muestra TODOS los triggers
        Debug.Log($"[TRIGGER ENTER] GameObject: {other.gameObject.name}, Layer: {other.gameObject.layer}, LayerName: {LayerMask.LayerToName(other.gameObject.layer)}");

        // 🌊 Detectar agua
        int waterLayerValue = waterLayer.value;
        int otherLayerMask = 1 << other.gameObject.layer;
        bool isWater = (otherLayerMask & waterLayerValue) != 0;

        Debug.Log($"[WATER CHECK] WaterLayer value: {waterLayerValue}, Other layer mask: {otherLayerMask}, Is water: {isWater}");

        if (isWater)
        {
            Debug.Log("✅ ¡AGUA DETECTADA! Llamando a EnterWater()");
            EnterWater();
            return;
        }

        // Interactuables normales
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            currentInteractable = interactable;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (hasDied) return;

        Debug.Log($"[TRIGGER EXIT] GameObject: {other.gameObject.name}, Layer: {other.gameObject.layer}");

        // 🌊 Salir del agua
        int waterLayerValue = waterLayer.value;
        int otherLayerMask = 1 << other.gameObject.layer;
        bool isWater = (otherLayerMask & waterLayerValue) != 0;

        if (isWater)
        {
            Debug.Log("🏝️ ¡SALIENDO DEL AGUA! Llamando a ExitWater()");
            ExitWater();
            return;
        }

        // Interactuables normales
        if (other.TryGetComponent<IInteractable>(out var interactable) && interactable == currentInteractable)
        {
            currentInteractable = null;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);
        }
    }

    // ============================================
    // CONTROLES DE ENTRADA
    // ============================================

    private bool IsGrounded()
    {
        if (groundCheck == null)
        {
            Debug.LogWarning("GroundCheck es null!");
            return false;
        }

        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        return isGrounded;
    }

    private void StickToWall()
    {
        isStickingToWall = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    private void UnstickFromWall()
    {
        isStickingToWall = false;
        rb.gravityScale = normalGravity;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (hasDied) return;

        Vector2 input = context.ReadValue<Vector2>();
        horizontal = input.x;

        // 🌊 Bajo el agua solo se usa horizontal, no vertical
        if (isUnderwater) return;

        if (isClimbingLadder && input != Vector2.zero)
        {
            isClimbingLadder = false;
            rb.gravityScale = normalGravity;
        }

        if (isStickingToWall && Mathf.Sign(horizontal) != Mathf.Sign(transform.localScale.x) && horizontal != 0)
        {
            UnstickFromWall();
            rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
        }
    }

    // ✅ SALTO - Cambia según el contexto (tierra vs agua)
    // ✅ SALTO - Cambia según el contexto (tierra vs agua)
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.started || !canMove || hasDied) return;

        // 🌊 SALTO BAJO EL AGUA (Estilo Flappy Bird)
        if (isUnderwater)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, waterJumpForce);

            if (animator != null)
            {
                animator.SetTrigger("WaterShoot");
                Debug.Log("🌊 Trigger WaterShoot activado"); // ✅ AÑADE ESTE LOG
            }

            if (showDebugLogs)
                Debug.Log("🌊 ¡Aleteo bajo el agua!");

            return;
        }

        // --- SALTO TERRESTRE NORMAL ---
        Debug.Log($"Intento salto normal - Grounded: {IsGrounded()}, Coyote: {coyoteTimeCounter}"); // ✅ AÑADE ESTE LOG

        if (coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
            coyoteTimeCounter = 0f;

            if (showDebugLogs)
                Debug.Log("¡Salto normal ejecutado!");
        }
        else if (isStickingToWall)
        {
            float wallDir = Mathf.Sign(transform.localScale.x);
            rb.gravityScale = normalGravity;
            rb.linearVelocity = new Vector2(-wallDir * wallJumpForce.x, wallJumpForce.y);
            transform.localScale = new Vector3(-wallDir, 1, 1);
            isStickingToWall = false;
            wallJumping = true;
            canStickToWall = false;

            if (animator != null)
                animator.SetBool("isTouchingWall", false);

            StartCoroutine(ResetWallJumpState(0.2f));

            if (showDebugLogs)
                Debug.Log("¡WALL JUMP EJECUTADO!");
        }
    }
    // ... resto del código

    private IEnumerator ResetWallJumpState(float delay)
    {
        yield return new WaitForSeconds(delay);
        wallJumping = false;
        canStickToWall = true;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        // 🌊 No se puede hacer dash bajo el agua
        if (isUnderwater) return;

        if (context.started && canDash && !hasDied)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        canDash = false;

        if (animator != null)
            animator.SetBool("isDashing", true);

        float direction = horizontal != 0 ? Mathf.Sign(horizontal) : transform.localScale.x;
        rb.linearVelocity = new Vector2(direction * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        if (animator != null)
            animator.SetBool("isDashing", false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // ============================================
    // ANIMACIONES
    // ============================================

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // 🌊 Animaciones bajo el agua - SOLO isUnderwater
        if (isUnderwater)
        {
            // isUnderwater ya se setea en EnterWater() y ExitWater()
            // No necesitas hacer nada más aquí para las animaciones básicas
            return;
        }

        // Animaciones terrestres normales
        animator.SetBool("isTouchingWall", isStickingToWall && !IsGrounded());
        animator.SetFloat("Speed", Mathf.Abs(horizontal));

        float verticalVelocity = rb.linearVelocity.y;
        bool grounded = IsGrounded();

        bool isJumping = !grounded && verticalVelocity > 0.1f;
        animator.SetBool("isJumping", isJumping);

        bool isFalling = !grounded && verticalVelocity < -0.1f;
        animator.SetBool("isFalling", isFalling);
    }

    // ============================================
    // OTROS MÉTODOS
    // ============================================

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started && currentInteractable != null && !hasDied)
        {
            currentInteractable.Interact(this);
            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);
        }
    }

    public void StartClimbingLadder(Ladder ladder)
    {
        if (hasDied || isUnderwater) return;

        isClimbingLadder = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(0f, 2.5f);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(wallCheck.position, 0.2f);
        }
    }

    public void StartDialogue(DialogueDataSO dialogue)
    {
        if (hasDied) return;

        if (dialogueUI != null)
        {
            dialogueUI.ShowDialogue(dialogue);
        }
        else
        {
            Debug.LogWarning("DialogueUI no está asignado.");
        }
    }

    public void ApplyKnockback(Vector2 sourcePosition, float forceMultiplier = 1f)
    {
        if (hasDied) return;

        Vector2 direction = ((Vector2)transform.position - sourcePosition).normalized;

        if (Mathf.Abs(direction.x) < 0.1f)
        {
            direction.x = transform.localScale.x;
        }

        StartCoroutine(KnockbackCoroutine(direction, forceMultiplier));

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        if (showDebugLogs)
            Debug.Log($"Knockback aplicado: dirección {direction}, multiplicador {forceMultiplier}");
    }

    private IEnumerator KnockbackCoroutine(Vector2 direction, float forceMultiplier)
    {
        canMove = false;

        float horizontalKnockback = direction.x * knockbackHorizontalForce * forceMultiplier;
        float verticalKnockback = knockbackVerticalForce * forceMultiplier;

        rb.linearVelocity = new Vector2(horizontalKnockback, verticalKnockback);

        float elapsed = 0f;
        Vector2 initialVelocity = rb.linearVelocity;

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / knockbackDuration;

            rb.linearVelocity = new Vector2(
                initialVelocity.x * (1 - t),
                rb.linearVelocity.y
            );

            yield return null;
        }

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        canMove = true;
    }

    public void Die()
    {
        if (hasDied) return;

        Debug.Log("¡Jugador ha muerto!");

        hasDied = true;
        canMove = false;

        StopAllCoroutines();

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        if (animator != null)
        {
            animator.ResetTrigger("Hit");
            animator.ResetTrigger("JumpCharge");
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isDashing", false);
            animator.SetBool("isTouchingWall", false);
            animator.SetBool("isUnderwater", false);
            animator.SetFloat("Speed", 0f);
            animator.SetTrigger("Death");
        }

        if (showDebugLogs)
            Debug.Log("Animación de muerte activada");
    }

    // ✅ AÑADE ESTE MÉTODO AL FINAL DE PlayerController.cs
    public bool IsUnderwater()
    {
        return isUnderwater;
    }
}