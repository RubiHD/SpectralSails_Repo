using UnityEngine;

public class EnemyDeathHandler : MonoBehaviour
{
    [Header("Configuración de Muerte")]
    public float destroyDelay = 2f; // Tiempo antes de destruir el objeto
    public bool disableColliderOnDeath = true;
    public bool disableAIOnDeath = true;

    private Animator animator;
    private Collider enemyCollider;
    private TiburonAI tiburonAI;

    private void Start()
    {
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider>();
        tiburonAI = GetComponent<TiburonAI>();
    }

    public void Die()
    {
        // Desactivar el collider para que no pueda ser golpeado más
        if (disableColliderOnDeath && enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        // Desactivar la IA
        if (disableAIOnDeath && tiburonAI != null)
        {
            tiburonAI.OnDeath();
        }

        // Reproducir animación de muerte
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Destruir el objeto después del delay
        Destroy(gameObject, destroyDelay);
    }
}
