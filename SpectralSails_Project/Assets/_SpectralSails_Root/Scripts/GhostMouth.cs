using UnityEngine;

public class GhostMouth : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float maxLifetime = 3f;
    [SerializeField] private int damage = 2;
    [SerializeField] private float damageRadius = 1.5f;

    [Header("Referencias")]
    [SerializeField] private Animator mouthAnimator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Efectos")]
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private GameObject closeEffect;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private bool hasDealtDamage = false;

    private void Awake()
    {
        if (mouthAnimator == null)
            mouthAnimator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (showDebugLogs)
        {
            Debug.Log($"✅ GhostMouth creado en: {transform.position}");
        }
    }

    private void Start()
    {
        if (spawnSound != null)
        {
            AudioSource.PlayClipAtPoint(spawnSound, transform.position, 0.5f);
        }

        Destroy(gameObject, maxLifetime);
    }

    /// <summary>
    /// ✅ Inicializar con la dirección del boss
    /// Solo voltea el sprite en X, SIN rotar
    /// </summary>
    public void Initialize(float bossDirection)
    {
        // bossDirection: 1 = derecha, -1 = izquierda

        // ✅ SOLO voltear el sprite en X (flip horizontal)
        // NO rotación, NO cambio en Y o Z
        transform.localScale = new Vector3(bossDirection, 1f, 1f);

        if (showDebugLogs)
        {
            Debug.Log($"=== GHOST MOUTH INICIALIZADO ===");
            Debug.Log($"Boss mira hacia: {(bossDirection > 0 ? "DERECHA" : "IZQUIERDA")}");
            Debug.Log($"Scale aplicada: {transform.localScale}");
        }
    }

    public void DealDamage()
    {
        if (hasDealtDamage) return;
        hasDealtDamage = true;

        if (showDebugLogs)
        {
            Debug.Log("=== GHOST MOUTH MORDIENDO ===");
        }

        if (closeSound != null)
        {
            AudioSource.PlayClipAtPoint(closeSound, transform.position, 0.7f);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            damageRadius,
            LayerMask.GetMask("Player")
        );

        if (showDebugLogs)
        {
            Debug.Log($"Detectados {hits.Length} objetos en radio {damageRadius}");
        }

        bool hitPlayer = false;
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth player = hit.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(damage, transform.position);
                    hitPlayer = true;

                    if (showDebugLogs)
                    {
                        Debug.Log($"✅ ¡Mordió al jugador! Daño: {damage}");
                    }
                }
            }
        }

        if (!hitPlayer && showDebugLogs)
        {
            Debug.Log("❌ Falló el mordisco");
        }

        if (closeEffect != null)
        {
            Instantiate(closeEffect, transform.position, Quaternion.identity);
        }
    }

    public void DestroySelf()
    {
        if (showDebugLogs)
        {
            Debug.Log("GhostMouth destruyéndose");
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Área de daño
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}