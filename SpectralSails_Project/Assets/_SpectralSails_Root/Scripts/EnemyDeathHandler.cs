using UnityEngine;

public class EnemyDeathHandler : MonoBehaviour
{
    private EnemyAnimationHandler animHandler;
    private Animator animator;
    private bool isDying = false;

    private void Awake()
    {
        animHandler = GetComponent<EnemyAnimationHandler>();
        animator = GetComponent<Animator>();
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

        var shark = GetComponent<SharkEnemy>();
        if (shark != null)
        {
            shark.DisableBehavior();
        }

        // Reproducir animación de muerte
        animHandler?.PlayDeath();

        // ✅ CALCULAR DURACIÓN AUTOMÁTICAMENTE
        float deathAnimationDuration = GetDeathAnimationDuration();
        Destroy(gameObject, deathAnimationDuration + 0.1f); // +0.1f de margen
    }

    // ✅ NUEVO MÉTODO: Obtener duración de la animación de muerte
    private float GetDeathAnimationDuration()
    {
        if (animator == null) return 2f; // Valor por defecto

        // Buscar el clip de animación de muerte
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);

        foreach (AnimatorClipInfo clip in clipInfo)
        {
            if (clip.clip.name.Contains("Death") || clip.clip.name.Contains("Die"))
            {
                return clip.clip.length;
            }
        }

        // Si no encuentra, buscar en todos los clips del RuntimeAnimatorController
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name.Contains("Death") || clip.name.Contains("Die"))
            {
                return clip.length;
            }
        }

        // Valor por defecto si no encuentra la animación
        return 2f;
    }
}