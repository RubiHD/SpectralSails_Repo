using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public int damage;
    private Vector2 moveDirection;

    private void Start()
    {
        // Determinar dirección según la escala del padre (enemigo)
        float direction = Mathf.Sign(transform.localScale.x);
        moveDirection = new Vector2(direction, 0);
    }

    private void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerHealth playerHealth))
        {
            playerHealth.TakeDamage(damage, transform.position);
            Destroy(gameObject);
        }
    }
}

