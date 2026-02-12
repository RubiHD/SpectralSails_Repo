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
        if (hasCompletedQuest)
        {
            Debug.Log("Ya has completado la quest del barco.");
            return;
        }

        if (player != null && player.dialogueUI != null)
        {
            player.dialogueUI.ShowDialogue(initialDialogue, this);
        }
        else
        {
            Debug.LogError("Player o DialogueUI es null!");
        }
    }

    public bool HasRequirements(PlayerInventory inventory)
    {
        if (inventory == null)
        {
            Debug.LogError("Inventory es null!");
            return false;
        }

        if (inventory.coinCount < requiredCoins)
        {
            Debug.Log($"Faltan monedas. Tiene: {inventory.coinCount}/{requiredCoins}");
            return false;
        }

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

    public void CompleteQuest(PlayerInventory inventory)
    {
        if (inventory == null)
        {
            Debug.LogError("Inventory es null!");
            return;
        }

        Debug.Log("¡Quest completada! Consumiendo recursos...");

        inventory.RemoveCoins(requiredCoins);

        foreach (string itemID in requiredItems)
        {
            inventory.RemoveItem(itemID);
        }

        hasCompletedQuest = true;
        BuildShip();
    }

    private void BuildShip()
    {
        Debug.Log("🚢 ¡Barco construido! Cargando siguiente escena...");
        Invoke("LoadNextScene", 2f);
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