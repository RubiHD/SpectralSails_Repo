using UnityEngine;

public class GhostMouth : MonoBehaviour
{
    public float extendSpeed = 10f;
    public float maxLifetime = 2f;
    public int damage = 2;

    private Vector3 targetPosition;
    private bool hasAttacked = false;

    public void Initialize(Vector3 target)
    {
        targetPosition = target;
        Vector3 direction = (target - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        if (!hasAttacked)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, extendSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                hasAttacked = true;
                GetComponent<Animator>().SetTrigger("Close"); // lanza animación de cierre
            }
        }
    }

    // Llamado desde un Animation Event en el frame de cierre
    public void DealDamage()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Player"));
        if (hit != null)
        {
            PlayerHealth player = hit.GetComponent<PlayerHealth>();
            if (player != null)
                player.TakeDamage(damage);
        }
    }

    // Llamado al final de la animación
    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}