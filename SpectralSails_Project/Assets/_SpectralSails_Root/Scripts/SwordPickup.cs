using UnityEngine;

public class SwordPickup : MonoBehaviour
{
    public Sword swordToGive; // arrastra aquí el prefab de la espada buena

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerCombat player = other.GetComponent<PlayerCombat>();
        if (player != null && !player.HasSword(swordToGive))
        {
            player.AddSword(swordToGive);
            Destroy(gameObject); // elimina el pickup del suelo
        }
    }
}
