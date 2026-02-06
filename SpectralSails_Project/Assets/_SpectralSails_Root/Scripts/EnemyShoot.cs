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
    private bool isAttacking = false;
    private bool inRange = false;

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

        // Dirección basada en la escala del objeto
        Vector2 direction = new Vector2(Mathf.Sign(transform.localScale.x), 0);

        RaycastHit2D hit = Physics2D.Raycast(shootController.position, direction, laneDistance, playerLayer);
        inRange = hit.collider != null;

        Debug.DrawRay(shootController.position, direction * laneDistance, inRange ? Color.green : Color.red);

        if (inRange)
        {
            // Voltear sprite hacia el jugador
            float dirX = hit.collider.transform.position.x - transform.position.x;
            if (dirX != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(dirX);
                transform.localScale = scale;
            }

            if (Time.time > timeToShoot + timeLastShoot && !isAttacking)
            {
                timeLastShoot = Time.time;
                isAttacking = true;

                animator.SetTrigger("Attack"); // La animación debe tener un evento que llame a Shoot()
            }
        }
        else
        {
            animator.SetBool("isAttacking", false);
        }
    }

    // Este método debe ser llamado desde un Animation Event
    public void Shoot()
    {
        if (enemyBullet != null && shootController != null)
        {
            Instantiate(enemyBullet, shootController.position, shootController.rotation);
        }
        isAttacking = false;
    }

    public void DisableBehavior()
    {
        isDead = true;
        isAttacking = false;
        if (animator != null)
        {
            animator.SetBool("isAttacking", false);
        }
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
