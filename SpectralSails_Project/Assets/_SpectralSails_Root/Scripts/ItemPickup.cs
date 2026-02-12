using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Configuración del Item")]
    [SerializeField] private string itemID = "Madera";
    [SerializeField] private string itemName = "Madera";

    [Header("Efectos Visuales (Opcional)")]
    [SerializeField] private GameObject collectParticles;
    [SerializeField] private AudioClip pickupSound;

    [Header("Animación de Flotación")]
    [SerializeField] private bool enableBobbing = true;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    [Header("Animación de Rotación")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationSpeed = 50f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;

        // ✅ Verificar que tenga collider
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"❌ {gameObject.name} no tiene Collider2D! Añadiendo BoxCollider2D automáticamente.");
            BoxCollider2D newCol = gameObject.AddComponent<BoxCollider2D>();
            newCol.isTrigger = true;
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"⚠️ {gameObject.name} tiene Collider pero NO es Trigger. Corrigiendo...");
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        // ✅ ANIMACIÓN DE FLOTACIÓN
        if (enableBobbing)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        // ✅ ANIMACIÓN DE ROTACIÓN
        if (enableRotation)
        {
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collect(other.gameObject);
        }
    }

    private void Collect(GameObject player)
    {
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();

        if (inventory != null)
        {
            inventory.AddItem(itemID);
            Debug.Log($"✅ {itemName} recogido! (ID: {itemID})");

            // ✅ EFECTOS
            if (collectParticles != null)
            {
                Instantiate(collectParticles, transform.position, Quaternion.identity);
            }

            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // ✅ DESTRUIR EL OBJETO
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("❌ El jugador NO tiene PlayerInventory!");
        }
    }
}