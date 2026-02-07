using UnityEngine;
using System.Collections;

public class FloatingEnemy : MonoBehaviour
{
    [Header("Floating Movement")]
    public float floatAmplitude = 0.5f;
    public float floatSpeed = 2f;

    [Header("Damage Settings")]
    public int damageAmount = 1;

    private Vector3 startPos;
    private Animator animator;
    private bool isDead = false;

    private void Start()
    {
        startPos = transform.position;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isDead) return; // Detiene el movimiento si está muerto

        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHealth player = collision.GetComponent<PlayerHealth>();
        PlayerController controller = collision.GetComponent<PlayerController>();

        if (player != null)
        {
            player.TakeDamage(damageAmount, transform.position);

            if (animator != null)
            {
                StartCoroutine(PlayAttackAnimation());
            }
        }
    }


    private IEnumerator PlayAttackAnimation()
    {
        animator.SetBool("isAttacking", true);
        yield return new WaitForSeconds(0.5f); // Ajusta al tiempo real del clip GhostAttack
        animator.SetBool("isAttacking", false);
    }

    // Este método lo llama EnemyDeathHandler para detener el movimiento
    public void DisableBehavior()
    {
        isDead = true;
    }
}
