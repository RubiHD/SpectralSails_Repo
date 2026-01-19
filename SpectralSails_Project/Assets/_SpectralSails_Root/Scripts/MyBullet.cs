using UnityEngine;

public class MyBullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifetime = 2f;

    private Vector2 direction;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si el enemigo tiene vida, aquí puedes añadir daño
        var enemy = collision.GetComponent<FloatingEnemy>();
        if (enemy != null)
        {
            // enemy.TakeDamage(damage);  <-- si luego quieres añadir vida al enemigo
            Destroy(gameObject);
        }

        // Si choca con el suelo u otro obstáculo
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
