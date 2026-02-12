using UnityEngine;

public class BossDropItem : MonoBehaviour
{
    [Header("Item a Dropear")]
    [SerializeField] private GameObject itemPrefab; // Prefab del item (con ItemPickup)
    [SerializeField] private string itemID = "Velas";

    [Header("Configuración del Drop")]
    [SerializeField] private Vector3 dropOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private float dropForce = 3f;
    [SerializeField] private bool addPhysics = true;

    // ✅ LLAMAR ESTE MÉTODO CUANDO EL BOSS MUERA
    public void DropItem()
    {
        if (itemPrefab == null)
        {
            Debug.LogError("❌ Item Prefab NO asignado en BossDropItem!");
            return;
        }

        Vector3 dropPosition = transform.position + dropOffset;
        GameObject droppedItem = Instantiate(itemPrefab, dropPosition, Quaternion.identity);

        Debug.Log($"🎁 Boss dropeó: {itemID} en posición {dropPosition}");

        // ✅ OPCIONAL: Añadir física para que "salga volando"
        if (addPhysics)
        {
            Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();

            if (rb == null)
            {
                rb = droppedItem.AddComponent<Rigidbody2D>();
            }

            // Configurar física temporal
            rb.gravityScale = 1f;
            rb.linearVelocity = new Vector2(Random.Range(-2f, 2f), dropForce);

            // ✅ DESPUÉS DE 1 SEGUNDO, DESACTIVAR FÍSICA
            StartCoroutine(DisablePhysicsAfterDelay(rb, 1f));
        }
    }

    private System.Collections.IEnumerator DisablePhysicsAfterDelay(Rigidbody2D rb, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
    }
}