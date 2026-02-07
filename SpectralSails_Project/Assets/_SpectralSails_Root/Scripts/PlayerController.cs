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

    // ✅ NUEVO: Variables para el salto con animación de carga
    private bool isChargingJump = false;
    private bool jumpQueued = false; // Para recordar que queremos saltar

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
        if (hasDied)
        {
            return;
        }

        if (!canMove) return;

        // Voltear sprite
        if (horizontal > 0.01f && !isStickingToWall)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (horizontal < -0.01f && !isStickingToWall)
            transform.localScale = new Vector3(-1f, 1f, 1f);

        // ✅ No permitir movimiento durante carga de salto
        if (isDashing || wallJumping || isClimbingLadder || isChargingJump)
            return;

        bool grounded = IsGrounded();
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);

        isGroundedDebug = grounded;
        verticalVelocityDebug = rb.linearVelocity.y;

        // Wall stick logic
        if (!grounded && isTouchingWall && !wallJumping && canStickToWall)
            StickToWall();
        else if ((grounded || !isTouchingWall) && isStickingToWall)
            UnstickFromWall();

        animator.SetBool("isTouchingWall", isStickingToWall && !grounded);

        // Coyote time
        coyoteTimeCounter = grounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;

        // Animaciones
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
        // ✅ No mover durante carga de salto
        if (isDashing || wallJumping || isClimbingLadder || hasDied || isChargingJump)
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

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.started || !canMove || hasDied) return;

        // ✅ SALTO NORMAL (con animación de carga)
        if (coyoteTimeCounter > 0f && !isChargingJump)
        {
            // Iniciar animación de carga
            isChargingJump = true;
            jumpQueued = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Detener movimiento vertical
            coyoteTimeCounter = 0f;

            // Activar animación de carga
            animator.SetTrigger("JumpCharge"); // ← Trigger para iniciar la animación

            if (showDebugLogs)
                Debug.Log("¡Cargando salto!");

            // ⚠️ NO saltamos aquí, se hará desde ExecuteJump() llamado por Animation Event
        }
        // ✅ WALL JUMP (sin carga, instantáneo)
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

    // ✅ ESTE MÉTODO SE LLAMA DESDE UN ANIMATION EVENT EN EL FRAME 3 (después de la carga)
    public void ExecuteJump()
    {
        if (!jumpQueued) return;

        // Aplicar la fuerza del salto
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);

        jumpQueued = false;
        isChargingJump = false;

        if (showDebugLogs)
            Debug.Log("¡SALTO EJECUTADO!");
    }

    // ✅ LLAMAR ESTO SI LA ANIMACIÓN DE CARGA SE CANCELA (opcional)
    public void CancelJumpCharge()
    {
        isChargingJump = false;
        jumpQueued = false;
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

    public void ApplyKnockback(Vector2 sourcePosition, float force)
    {
        if (hasDied) return;

        Vector2 direction = ((Vector2)transform.position - sourcePosition).normalized;
        StartCoroutine(KnockbackCoroutine(direction * force));

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
    }

    private IEnumerator KnockbackCoroutine(Vector2 knockbackVelocity)
    {
        canMove = false;
        rb.linearVelocity = knockbackVelocity;

        yield return new WaitForSeconds(0.15f);

        rb.linearVelocity = Vector2.zero;
        canMove = true;
    }

    public void Die()
    {
        if (hasDied) return;

        Debug.Log("¡Jugador ha muerto!");

        hasDied = true;
        canMove = false;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        if (animator != null)
        {
            animator.ResetTrigger("Hit");
            animator.SetFloat("Speed", 0f);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isDashing", false);
            animator.SetBool("isTouchingWall", false);
            animator.SetTrigger("Death");
        }

        GetComponent<Collider2D>().enabled = false;
    }

    public void DieAlternative()
    {
        if (hasDied) return;

        Debug.Log("¡Jugador ha muerto!");

        hasDied = true;
        canMove = false;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        if (animator != null)
        {
            animator.enabled = false;
            StartCoroutine(PlayDeathAnimation());
        }

        GetComponent<Collider2D>().enabled = false;
    }

    private IEnumerator PlayDeathAnimation()
    {
        yield return null;

        animator.enabled = true;
        animator.Play("PlayerDeath", 0, 0f);

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);

        animator.speed = 0f;
    }
}