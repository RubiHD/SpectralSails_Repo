using UnityEngine;

/// <summary>
/// Versión SIMPLE de barril estilo bolos
/// El barril hace un pequeño arco y luego rueda por el suelo
/// </summary>
public class BarrelSimple : MonoBehaviour
{
    [Header("Lanzamiento")]
    [SerializeField] private float horizontalSpeed = 7f;
    [SerializeField] private float verticalSpeed = 3f; // Arco inicial

    [Header("Rotación")]
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Daño")]
    [SerializeField] private int damage = 1;

    [Header("Vida")]
    [SerializeField] private float lifeTime = 8f;

    [Header("Efectos")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip hitSound;

    private Rigidbody2D rb;
    private bool hasHit = false;
    private float direction = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // Configuración para bola de bolos
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 2.5f; // Gravedad normal
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Collider principal (física)
        CircleCollider2D physicsCollider = GetComponent<CircleCollider2D>();
        if (physicsCollider == null)
        {
            physicsCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        physicsCollider.radius = 0.5f;

        // Material de física para fricción
        PhysicsMaterial2D material = new PhysicsMaterial2D();
        material.friction = 0.3f;
        material.bounciness = 0.2f;
        physicsCollider.sharedMaterial = material;

        // Trigger collider para daño al jugador
        CircleCollider2D triggerCollider = gameObject.AddComponent<CircleCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = 0.5f;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Rotar basado en velocidad
        if (rb != null)
        {
            float rotation = -(rb.linearVelocity.x / horizontalSpeed) * rotationSpeed;
            transform.Rotate(0, 0, rotation * Time.deltaTime);
        }
    }

    public void SetDirection(float dir)
    {
        direction = Mathf.Sign(dir);

        // Voltear sprite
        transform.localScale = new Vector3(direction, 1, 1);

        // Aplicar velocidad inicial (arco de lanzamiento)
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction * horizontalSpeed, verticalSpeed);
            Debug.Log($"Barrel lanzado: velocidad = {rb.linearVelocity}");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        // Daño al jugador
        if (collision.CompareTag("Player"))
        {
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage, transform.position);
                Debug.Log($"Barrel golpeó al jugador");
            }
            hasHit = true;
            DestroyBarrel();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si choca con una pared, destruir
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Verificar si es una pared vertical
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Si la normal es muy horizontal, es una pared
                if (Mathf.Abs(contact.normal.x) > 0.7f)
                {
                    hasHit = true;
                    DestroyBarrel();
                    return;
                }
            }
        }
    }

    private void DestroyBarrel()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }

        Destroy(gameObject);
    }
}