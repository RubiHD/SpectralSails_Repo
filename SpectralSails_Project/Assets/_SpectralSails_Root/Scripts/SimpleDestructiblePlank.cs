using UnityEngine;

public class SimpleDestructiblePlank : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D solidCollider;
    [SerializeField] private ParticleSystem woodParticles;

    [Header("Audio (Opcional)")]
    [SerializeField] private AudioClip breakSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Efectos Visuales")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeIntensity = 0.15f;

    private Vector3 originalPosition;
    private bool isDestroyed = false;

    private void Start()
    {
        originalPosition = transform.position;

        // Auto-asignar componentes
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (solidCollider == null)
            solidCollider = GetComponent<Collider2D>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    // ✅ Llamado cuando una bala colisiona
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDestroyed) return;

        // Solo destruir si es una bala
        MyBullet bullet = collision.GetComponent<MyBullet>();
        if (bullet != null)
        {
            Debug.Log("¡Bala impactó la tabla! Destruyendo...");
            DestroyPlank();
        }
    }

    private void DestroyPlank()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        Debug.Log("¡Tabla destruida!");

        // Reproducir sonido
        PlaySound(breakSound);

        // Desactivar collider para permitir el paso
        if (solidCollider != null)
        {
            solidCollider.enabled = false;
        }

        // Emitir partículas
        if (woodParticles != null)
        {
            var emission = woodParticles.emission;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0.0f, 20)
            });
            woodParticles.Play();
        }

        // Efecto de temblor + fade out
        StartCoroutine(ShakeAndFadeOut());
    }

    private System.Collections.IEnumerator ShakeAndFadeOut()
    {
        // Fase 1: Temblor
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

        // Fase 2: Fade out
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            elapsed = 0f;
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
        }

        // Destruir el GameObject
        Destroy(gameObject);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
