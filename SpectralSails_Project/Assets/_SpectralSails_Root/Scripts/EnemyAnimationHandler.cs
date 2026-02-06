using UnityEngine;

public class EnemyAnimationHandler : MonoBehaviour
{
    private Animator animator;
    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayHit()
    {
        if (animator != null && !isDead)
        {
            animator.SetTrigger("Hit");
        }
    }

    public void PlayDeath()
    {
        if (animator != null && !isDead)
        {
            isDead = true;
            animator.SetTrigger("Die");
        }
    }
}
