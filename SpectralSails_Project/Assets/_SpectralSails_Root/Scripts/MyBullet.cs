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

    [Header("🪵 Tablas Destructibles")]
    public int plankDamage = 3;

    [Header("🔧 Debug")]
    public bool showDebugLogs = false;

    private Vector2 direction;
    private bool isUnderwater = false;
    private float currentSpeed;
    private bool hasHit = false;
    private GameObject shooter; // ✅ NUEVO - Referencia al que disparó

    private void Start()
    {
        CheckIfUnderwater();
        currentSpeed = isUnderwater ? speed * underwaterSpeedMultiplier : speed;
        float actualLifetime = isUnderwater ? underwaterLifetime : lifetime;

        if (showDebugLogs)
            Debug.Log($"Bala creada. Underwater: {isUnderwater}, Speed: {currentSpeed}");

        Destroy(gameObject, actualLifetime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (showDebugLogs)
            Debug.Log($"Bala dirección establecida: {direction}");
    }

    // ✅ NUEVO MÉTODO: Establecer quién disparó
    public void SetShooter(GameObject shooterObject)
    {
        shooter = shooterObject;

        // Ignorar colisión con el shooter temporalmente
        if (shooter != null)
        {
            Collider2D shooterCollider = shooter.GetComponent<Collider2D>();
            Collider2D bulletCollider = GetComponent<Collider2D>();

            if (shooterCollider != null && bulletCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, shooterCollider, true);
            }
        }
    }

    private void Update()
    {
        if (!hasHit)
        {
            transform.Translate(direction * currentSpeed * Time.deltaTime, Space.World);
        }
    }

    private void CheckIfUnderwater()
    {
        Collider2D waterCollider = Physics2D.OverlapPoint(transform.position, waterLayer);
        isUnderwater = waterCollider != null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        // ✅ Ignorar al jugador que disparó
        if (shooter != null && collision.gameObject == shooter)
        {
            if (showDebugLogs)
                Debug.Log("[BALA] Ignorando colisión con el shooter");
            return;
        }

        if (showDebugLogs)
            Debug.Log($"[BALA] Colisión con: {collision.gameObject.name}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");

        // 🌊 Entrada al agua
        if (((1 << collision.gameObject.layer) & waterLayer) != 0)
        {
            if (!isUnderwater)
            {
                isUnderwater = true;
                currentSpeed = speed * underwaterSpeedMultiplier;

                if (showDebugLogs)
                    Debug.Log("[BALA] Entró al agua, velocidad reducida");
            }
            return;
        }

        // 🪵 TABLAS DESTRUCTIBLES
        DestructiblePlank plank = collision.GetComponent<DestructiblePlank>();
        if (plank != null)
        {
            if (showDebugLogs)
                Debug.Log($"[BALA] ✅ Golpeó tabla! Daño: {plankDamage}");

            plank.TakeDamage(plankDamage);
            hasHit = true;
            Destroy(gameObject);
            return;
        }

        // 🏴‍☠️ Boss
        BossPirate boss = collision.GetComponent<BossPirate>();
        if (boss != null)
        {
            if (showDebugLogs)
                Debug.Log($"[BALA] ✅ Golpeó al Boss! Daño: {damage}");

            boss.TakeDamage(damage);
            hasHit = true;
            Destroy(gameObject);
            return;
        }

        // 👹 Enemigos normales
        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            if (showDebugLogs)
                Debug.Log($"[BALA] ✅ Golpeó enemigo: {collision.gameObject.name}! Daño: {damage}");

            enemy.TakeDamage(damage);

            EnemyAnimationHandler animHandler = collision.GetComponent<EnemyAnimationHandler>();
            animHandler?.PlayHit();

            hasHit = true;
            Destroy(gameObject);
            return;
        }

        // 🧱 Suelo / Paredes
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            if (showDebugLogs)
                Debug.Log("[BALA] Chocó con terreno");

            hasHit = true;
            Destroy(gameObject);
            return;
        }

        // ⚠️ Colisión no manejada
        if (showDebugLogs)
            Debug.LogWarning($"[BALA] Colisión no manejada con: {collision.gameObject.name}, Tag: {collision.tag}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
    }
}