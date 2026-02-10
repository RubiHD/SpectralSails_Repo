using UnityEngine;

public class DestructiblePlank : MonoBehaviour
{
    [Header("Configuración de Resistencia")]
    [SerializeField] private int maxHits = 3;
    private int currentHits = 0;

    [Header("Componentes")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D solidCollider; // El collider que bloquea el paso
    [SerializeField] private ParticleSystem woodParticles; // Partículas de madera

    [Header("Sprites de Daño")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite crackedSprite; // Después de 1 golpe
    [SerializeField] private Sprite veryCrackedSprite; // Después de 2 golpes

    [Header("Efectos Visuales")]
    [SerializeField] private float shakeIntensity = 0.15f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Audio (Opcional)")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip breakSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Indicador Visual Inicial")]
    [SerializeField] private bool pulseOnStart = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.2f;

    private Vector3 originalPosition;
    private Color originalColor;
    private bool isDestroyed = false;

    private void Start()
    {
        // Guardar valores originales
        originalPosition = transform.position;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Auto-asignar componentes si no están asignados
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (solidCollider == null)
            solidCollider = GetComponent<Collider2D>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Guardar sprite original
        if (spriteRenderer != null && normalSprite == null)
            normalSprite = spriteRenderer.sprite;
    }

    private void Update()
    {
        // Efecto de pulso para indicar que es destructible
        if (pulseOnStart && !isDestroyed && currentHits == 0)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            spriteRenderer.color = Color.Lerp(originalColor, Color.yellow, pulse);
        }
    }

    // ✅ Este método se llama desde PlayerCombat cuando la espada golpea
    public void TakeDamage(int damage = 1)
    {
        if (isDestroyed) return;

        currentHits++;

        Debug.Log($"Tabla golpeada: {currentHits}/{maxHits}");

        // Reproducir sonido de golpe
        PlaySound(hitSound);

        // Efectos visuales
        StartCoroutine(ShakeEffect());
        StartCoroutine(FlashEffect());

        // Emitir partículas de madera
        if (woodParticles != null)
        {
            woodParticles.Play();
        }

        // Cambiar sprite según daño acumulado
        UpdateSprite();

        // Comprobar si se destruye
        if (currentHits >= maxHits)
        {
            DestroyPlank();
        }
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null) return;

        // Cambiar sprite según golpes recibidos
        if (currentHits == 1 && crackedSprite != null)
        {
            spriteRenderer.sprite = crackedSprite;
        }
        else if (currentHits == 2 && veryCrackedSprite != null)
        {
            spriteRenderer.sprite = veryCrackedSprite;
        }
    }

    private void DestroyPlank()
    {
        if (isDestroyed) return;

        isDestroyed = true;

        Debug.Log("¡Tabla destruida!");

        // Reproducir sonido de rotura
        PlaySound(breakSound);

        // Desactivar collider para permitir el paso
        if (solidCollider != null)
        {
            solidCollider.enabled = false;
        }

        // Efecto de destrucción
        if (woodParticles != null)
        {
            // Emitir más partículas en la destrucción
            var emission = woodParticles.emission;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0.0f, 20)
            });
            woodParticles.Play();
        }

        // Hacer invisible el sprite
        if (spriteRenderer != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            // Si no hay sprite renderer, destruir inmediatamente
            Destroy(gameObject, 0.5f);
        }
    }

    // ✅ Efecto de temblor al recibir golpe
    private System.Collections.IEnumerator ShakeEffect()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

            transform.position = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }

    // ✅ Efecto de flash al recibir golpe
    private System.Collections.IEnumerator FlashEffect()
    {
        if (spriteRenderer == null) yield break;

        // Detener el pulso durante el flash
        bool wasPulsing = pulseOnStart;
        pulseOnStart = false;

        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;

        pulseOnStart = wasPulsing && currentHits == 0;
    }

    // ✅ Desvanecer el sprite al destruirse
    private System.Collections.IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float fadeDuration = 0.5f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeDuration);
            spriteRenderer.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                alpha
            );
            yield return null;
        }

        Destroy(gameObject);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // ✅ Método para debugging en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}