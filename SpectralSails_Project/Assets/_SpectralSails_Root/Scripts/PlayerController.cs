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
        // ✅ No hacer nada si está muerto
        if (hasDied)
        {
            return;
        }

        if (!canMove) return;

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

        animator.SetBool("isTouchingWall", isStickingToWall && !grounded);

        coyoteTimeCounter = grounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;

        animator.SetFloat("Speed", Mathf.Abs(horizontal));

        float verticalVelocity = rb.linearVelocity.y;

        bool isJumping = !grounded && verticalVelocity > 0.1f;
        animator.SetBool("isJumping", isJumping);

        bool isFalling = !grounded && verticalVelocity < -0.1f;
        animator.SetBool("isFalling", isFalling);

        if (showDebugLogs)
        {
            Debug.Log($"Grounded: {grounded} | VelY: {verticalVelocity:F2} | Jumping: {isJumping} | Falling: {isFalling}");
        }
    }

    private void FixedUpdate()
    {
        if (isDashing || wallJumping || isClimbingLadder || hasDied)
            return;

        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
    }

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
        rb.gravityScale = 1f;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (hasDied) return;

        Vector2 input = context.ReadValue<Vector2>();
        horizontal = input.x;

        if (isClimbingLadder && input != Vector2.zero)
        {
            isClimbingLadder = false;
            rb.gravityScale = 1f;
        }

        if (isStickingToWall && Mathf.Sign(horizontal) != Mathf.Sign(transform.localScale.x) && horizontal != 0)
        {
            UnstickFromWall();
            rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
        }
    }

    // ✅ SALTO INSTANTÁNEO - Sin carga
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.started || !canMove || hasDied) return;

        // Salto normal desde el suelo
        if (coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
            coyoteTimeCounter = 0f;

            if (showDebugLogs)
                Debug.Log("¡Salto normal ejecutado!");
        }
        // Wall jump
        else if (isStickingToWall)
        {
            float wallDir = Mathf.Sign(transform.localScale.x);
            rb.gravityScale = 1f;
            rb.linearVelocity = new Vector2(-wallDir * wallJumpForce.x, wallJumpForce.y);
            transform.localScale = new Vector3(-wallDir, 1, 1);
            isStickingToWall = false;
            wallJumping = true;
            canStickToWall = false;

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
        if (context.started && canDash && !hasDied)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        canDash = false;

        animator.SetBool("isDashing", true);

        float direction = horizontal != 0 ? Mathf.Sign(horizontal) : transform.localScale.x;
        rb.linearVelocity = new Vector2(direction * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        animator.SetBool("isDashing", false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

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
        if (hasDied) return;

        isClimbingLadder = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(0f, 2.5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasDied) return;

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

        if (other.TryGetComponent<IInteractable>(out var interactable) && interactable == currentInteractable)
        {
            currentInteractable = null;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);
        }
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

    // ✅ KNOCKBACK MEJORADO - No se aplica si está muerto
    public void ApplyKnockback(Vector2 sourcePosition, float forceMultiplier = 1f)
    {
        if (hasDied) return; // ← Importante: no knockback si está muerto

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

    // ✅ MÉTODO DE MUERTE CORREGIDO
    public void Die()
    {
        if (hasDied) return;

        Debug.Log("¡Jugador ha muerto!");

        // ✅ PRIMERO: Marcar como muerto para detener todo
        hasDied = true;
        canMove = false;

        // ✅ Detener todas las coroutines que puedan interferir
        StopAllCoroutines();

        // ✅ Congelar física completamente
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // ✅ Desactivar collider para evitar interacciones
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // ✅ Limpiar TODOS los parámetros del Animator
        if (animator != null)
        {
            // Resetear triggers activos
            animator.ResetTrigger("Hit");
            animator.ResetTrigger("JumpCharge");

            // Resetear todos los bools
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isDashing", false);
            animator.SetBool("isTouchingWall", false);

            // Resetear floats
            animator.SetFloat("Speed", 0f);

            // ✅ Activar animación de muerte
            animator.SetTrigger("Death");
        }

        if (showDebugLogs)
            Debug.Log("Animación de muerte activada");
    }
}