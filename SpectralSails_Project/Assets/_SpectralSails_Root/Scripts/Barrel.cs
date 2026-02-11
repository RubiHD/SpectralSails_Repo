using UnityEngine;

public class Barrel : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;
    public float lifeTime = 8f;
    private float moveDirection = 1f;

    [Header("Daño")]
    public int damage = 1;

    [Header("Efectos")]
    public GameObject explosionEffect; // Opcional
    public AudioClip hitSound; // Opcional

    [Header("Rotación")]
    public bool rotateWhileMoving = true;
    public float rotationSpeed = 360f;

    private Rigidbody2D rb;
    private bool hasHit = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);

        // Aplicar velocidad inicial
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(moveDirection * speed, 0);
        }
    }

    private void Update()
    {
        // Rotación visual del barril
        if (rotateWhileMoving)
        {
            transform.Rotate(0, 0, rotationSpeed * moveDirection * Time.deltaTime);
        }

        // Si no tiene Rigidbody2D, mover manualmente
        if (rb == null)
        {
            transform.Translate(Vector2.right * moveDirection * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        // Golpear al jugador
        if (collision.CompareTag("Player"))
        {
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage, transform.position);
            }

            hasHit = true;
            DestroyBarrel();
        }

        // Chocar con terreno
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            hasHit = true;
            DestroyBarrel();
        }
    }

    private void DestroyBarrel()
    {
        // Efecto de explosión
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Sonido
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }

        Destroy(gameObject);
    }

    public void SetDirection(float dir)
    {
        moveDirection = dir;

        // Actualizar velocidad si tiene Rigidbody2D
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(moveDirection * speed, 0);
        }

        // Voltear sprite si es necesario
        if (dir < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}