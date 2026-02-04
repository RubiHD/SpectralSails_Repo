using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject interactPromptUI;

    [Header("Movimiento")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpingPower = 10f;

    [Header("Wall Jump")]
    [SerializeField] private Vector2 wallJumpForce = new Vector2(12f, 12f);

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Dash")]
    [SerializeField] private float dashForce = 12f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;
    private bool isDashing = false;
    private bool canDash = true;

    [Header("Detección")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    private float horizontal;
    private bool isTouchingWall;
    private bool isStickingToWall = false;
    private bool wallJumping = false;
    private bool canStickToWall = true;
    private bool isClimbingLadder = false;

    private IInteractable currentInteractable;


    public DialogueUI dialogueUI;

    public bool canMove = true;


    private void Update()
    {

        if (!canMove) return;

        if (isDashing || wallJumping || isClimbingLadder)
            return;

        bool grounded = IsGrounded();
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);

        if (!grounded && isTouchingWall && !wallJumping && canStickToWall)
        {
            StickToWall();
        }
        else if ((grounded || !isTouchingWall) && isStickingToWall)
        {
            UnstickFromWall();
        }

        coyoteTimeCounter = IsGrounded() ? coyoteTime : coyoteTimeCounter - Time.deltaTime;


    }

    private void FixedUpdate()
    {
        if (isDashing || wallJumping || isClimbingLadder)
            return;

        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void StickToWall()
    {
        isStickingToWall = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
    }

    private void UnstickFromWall()
    {
        isStickingToWall = false;
        rb.gravityScale = 1f;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
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
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (coyoteTimeCounter > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
                coyoteTimeCounter = 0f;
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
                StartCoroutine(ResetWallJumpState(0.2f));
            }
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
        if (context.started && canDash)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        canDash = false;

        float direction = horizontal != 0 ? Mathf.Sign(horizontal) : transform.localScale.x;
        rb.linearVelocity = new Vector2(direction * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started && currentInteractable != null)
        {
            currentInteractable.Interact(this);
            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);
        }
    }

    public void StartClimbingLadder(Ladder ladder)
    {
        isClimbingLadder = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(0f, 2.5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            currentInteractable = interactable;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
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
        Debug.Log("StartDialogue llamado con: " + dialogue.name);

        if (dialogueUI != null)
        {
            dialogueUI.ShowDialogue(dialogue); // ✅ Pasa el ScriptableObject entero
        }
        else
        {
            Debug.LogWarning("DialogueUI no está asignado.");
        }
    }


}
