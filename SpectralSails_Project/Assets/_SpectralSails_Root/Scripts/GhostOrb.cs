using UnityEngine;

public class GhostOrb : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;
    public float lifeTime = 10f;

    [Header("Daño")]
    public int damage = 1;

    [Header("Efectos")]
    public GameObject hitEffect;
    public AudioClip hitSound;
    public bool rotateTowardsDirection = true;

    [Header("Trail")]
    public bool useTrail = true;
    public Color trailColor = new Color(0.5f, 0f, 1f, 1f); // Púrpura

    private Vector2 direction;
    private TrailRenderer trail;
    private bool hasHit = false;

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        // Configurar trail si está activado
        if (useTrail)
        {
            trail = GetComponent<TrailRenderer>();
            if (trail != null)
            {
                trail.startColor = trailColor;
                trail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
            }
        }
    }

    public void Initialize(Vector3 targetPosition)
    {
        direction = (targetPosition - transform.position).normalized;

        // Rotar el sprite hacia la dirección
        if (rotateTowardsDirection)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void Update()
    {
        if (!hasHit)
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // Golpear al jugador
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage, transform.position);
            }

            hasHit = true;
            DestroyOrb();
        }

        // Chocar con terreno
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground") ||
            other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            hasHit = true;
            DestroyOrb();
        }
    }

    private void DestroyOrb()
    {
        // Efecto visual
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // Sonido
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }

        Destroy(gameObject);
    }
}