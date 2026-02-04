using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int coinCount = 0;
    private HashSet<string> items = new HashSet<string>();

    public bool HasItem(string id) => items.Contains(id);
    public void AddItem(string id) => items.Add(id);
    public void RemoveItem(string id) => items.Remove(id);
    public void RemoveCoins(int amount) => coinCount = Mathf.Max(0, coinCount - amount);
}