using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : MonoBehaviour
{
    [Header("Monedas")]
    public int coinCount = 0;

    [Header("Items")]
    private HashSet<string> items = new HashSet<string>();

    [Header("Eventos")] // ✅ NUEVO: Para notificar cambios
    public UnityEvent<int> OnCoinsChanged; // Evento cuando cambian las monedas
    public UnityEvent<string> OnItemAdded; // Evento cuando se añade un item

    // Métodos de items
    public bool HasItem(string id) => items.Contains(id);

    public void AddItem(string id)
    {
        items.Add(id);
        OnItemAdded?.Invoke(id);
        Debug.Log($"Item añadido: {id}");
    }

    public void RemoveItem(string id)
    {
        items.Remove(id);
        Debug.Log($"Item removido: {id}");
    }

    // ✅ NUEVO: Método para añadir monedas
    public void AddCoins(int amount)
    {
        coinCount += amount;
        OnCoinsChanged?.Invoke(coinCount);
        Debug.Log($"Monedas añadidas: +{amount}. Total: {coinCount}");
    }

    // Método para remover monedas
    public void RemoveCoins(int amount)
    {
        int previousCount = coinCount;
        coinCount = Mathf.Max(0, coinCount - amount);

        if (coinCount != previousCount)
        {
            OnCoinsChanged?.Invoke(coinCount);
            Debug.Log($"Monedas gastadas: -{amount}. Total: {coinCount}");
        }
    }

    // ✅ NUEVO: Verificar si tiene suficientes monedas
    public bool HasEnoughCoins(int amount)
    {
        return coinCount >= amount;
    }

    // ✅ NUEVO: Método de debug para ver inventario
    [ContextMenu("Debug Inventory")]
    public void DebugInventory()
    {
        Debug.Log($"=== INVENTARIO ===");
        Debug.Log($"Monedas: {coinCount}");
        Debug.Log($"Items ({items.Count}): {string.Join(", ", items)}");
    }
}