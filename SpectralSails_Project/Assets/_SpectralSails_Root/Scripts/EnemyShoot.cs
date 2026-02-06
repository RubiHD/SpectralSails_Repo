using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    public Transform shootController;
    public float laneDistance = 5f;
    public LayerMask playerLayer;
    public float timeToShoot = 2f;
    public float timeLastShoot = 0f;
    public GameObject enemyBullet;
    public Animator animator;

    private bool isDead = false;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator no asignado en " + gameObject.name);
            }
        }
    }

    private void Update()
    {
        if (isDead || animator == null) return;

        Vector2 direction = new Vector2(Mathf.Sign(transform.localScale.x), 0);
        RaycastHit2D hit = Physics2D.Raycast(shootController.position, direction, laneDistance, playerLayer);
        bool inRange = hit.collider != null;

        Debug.DrawRay(shootController.position, direction * laneDistance, inRange ? Color.green : Color.red);

        if (inRange && Time.time > timeToShoot + timeLastShoot)
        {
            // Voltear hacia el jugador
            float dirX = hit.collider.transform.position.x - transform.position.x;
            if (dirX != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(dirX);
                transform.localScale = scale;
            }

            animator.SetTrigger("Attack");
        }
    }

    // Este método debe ser llamado desde un Animation Event
    public void Shoot()
    {
        Debug.Log("¡Daga lanzada!");
        if (enemyBullet != null && shootController != null)
        {
            Instantiate(enemyBullet, shootController.position, shootController.rotation);
        }

        timeLastShoot = Time.time; // Se actualiza aquí para controlar bien el ritmo de disparo
    }

    public void DisableBehavior()
    {
        isDead = true;
    }

    private void OnDrawGizmos()
    {
        if (shootController != null)
        {
            Vector2 direction = new Vector2(Mathf.Sign(transform.localScale.x), 0);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(shootController.position, shootController.position + (Vector3)(direction * laneDistance));
        }
    }
}

