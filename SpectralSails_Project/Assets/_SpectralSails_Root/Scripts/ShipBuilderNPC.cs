using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipBuilderNPC : MonoBehaviour, IInteractable
{
    [Header("Diálogos")]
    public DialogueDataSO initialDialogue;
    public DialogueDataSO successDialogue;
    public DialogueDataSO missingRequirementsDialogue;

    [Header("Requisitos")]
    public int requiredCoins = 10;
    public string[] requiredItems = new string[] { "Madera", "Velas" };

    [Header("Configuración")]
    public string nextSceneName = "Victory";
    public bool hasCompletedQuest = false;

    public void Interact(PlayerController player)
    {
        // ✅ VALIDACIONES MEJORADAS
        if (player == null)
        {
            Debug.LogError("❌ Player es NULL en ShipBuilderNPC.Interact!");
            return;
        }

        if (hasCompletedQuest)
        {
            Debug.Log("Ya has completado la quest del barco.");
            return;
        }

        if (player.dialogueUI == null)
        {
            Debug.LogError("❌ DialogueUI NO está asignado en el PlayerController!");
            Debug.LogError("→ SOLUCIÓN: Selecciona el Player en la Jerarquía > PlayerController > Arrastra DialogueUI del Canvas al campo 'Dialogue UI'");
            return;
        }

        if (initialDialogue == null)
        {
            Debug.LogError("❌ Initial Dialogue NO está asignado en ShipBuilderNPC!");
            return;
        }

        Debug.Log($"✅ Iniciando diálogo con NPC: {gameObject.name}");
        player.dialogueUI.ShowDialogue(initialDialogue, this);
    }

    public bool HasRequirements(PlayerInventory inventory)
    {
        if (inventory == null)
        {
            Debug.LogError("❌ Inventory es null!");
            return false;
        }

        // Verificar monedas
        if (inventory.coinCount < requiredCoins)
        {
            Debug.Log($"❌ Faltan monedas. Tiene: {inventory.coinCount}/{requiredCoins}");
            return false;
        }

        // Verificar items
        foreach (string itemID in requiredItems)
        {
            if (!inventory.HasItem(itemID))
            {
                Debug.Log($"❌ Falta item: {itemID}");
                return false;
            }
        }

        Debug.Log("✅ Jugador tiene todos los requisitos!");
        return true;
    }

    public void CompleteQuest(PlayerInventory inventory)
    {
        if (inventory == null)
        {
            Debug.LogError("❌ Inventory es null!");
            return;
        }

        Debug.Log("🎉 ¡Quest completada! Consumiendo recursos...");

        // Quitar monedas
        inventory.RemoveCoins(requiredCoins);

        // Quitar items
        foreach (string itemID in requiredItems)
        {
            inventory.RemoveItem(itemID);
        }

        hasCompletedQuest = true;
        BuildShip();
    }

    private void BuildShip()
    {
        Debug.Log("🚢 ¡Barco construido! Cargando siguiente escena en 2 segundos...");
        Invoke("LoadNextScene", 2f);
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"Cargando escena: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("⚠️ No se ha asignado una escena siguiente.");
        }
    }

    // ✅ MÉTODO DE DEBUG
    private void OnDrawGizmos()
    {
        // Visualizar rango de interacción
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}