using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{
    #region General Variables
    [Header("Movement & Jump Configuration")]
    [SerializeField] float speed = 8f;
    [SerializeField] bool isFacingRight = true;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] bool isGrounded;
    [SerializeField] Transform groundCheck; //Posicion del detector del suelo 
    [SerializeField] float groundCheckRadius = 0.1f; //Radio del detector del suelo 
    [SerializeField] LayerMask groundLayer; //Define la capa que puede tocar el detector del suelo 

   
    //variables de referencia privadas 
    Rigidbody2D playerRb; //referencia al rigidbody del personaje 
    Animator anim; //referencia al controlador de animaciones del player 
    PlayerInput input; //referencia al cerebro de inputs del player 
    Vector2 moveInput; //referencia al valor pulsado de las teclas de movimiento
    bool canAttack; //comprobador para determinar si se puede atacar  

    #endregion

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        input = GetComponent<PlayerInput>();
        canAttack = true;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //logica de la deteccion del suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
       
        //ejecucion de la logica del flip
        if (moveInput.x > 0 && !isFacingRight) Flip();
        if (moveInput.x < 0 && isFacingRight) Flip();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    void Movement()
    {
        playerRb.linearVelocity = new Vector2(moveInput.x * speed, playerRb.linearVelocity.y);
    }

    void Flip()
    {
        Vector3 currentScale = transform.localScale; //almacenamos el valor scale actual 
        currentScale.x *= -1; //cambiamos el valor de scale X al contrario actual 
        transform.localScale = currentScale; //a la sclae actual le pasamos la nueva modificada 
        isFacingRight = !isFacingRight; //cambiar el bool al valor contrario 
    }

    void Jump()
    {
        playerRb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
       
    }

    IEnumerator Attack()
    {
        anim.SetTrigger("Attacking");
        canAttack = false;
        float actualSpeed = speed;
        speed = 0;
        yield return new WaitForSeconds(0.8f);
        speed = actualSpeed;
        canAttack = true;
        yield return null;
    }
   
    

    #region Input Methods
    public void OnMovement(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded) Jump();
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded && canAttack) StartCoroutine(Attack());
    }

    #endregion

}
