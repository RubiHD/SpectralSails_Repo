using UnityEngine;

/// <summary>
/// Plataforma móvil que mueve al jugador sin usar SetParent
/// Evita problemas de deformación
/// </summary>
public class SimplePlatform : MonoBehaviour
{
    [Header("Puntos de Movimiento")]
    [SerializeField] private Vector2 pointA = new Vector2(0, 0);
    [SerializeField] private Vector2 pointB = new Vector2(5, 0);

    [Header("Configuración")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float pauseTime = 1f;

    private Vector2 targetPosition;
    private bool movingToB = true;
    private float pauseTimer = 0f;
    private bool isPaused = false;

    // ✅ Guardamos la posición anterior para calcular el movimiento
    private Vector3 previousPosition;

    // ✅ Lista de jugadores sobre la plataforma
    private Transform playerOnPlatform;

    private void Start()
    {
        transform.position = pointA;
        targetPosition = pointB;
        previousPosition = transform.position;
    }

    private void Update()
    {
        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPaused = false;
            }
            return;
        }

        // Guardar posición antes de moverse
        previousPosition = transform.position;

        // Mover hacia el objetivo
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Si llegó al objetivo
        if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
        {
            if (movingToB)
            {
                targetPosition = pointA;
                movingToB = false;
            }
            else
            {
                targetPosition = pointB;
                movingToB = true;
            }

            if (pauseTime > 0f)
            {
                isPaused = true;
                pauseTimer = pauseTime;
            }
        }
    }

    private void LateUpdate()
    {
        // ✅ Mover al jugador la misma distancia que se movió la plataforma
        if (playerOnPlatform != null)
        {
            Vector3 platformMovement = transform.position - previousPosition;
            playerOnPlatform.position += platformMovement;
        }
    }

    // ✅ Detectar cuando el jugador está sobre la plataforma
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = collision.transform;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = collision.transform;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (playerOnPlatform == collision.transform)
            {
                playerOnPlatform = null;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pointA, pointB);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pointA, 0.3f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pointB, 0.3f);
    }
}   