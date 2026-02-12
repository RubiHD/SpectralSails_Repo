using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Espadas disponibles")]
    public List<Sword> swords = new List<Sword>();
    private int currentSwordIndex = 0;

    [Header("Animator del jugador")]
    public Animator animator;

    [Header("Input Actions")]
    public InputAction attackAction;
    public InputAction switchSwordAction;

    private void OnEnable()
    {
        attackAction.Enable();
        switchSwordAction.Enable();
    }

    private void OnDisable()
    {
        attackAction.Disable();
        switchSwordAction.Disable();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && swords.Count > 0)
        {
            if (animator != null)
            {
                if (swords[currentSwordIndex] is AdvancedSword)
                {
                    animator.SetTrigger("attackAdvanced");
                }
                else
                {
                    animator.SetTrigger("attackBasic");
                }
            }
        }
    }

    public void ApplyAttackDamage()
    {
        swords[currentSwordIndex].Attack(this);
    }

    public void OnSwitchSword(InputAction.CallbackContext context)
    {
        if (context.performed && swords.Count > 1)
        {
            SwitchSword();
        }
    }

    private void SwitchSword()
    {
        currentSwordIndex = (currentSwordIndex + 1) % swords.Count;

        if (animator != null && swords[currentSwordIndex].animatorOverride != null)
        {
            animator.runtimeAnimatorController = swords[currentSwordIndex].animatorOverride;
        }

        Debug.Log("Espada equipada: " + swords[currentSwordIndex].swordName);
    }

    public void AddSword(Sword newSword)
    {
        if (!swords.Contains(newSword))
        {
            swords.Add(newSword);
            Debug.Log("Espada recogida: " + newSword.swordName);
        }
    }

    public bool HasSword(Sword sword)
    {
        return swords.Contains(sword);
    }

    // ✅ NUEVO MÉTODO - Para que SwordUI pueda acceder al índice actual
    public int GetCurrentSwordIndex()
    {
        return currentSwordIndex;
    }

    // ✅ MÉTODO ACTUALIZADO - Ahora detecta tablas destructibles
    public void DealDamage(int amount)
    {
        Vector2 direction = new Vector2(Mathf.Sign(transform.localScale.x), 0);
        Vector2 attackPosition = (Vector2)transform.position + direction * 1f;
        float radius = 1f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPosition, radius);

        foreach (var hit in hits)
        {
            // ✅ NUEVO: Detectar tablas destructibles
            DestructiblePlank plank = hit.GetComponent<DestructiblePlank>();
            if (plank != null)
            {
                plank.TakeDamage(amount);
                continue; // Pasar al siguiente objeto
            }

            // Enemigos normales
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(amount);

                // Reproducir animación de impacto si tiene el componente
                if (hit.TryGetComponent(out EnemyAnimationHandler animHandler))
                {
                    animHandler.PlayHit();
                }
                continue;
            }

            // Boss
            BossPirate boss = hit.GetComponent<BossPirate>();
            if (boss != null)
            {
                boss.TakeDamage(amount);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 direction = new Vector2(Mathf.Sign(transform.localScale.x), 0);
        Vector2 attackPosition = (Vector2)transform.position + direction * 1f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPosition, 1f);
    }
}