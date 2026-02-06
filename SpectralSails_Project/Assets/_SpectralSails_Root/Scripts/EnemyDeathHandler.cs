using UnityEngine;

public class EnemyDeathHandler : MonoBehaviour
{
    private EnemyAnimationHandler animHandler;
    private bool isDying = false;

    private void Awake()
    {
        animHandler = GetComponent<EnemyAnimationHandler>();
    }

    public void Die()
    {
        if (isDying) return;

        isDying = true;

        // Desactivar comportamientos de enemigos compatibles
        var floating = GetComponent<FloatingEnemy>();
        if (floating != null)
        {
            floating.DisableBehavior();
        }

        var follower = GetComponent<EnemyFollowControl>();
        if (follower != null)
        {
            follower.DisableBehavior();
        }

        var shooter = GetComponent<EnemyShoot>();
        if (shooter != null)
        {
            shooter.DisableBehavior();
        }

        // Reproducir animación de muerte
        animHandler?.PlayDeath();

        // Destruir tras la animación
        Destroy(gameObject, 1.5f);
    }
}
