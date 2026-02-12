using UnityEngine;
using UnityEngine.SceneManagement; // Para cambiar de escena

public class ShipBuilderNPC : MonoBehaviour, IInteractable
{
    [Header("Diálogos")]
    public DialogueDataSO initialDialogue; // Primera conversación
    public DialogueDataSO successDialogue; // Cuando tiene todo
    public DialogueDataSO missingRequirementsDialogue; // Cuando le falta algo

    [Header("Requisitos")]
    public int requiredCoins = 10;
    public string[] requiredItems = { "Madera", "Velas" }; // IDs de items necesarios

    [Header("Configuración")]
    public string nextSceneName = "Victory"; // Escena que carga al terminar
    public bool hasCompletedQuest = false; // Estado de la quest

    public void Interact(PlayerController player)
    {
        if (hasCompletedQuest)
        {
            Debug.Log("Ya has completado la quest del barco.");
            return;
        }

        // Mostrar diálogo inicial
        DialogueUI dialogueUI = player.dialogueUI;
        if (dialogueUI != null)
        {
            dialogueUI.ShowDialogue(initialDialogue, this);
        }
    }

    // ✅ NUEVO: Verificar si el jugador tiene los requisitos
    public bool HasRequirements(PlayerInventory inventory)
    {
        // Verificar monedas
        if (inventory.coinCount < requiredCoins)
        {
            Debug.Log($"Faltan monedas. Tiene: {inventory.coinCount}/{requiredCoins}");
            return false;
        }

        // Verificar items
        foreach (string itemID in requiredItems)
        {
            if (!inventory.HasItem(itemID))
            {
                Debug.Log($"Falta item: {itemID}");
                return false;
            }
        }

        return true;
    }

    // ✅ NUEVO: Completar la quest
    public void CompleteQuest(PlayerInventory inventory)
    {
        Debug.Log("¡Quest completada! Consumiendo recursos...");

        // Quitar monedas
        inventory.RemoveCoins(requiredCoins);

        // Quitar items
        foreach (string itemID in requiredItems)
        {
            inventory.RemoveItem(itemID);
        }

        hasCompletedQuest = true;

        // Construir barco (aquí puedes agregar efectos visuales/sonido)
        BuildShip();
    }

    private void BuildShip()
    {
        Debug.Log("🚢 ¡Barco construido! Cargando siguiente escena...");

        // Esperar 2 segundos antes de cambiar de escena
        Invoke(nameof(LoadNextScene), 2f);
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("No se ha asignado una escena siguiente.");
        }
    }
}