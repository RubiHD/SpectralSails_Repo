using UnityEngine;

public class Ladder : MonoBehaviour, IInteractable
{
    public void Interact(PlayerController player)
    {
        player.StartClimbingLadder(this);
    }
}
