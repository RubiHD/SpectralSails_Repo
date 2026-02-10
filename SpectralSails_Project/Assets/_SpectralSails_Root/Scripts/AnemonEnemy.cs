using UnityEngine;

public class AnemonEnemy : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    private PlayerController playerController;
    public GameObject missilePrefab; // Prefab del misil
    public Transform shootPoint; // Punto desde donde dispara

    [Header("Detección")]
    public float detectionRadius = 10f;
    public float shootCooldown = 3f;

    [Header("🔧 DEBUG")]
    public bool ignoreWaterCheck = false;

    private Animator animator;
    private float lastShootTime;
    private bool isDead = false;
    private bool isShooting = false;

    private void Start()
    {
        animator = GetComponent<Animator>();

        // Buscar al jugador
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerController = player.GetComponent<PlayerController>();
            }
        }

        // Si no hay shootPoint, usar la posición del enemigo
        if (shootPoint == null)
        {
            shootPoint = transform;
        }
    }

    private void Update()
    {
        if (isDead || player == null || isShooting) return;

        // Solo disparar si el jugador está bajo el agua
        if (!ignoreWaterCheck && playerController != null && !playerController.IsUnderwater())
        {
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Si el jugador está en rango y ha pasado el cooldown
        if (distanceToPlayer <= detectionRadius && Time.time >= lastShootTime + shootCooldown)
        {
            Shoot();
            lastShootTime = Time.time;
        }

        // Voltear hacia el jugador
        if (player.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }

    private void Shoot()
    {
        isShooting = true;

        // Activar animación de disparo
        animator?.SetTrigger("Shoot");

        // El misil se instancia desde un Animation Event en el frame correcto
        // Ver método ShootMissile() más abajo
    }

    // ✅ LLAMADO DESDE ANIMATION EVENT EN EL FRAME DEL DISPARO
    public void ShootMissile()
    {
        if (isDead || player == null || missilePrefab == null) return;

        // Instanciar el misil
        GameObject missile = Instantiate(missilePrefab, shootPoint.position, Quaternion.identity);

        // Configurar el misil
        HomingMissile homingScript = missile.GetComponent<HomingMissile>();
        if (homingScript != null)
        {
            homingScript.SetTarget(player);
        }

        Debug.Log("¡Anémona disparó un misil!");
    }

    // ✅ LLAMADO DESDE ANIMATION EVENT AL FINAL DE LA ANIMACIÓN DE DISPARO
    public void OnShootAnimationEnd()
    {
        isShooting = false;
    }

    public void DisableBehavior()
    {
        isDead = true;
        isShooting = false;

        // La anémona no tiene Rigidbody2D ya que no se mueve
        // Solo detener animaciones
        animator?.SetBool("isShooting", false);
    }

    private void OnDrawGizmosSelected()
    {
        // Radio de detección
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Punto de disparo
        if (shootPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(shootPoint.position, 0.3f);
        }
    }
}