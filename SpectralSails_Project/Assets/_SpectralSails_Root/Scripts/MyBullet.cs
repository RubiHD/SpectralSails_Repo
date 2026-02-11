using UnityEngine;

public class MyBullet : MonoBehaviour
{
    [Header("Normal Settings")]
    public float speed = 10f;
    public int damage = 1;
    public float lifetime = 2f;

    [Header("🌊 Underwater Settings")]
    public float underwaterSpeedMultiplier = 0.6f;
    public float underwaterLifetime = 1.5f;
    public LayerMask waterLayer;

    [Header("Tablas Destructibles")]
    public int plankDamage = 3; // 3 = rompe en un solo impacto (maxHits por defecto es 3)

    private Vector2 direction;
    private bool isUnderwater = false;
    private float currentSpeed;

    private void Start()
    {
        CheckIfUnderwater();
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
        Collider2D waterCollider = Physics2D.OverlapPoint(transform.position, waterLayer);
        isUnderwater = waterCollider != null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 🌊 Entrada al agua
        if (((1 << collision.gameObject.layer) & waterLayer) != 0)
        {
            if (!isUnderwater)
            {
                isUnderwater = true;
                currentSpeed = speed * underwaterSpeedMultiplier;
            }
            return;
        }

        // 🪵 Tablas destructibles (1 impacto de bala)
        DestructiblePlank plank = collision.GetComponent<DestructiblePlank>();
        if (plank != null)
        {
            plank.TakeDamage(plankDamage);
            Destroy(gameObject);
            return;
        }

        // Boss
        BossPirate boss = collision.GetComponent<BossPirate>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Enemigos normales
        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            EnemyAnimationHandler animHandler = collision.GetComponent<EnemyAnimationHandler>();
            animHandler?.PlayHit();
            Destroy(gameObject);
            return;
        }

        // Suelo
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}