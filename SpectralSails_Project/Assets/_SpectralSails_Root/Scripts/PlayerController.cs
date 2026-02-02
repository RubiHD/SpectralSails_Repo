using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Component References")]
    [SerializeField] Rigidbody2D rb;

    [Header("Player Settings")]
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpingPower = 10f;

    [Header("Grounding")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundCheck;

    [Header("Dash Settings")]
    [SerializeField] float dashForce = 12f;
    [SerializeField] float dashDuration = 0.15f;
    [SerializeField] float dashCooldown = 0.5f;

    private float horizontal;
    private bool jumpPressed;

    private bool isDashing = false;
    private bool canDash = true;

    [Header("Wall Jump Settings")]
    [SerializeField] LayerMask wallLayer;
    [SerializeField] Transform wallCheck;
    [SerializeField] float wallSlideSpeed = 1f;
    [SerializeField] Vector2 wallJumpForce = new Vector2(8f, 12f);

    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isWallJumping;


    private void Update()
    {
        if (isDashing)
            return;

        if (jumpPressed && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
            return;

        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
        if (horizontal > 0)
            transform.localScale = new Vector3(1, 1, 1);

        if (horizontal < 0)
            transform.localScale = new Vector3(-1, 1, 1);


        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);

        if (!IsGrounded() && isTouchingWall && horizontal != 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }

        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
        }

    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    // -------------------------
    // INPUT ACTIONS (Unity Events)
    // -------------------------

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        horizontal = input.x;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
            jumpPressed = true;

        if (context.canceled)
            jumpPressed = false;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && canDash)
        {
            StartDash();
        }
    }

    // -------------------------
    // DASH LOGIC
    // -------------------------

    private void StartDash()
    {
        isDashing = true;
        canDash = false;

        float direction = horizontal != 0 ? Mathf.Sign(horizontal) : transform.localScale.x;

        rb.linearVelocity = new Vector2(direction * dashForce, 0f);

        Invoke(nameof(EndDash), dashDuration);
        Invoke(nameof(ResetDash), dashCooldown);
    }

    private void EndDash()
    {
        isDashing = false;
    }

    private void ResetDash()
    {
        canDash = true;
    }
}
