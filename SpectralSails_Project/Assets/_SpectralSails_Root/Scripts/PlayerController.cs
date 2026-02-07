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

    [Header("Interacción")]
    public DialogueUI dialogueUI;
    private IInteractable currentInteractable;
    public bool canMove = true;

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
    }

    private void Update()
    {
        if (hasDied)
        {
            return; // Salir completamente si ha muerto
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

        if (!grounded && isTouchingWall && !wallJumping && canStickToWall)
            StickToWall();
        else if ((grounded || !isTouchingWall) && isStickingToWall)
            UnstickFromWall();

        animator.SetBool("isTouchingWall", isStickingToWall && !grounded);

        coyoteTimeCounter = grounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;

        animator.SetFloat("Speed", Mathf.Abs(horizontal));
        float verticalVelocity = rb.linearVelocity.y;
        animator.SetBool("isFalling", !grounded && verticalVelocity < -0.1f);
        if (grounded || rb.linearVelocity.y <= 0f)
            animator.SetBool("isJumping", false);
    }

    private void FixedUpdate()
    {
        if (isDashing || wallJumping || isClimbingLadder || hasDied) return;

        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
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

        if (coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
            coyoteTimeCounter = 0f;
            animator.SetBool("isJumping", true);
        }
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
            animator.SetBool("isJumping", true);

            StartCoroutine(ResetWallJumpState(0.2f));
        }

        animator.SetBool("isJumping", true);
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
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
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

        Debug.Log("¡Jugador ha muerto! Activando animación de muerte.");

        hasDied = true;
        canMove = false;

        // Detener completamente el movimiento
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        if (animator != null)
        {
            // Resetear todos los triggers que puedan estar activos
            animator.ResetTrigger("Hit");

            // Resetear parámetros del animator
            animator.SetFloat("Speed", 0f);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isDashing", false);
            animator.SetBool("isTouchingWall", false);

            // Activar el trigger de muerte
            animator.SetTrigger("Death"); // Cambia esto por el trigger que uses para la muerte
        }

        GetComponent<Collider2D>().enabled = false;

        // Opcional: reiniciar el juego después de un tiempo
        // StartCoroutine(RestartAfterDeath());
    }

    // Método alternativo si quieres usar Play() directamente
    public void DieAlternative()
    {
        if (hasDied) return;

        Debug.Log("¡Jugador ha muerto! Activando animación de muerte.");

        hasDied = true;
        canMove = false;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        if (animator != null)
        {
            // Detener el animator temporalmente
            animator.enabled = false;

            // Reactivarlo y reproducir la animación
            StartCoroutine(PlayDeathAnimation());
        }

        GetComponent<Collider2D>().enabled = false;
    }

    private IEnumerator PlayDeathAnimation()
    {
        yield return null; // Esperar un frame

        animator.enabled = true;
        animator.Play("PlayerDeath", 0, 0f);

        // Obtener la duración de la animación
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);

        // Congelar en el último frame
        animator.speed = 0f;
    }
}
