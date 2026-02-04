using UnityEngine;

public class ShipBuilderNPC : MonoBehaviour, IInteractable
{
    [Header("Diálogo")]
    public DialogueDataSO initialDialogue;
    public DialogueDataSO finalDialogue;
    public DialogueDataSO missingRequirementsDialogue;

    [Header("Requisitos")]
    public int requiredCoins = 10;
    public string requiredItemID = "BossCore";

    private bool finalPhase = false;

    public void Interact(PlayerController player)
    {
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();

        if (finalPhase)
        {
            if (inventory.HasItem(requiredItemID) && inventory.coinCount >= requiredCoins)
            {
                inventory.RemoveItem(requiredItemID);
                inventory.RemoveCoins(requiredCoins);
                player.StartDialogue(finalDialogue);
                BuildShip();
            }
            else
            {
                player.StartDialogue(missingRequirementsDialogue);
            }
        }
        else
        {
            player.StartDialogue(initialDialogue);
        }
    }

    public void ActivateFinalPhase()
    {
        finalPhase = true;
    }

    private void BuildShip()
    {
        Debug.Log("¡Barco construido! Puedes zarpar.");
        // Aquí puedes lanzar una animación, cambiar de escena, etc.
    }
}