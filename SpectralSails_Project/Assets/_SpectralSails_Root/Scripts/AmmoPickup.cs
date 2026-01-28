using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public int ammoAmount = 5;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerGun gun = collision.GetComponent<PlayerGun>();

        if (gun != null)
        {
            gun.AddAmmo(ammoAmount);
            Destroy(gameObject);
        }
    }
}

