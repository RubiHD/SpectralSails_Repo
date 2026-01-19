using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    public float speed;

    public int damage;

    private void Update()
    {
        transform.Translate(Time.deltaTime * speed * Vector2.right);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerHealth playerHealth))
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
