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
    [SerializeField] private float waterGravity = 0.5f;
    [SerializeField] private float waterJumpForce = 8f;
    [SerializeField] private float waterHorizontalSpeed = 3f;
    [SerializeField] private float waterMaxFallSpeed = -3f;
    [SerializeField] private float waterDrag = 2f;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private RuntimeAnimatorController underwaterAnimator; // ✅ NUEVO
    [SerializeField] private RuntimeAnimatorController normalAnimator; // ✅ NUEVO
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
        normalGravity = rb.gravityScale;

        // ✅ Guardar el Animator Controller normal al inicio
        if (animator != null && normalAnimator == null)
        {
            normalAnimator = animator.runtimeAnimatorController as RuntimeAnimatorController;
            Debug.Log($"Animator normal guardado: {normalAnimator?.name}");
        }

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

        if (isUnderwater)
        {
            ApplyUnderwaterPhysics();
            return;
        }

        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
    }

    // 🌊 ============================================
    // SISTEMA DE MOVIMIENTO BAJO EL AGUA
    // ============================================

    private void UpdateUnderwaterMovement()
    {
        if (horizontal > 0.01f)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (horizontal < -0.01f)
            transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    private void ApplyUnderwaterPhysics()
    {
        float targetVelocityX = horizontal * waterHorizontalSpeed;
        rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);

        if (rb.linearVelocity.y < waterMaxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, waterMaxFallSpeed);
        }

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            rb.linearVelocity.y * (1f - waterDrag * Time.fixedDeltaTime)
        );
    }

    private void EnterWater()
    {
        if (isUnderwater) return;

        Debug.Log("🌊 === ENTRANDO AL AGUA ===");

        isUnderwater = true;
        rb.gravityScale = waterGravity;

        if (isStickingToWall)
            UnstickFromWall();

        canDash = false;
        isStickingToWall = false;
        wallJumping = false;

        // ✅ CAMBIAR AL ANIMATOR ACUÁTICO
        if (animator != null && underwaterAnimator != null)
        {
            animator.runtimeAnimatorController = underwaterAnimator;
            Debug.Log("✅ Animator cambiado a modo acuático");
        }
        else if (underwaterAnimator == null)
        {
            Debug.LogError("❌ Underwater Animator Controller NO asignado en el Inspector!");
        }
    }

    private void ExitWater()
    {
        if (!isUnderwater) return;

        Debug.Log("🏝️ === SALIENDO DEL AGUA ===");

        isUnderwater = false;
        rb.gravityScale = normalGravity;
        canDash = true;

        // ✅ VOLVER AL ANIMATOR NORMAL
        if (animator != null && normalAnimator != null)
        {
            animator.runtimeAnimatorController = normalAnimator;
            Debug.Log("✅ Animator cambiado a modo terrestre");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasDied) return;

        int waterLayerValue = waterLayer.value;
        int otherLayerMask = 1 << other.gameObject.layer;
        bool isWater = (otherLayerMask & waterLayerValue) != 0;

        if (isWater)
        {
            EnterWater();
            return;
        }

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

        int waterLayerValue = waterLayer.value;
        int otherLayerMask = 1 << other.gameObject.layer;
        bool isWater = (otherLayerMask & waterLayerValue) != 0;

        if (isWater)
        {
            ExitWater();
            return;
        }

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

        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
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
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.started || !canMove || hasDied) return;

        // 🌊 BAJO EL AGUA - El Espacio hace impulso hacia arriba (Flappy Bird)
        if (isUnderwater)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, waterJumpForce);

            // ✅ NO activar trigger de disparo aquí

            if (showDebugLogs)
                Debug.Log("🌊 ¡Impulso hacia arriba!");

            return;
        }

        // --- SALTO TERRESTRE NORMAL ---
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

    private IEnumerator ResetWallJumpState(float delay)
    {
        yield return new WaitForSeconds(delay);
        wallJumping = false;
        canStickToWall = true;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
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

        // 🌊 Bajo el agua - NO actualizar parámetros
        // El Animator acuático solo usa el trigger "Shoot"
        if (isUnderwater)
        {
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
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isDashing", false);
            animator.SetBool("isTouchingWall", false);
            animator.SetFloat("Speed", 0f);
            animator.SetTrigger("Death");
        }
    }

    public bool IsUnderwater()
    {
        return isUnderwater;
    }
}