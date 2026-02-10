using UnityEngine;

public class MyBullet : MonoBehaviour
{
    [Header("Normal Settings")]
    public float speed = 10f;
    public int damage = 1;
    public float lifetime = 2f;

    [Header("🌊 Underwater Settings")]
    public float underwaterSpeedMultiplier = 0.6f; // Más lento bajo el agua
    public float underwaterLifetime = 1.5f; // Duración reducida
    public LayerMask waterLayer; // Layer del agua

    private Vector2 direction;
    private bool isUnderwater = false;
    private float currentSpeed;

    private void Start()
    {
        // Detectar si empieza bajo el agua
        CheckIfUnderwater();

        // Ajustar velocidad y lifetime según el contexto
        currentSpeed = isUnderwater ? speed * underwaterSpeedMultiplier : speed;
        float actualLifetime = isUnderwater ? underwaterLifetime : lifetime;

        Destroy(gameObject, actualLifetime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    private void Update()
    {
        transform.Translate(direction * currentSpeed * Time.deltaTime);
    }

    private void CheckIfUnderwater()
    {
        // Verificar si hay un collider de agua en la posición inicial
        Collider2D waterCollider = Physics2D.OverlapPoint(transform.position, waterLayer);
        isUnderwater = waterCollider != null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 🌊 Detectar entrada al agua (cambio de medio)
        if (((1 << collision.gameObject.layer) & waterLayer) != 0)
        {
            if (!isUnderwater)
            {
                // Entró al agua desde el aire
                isUnderwater = true;
                currentSpeed = speed * underwaterSpeedMultiplier;
            }
            return; // No destruir al tocar agua
        }

        // Damage boss
        BossPirate boss = collision.GetComponent<BossPirate>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Damage normal enemies
        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);

            // Reproducir animación de impacto si el enemigo tiene el componente
            EnemyAnimationHandler animHandler = collision.GetComponent<EnemyAnimationHandler>();
            if (animHandler != null)
            {
                animHandler.PlayHit();
            }

            Destroy(gameObject);
            return;
        }

        // Destroy on ground
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}