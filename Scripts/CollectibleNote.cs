using UnityEngine;

public class CollectibleNote : MonoBehaviour
{
    [TextArea] public string text;
    public string inventoryName = "Записка";

    public void Read()
    {
        if (GameManager.Instance != null) GameManager.Instance.ShowNote(text);
        if (InventorySystem.Instance != null) InventorySystem.Instance.AddItem(inventoryName);
        Destroy(gameObject, 0.05f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            if (GameManager.Instance != null) GameManager.Instance.SetStatus("Нажми E, чтобы прочитать записку.");
    }
}
