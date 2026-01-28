using UnityEngine;

public class FloatingEnemy : MonoBehaviour
{
    [Header("Floating Movement")]
    public float floatAmplitude = 0.5f;
    public float floatSpeed = 2f;

    [Header("Damage Settings")]
    public int damageAmount = 1;

    [Header("Knockback Settings")]
    public float knockbackForce = 8f;
    public float knockbackUpwardForce = 2f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHealth player = collision.GetComponent<PlayerHealth>();
        Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();

        if (player != null)
        {
            // Aplicar daño
            player.TakeDamage(damageAmount);

            // Aplicar empujón si el jugador tiene Rigidbody2D
            if (playerRb != null)
            {
                Vector2 direction = (playerRb.transform.position - transform.position).normalized;

                Vector2 knockback = new Vector2(
                    direction.x * knockbackForce,
                    knockbackUpwardForce
                );

                playerRb.linearVelocity = knockback;
            }
        }
    }
}
