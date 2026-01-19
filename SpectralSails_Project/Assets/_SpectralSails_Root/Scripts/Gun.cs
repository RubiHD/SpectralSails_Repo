using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGun : MonoBehaviour
{
    [Header("Gun Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int ammo = 0;
    public float fireCooldown = 0.2f;

    private bool canShoot = true;
    private PlayerController player;

    private void Start()
    {
        player = GetComponent<PlayerController>();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (!canShoot || ammo <= 0)
            return;

        Shoot();
    }

    private void Shoot()
    {
        canShoot = false;
        ammo--;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // Dirección según hacia dónde mira el jugador
        float dir = Mathf.Sign(transform.localScale.x);

        bullet.GetComponent<MyBullet>().SetDirection(new Vector2(dir, 0));

        Invoke(nameof(ResetShoot), fireCooldown);
    }



    private void ResetShoot()
    {
        canShoot = true;
    }

    public void AddAmmo(int amount)
    {
        ammo += amount;
    }
}

