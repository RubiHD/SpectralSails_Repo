using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGun : MonoBehaviour
{
    [Header("Gun Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int ammo = 0;
    public float fireCooldown = 0.2f;

    [Header("🌊 Underwater Settings")]
    public GameObject waterBulletPrefab; // Prefab diferente para agua (opcional)
    public float waterFireCooldown = 0.3f; // Cooldown diferente bajo el agua

    private bool canShoot = true;
    private PlayerController player;
    private Animator animator;

    private void Start()
    {
        player = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();

        if (player == null)
        {
            Debug.LogError("PlayerController no encontrado!");
        }
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

        // 🌊 Verificar si está bajo el agua
        bool isUnderwater = player != null && player.IsUnderwater();

        if (isUnderwater)
        {
            ShootUnderwater();
        }
        else
        {
            ShootNormal();
        }
    }

    // 🏝️ DISPARO NORMAL (TERRESTRE)
    private void ShootNormal()
    {
        canShoot = false;
        ammo--;

        // ✅ Activar animación de disparo NORMAL
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }

        Invoke(nameof(ResetShoot), fireCooldown);
    }

    // 🌊 DISPARO BAJO EL AGUA
    private void ShootUnderwater()
    {
        canShoot = false;
        ammo--;

        // ✅ Activar animación de disparo ACUÁTICO
        if (animator != null)
        {
            animator.SetTrigger("WaterShoot");
        }

        Invoke(nameof(ResetShoot), waterFireCooldown);
    }

    // ✅ Este método lo llama el ANIMATION EVENT
    public void FireBullet()
    {
        bool isUnderwater = player != null && player.IsUnderwater();

        // Elegir el prefab correcto
        GameObject prefabToUse = isUnderwater && waterBulletPrefab != null
            ? waterBulletPrefab
            : bulletPrefab;

        GameObject bullet = Instantiate(prefabToUse, firePoint.position, Quaternion.identity);
        float dir = Mathf.Sign(transform.localScale.x);

        MyBullet bulletScript = bullet.GetComponent<MyBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(new Vector2(dir, 0));
        }
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