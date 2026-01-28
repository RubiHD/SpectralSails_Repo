public class BasicSword : Sword
{
    public override void Attack(PlayerCombat player)
    {
        

        // Detectar enemigos y aplicar daño
        player.DealDamage(damage);
    }
}
