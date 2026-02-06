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

        // Buscar y desactivar cualquier comportamiento de enemigo compatible
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

        // Reproducir animación de muerte
        animHandler?.PlayDeath();

        // Destruir tras la animación
        Destroy(gameObject, 1.5f);
    }
}
