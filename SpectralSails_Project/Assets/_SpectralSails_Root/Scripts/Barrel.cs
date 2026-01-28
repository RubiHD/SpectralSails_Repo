using UnityEngine;

public class Barrel : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1;
    public float lifeTime = 8f;

    private float moveDirection = 1f;


    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(Vector2.right * moveDirection * speed * Time.deltaTime);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            Destroy(gameObject);
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }

    public void SetDirection(float dir)
    {
        moveDirection = dir;
    }

}
