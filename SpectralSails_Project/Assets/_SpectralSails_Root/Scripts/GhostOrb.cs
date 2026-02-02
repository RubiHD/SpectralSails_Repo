using UnityEngine;

public class GhostOrb : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 direction;

    public void Initialize(Vector3 targetPosition)
    {
        direction = (targetPosition - transform.position).normalized;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
                player.TakeDamage(1); // o el daño que quieras

            Destroy(gameObject);
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}

