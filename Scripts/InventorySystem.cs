using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;
    public Text inventoryText;
    private readonly HashSet<string> items = new HashSet<string>();

    void Awake() { Instance = this; }
    void Start() { RefreshUI(); }

    public void AddItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;
        items.Add(itemId);
        RefreshUI();
    }

    public bool HasItem(string itemId) => items.Contains(itemId);

    public void RefreshUI()
    {
        if (inventoryText == null) return;
        string card = items.Contains("Keycard") ? "есть" : "нет";
        string jetpack = items.Contains("Jetpack") ? "есть" : "нет";
        inventoryText.text = "КЛЮЧ-КАРТА: " + card.ToUpper() + "\nДЖЕТПАК: " + jetpack.ToUpper();
    }
}
