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

    private float horizontal;
    private bool jumpPressed;

    private void Update()
    {
        if (jumpPressed && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
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
        // La acci�n debe ser de tipo Vector2
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
}

