using UnityEngine;

public abstract class Sword : MonoBehaviour
{
    public string swordName;
    public int damage;
    public RuntimeAnimatorController animatorOverride;

    public abstract void Attack(PlayerCombat player);
}
