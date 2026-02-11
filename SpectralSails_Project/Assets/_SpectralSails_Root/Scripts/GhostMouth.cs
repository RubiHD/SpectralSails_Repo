using UnityEngine;

public class GhostMouth : MonoBehaviour
{
    [Header("Configuración")]
    public float maxLifetime = 3f;
    public int damage = 2;
    public float damageRadius = 1.5f;

    [Header("Referencias")]
    [SerializeField] private Animator mouthAnimator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Efectos")]
    public AudioClip spawnSound;
    public AudioClip closeSound;
    public GameObject closeEffect;

    private bool hasDealtDamage = false;
    private Vector3 targetPosition;

    private void Start()
    {
        // Sonido de aparición
        if (spawnSound != null)
        {
            AudioSource.PlayClipAtPoint(spawnSound, transform.position);
        }

        // Destruir automáticamente si no se destruye antes
        Destroy(gameObject, maxLifetime);
    }

    public void Initialize(Vector3 target)
    {
        targetPosition = target;

        // Calcular dirección y rotación
        Vector3 direction = (target - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Rotar la boca para que "mire" al jugador
        // +180 porque la boca está "cerrada" mirando hacia atrás
        transform.rotation = Quaternion.Euler(0, 0, angle + 180f);

        // Aparecer un poco más adelante
        float offsetDistance = 0.5f;
        transform.position += direction * offsetDistance;

        // Activar animación de cierre
        if (mouthAnimator != null)
        {
            mouthAnimator.SetTrigger("Close");
        }
    }

    // ✅ LLAMADO DESDE ANIMATION EVENT EN EL FRAME DEL MORDISCO
    public void DealDamage()
    {
        if (hasDealtDamage) return;
        hasDealtDamage = true;

        // Sonido de cierre
        if (closeSound != null)
        {
            AudioSource.PlayClipAtPoint(closeSound, transform.position, 0.7f);
        }

        // Detectar jugador en el área
        Collider2D hit = Physics2D.OverlapCircle(transform.position, damageRadius, LayerMask.GetMask("Player"));

        if (hit != null)
        {
            PlayerHealth player = hit.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage, transform.position);
                Debug.Log("¡Boca fantasma mordió al jugador!");
            }
        }
        else
        {
            Debug.Log("Boca fantasma falló el mordisco");
        }

        // Efecto visual
        if (closeEffect != null)
        {
            Instantiate(closeEffect, transform.position, Quaternion.identity);
        }
    }

    // ✅ LLAMADO DESDE ANIMATION EVENT AL FINAL DE LA ANIMACIÓN
    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}