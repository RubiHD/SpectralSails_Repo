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

    private Animator animator;


    private void Start()
    {
        player = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();

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

        if (animator != null)
            animator.SetTrigger("Shoot");

        Invoke(nameof(ResetShoot), fireCooldown);
    }

    public void FireBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        float dir = Mathf.Sign(transform.localScale.x);
        bullet.GetComponent<MyBullet>().SetDirection(new Vector2(dir, 0));
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

