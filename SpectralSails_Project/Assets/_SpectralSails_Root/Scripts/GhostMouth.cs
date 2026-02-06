using UnityEngine;

public class GhostMouth : MonoBehaviour
{
    public float extendSpeed = 10f;
    public float maxLifetime = 2f;
    public int damage = 2;

    private Vector3 targetPosition;
    private bool hasAttacked = false;

    [SerializeField] private Animator mouthAnimator;


    public void Initialize(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Desplazar un poco hacia adelante
        float offsetDistance = 0.5f;
        transform.position += direction * offsetDistance;

        if (mouthAnimator != null)
            mouthAnimator.SetTrigger("Close");
    }







    // Llamado desde un Animation Event en el frame de cierre
    public void DealDamage()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 1.0f, LayerMask.GetMask("Player"));
        if (hit != null)
        {
            PlayerHealth player = hit.GetComponent<PlayerHealth>();
            if (player != null)
                player.TakeDamage(1, transform.position);

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