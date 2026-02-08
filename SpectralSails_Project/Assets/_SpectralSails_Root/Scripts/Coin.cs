using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int coinValue = 1; // Cuántas monedas suma esta
    [SerializeField] private float magnetRange = 2f; // Distancia a la que atrae al jugador
    [SerializeField] private float magnetSpeed = 8f; // Velocidad al ser atraída

    [Header("Efectos Visuales (Opcional)")]
    [SerializeField] private GameObject collectParticles; // Partículas al recoger
    [SerializeField] private AudioClip collectSound; // Sonido al recoger

    [Header("Animación de Flotación")] // ✅ MEJORADO
    [SerializeField] private bool enableBobbing = true; // Hacer que la moneda flote
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    [Header("Animación de Rotación")] // ✅ NUEVO
    [SerializeField] private bool enableRotation = true; // Hacer que la moneda gire
    [SerializeField] private float rotationSpeed = 50f; // Grados por segundo
    [SerializeField] private Vector3 rotationAxis = new Vector3(0f, 1f, 0f); // Eje de rotación (Y por defecto)

    private Transform player;
    private bool isBeingCollected = false;
    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;

        // Buscar al jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // ✅ ANIMACIÓN DE FLOTACIÓN
        if (enableBobbing && !isBeingCollected)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        // ✅ ANIMACIÓN DE ROTACIÓN
        if (enableRotation)
        {
            transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
        }

        // Sistema de imán: atraer hacia el jugador si está cerca
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= magnetRange && !isBeingCollected)
        {
            isBeingCollected = true;
        }

        if (isBeingCollected)
        {
            // Mover hacia el jugador
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                magnetSpeed * Time.deltaTime
            );

            // Si está muy cerca, recoger
            if (distanceToPlayer < 0.5f)
            {
                Collect();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Recoger al tocar al jugador directamente
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        // Añadir monedas al inventario
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            inventory.AddCoins(coinValue);
            Debug.Log($"¡Moneda recogida! Total: {inventory.coinCount}");
        }

        // Efectos visuales
        if (collectParticles != null)
        {
            Instantiate(collectParticles, transform.position, Quaternion.identity);
        }

        // Sonido
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // Destruir la moneda
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar rango del imán en el editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }
}