using UnityEngine;

public class AdvancedSword : Sword


{

    public override void Attack(PlayerCombat player)
    {
       

        // Detectar enemigos y aplicar daño
        player.DealDamage(damage);
    }
}
