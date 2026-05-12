using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    public string itemId = "Keycard";
    public string displayName = "Ключ-карта";
    public string pickupMessage = "Предмет добавлен";
    public AudioSource pickupSound;
    public int ammoAmount = 6;
    public float batteryAmount = 35f;
    [TextArea] public string noteText;
    public bool addToInventory = true;

    public void Interact()
    {
        if (itemId == "Ammo")
        {
            var gun = FindObjectOfType<GunController>();
            if (gun != null) gun.AddAmmo(ammoAmount);
            if (GameManager.Instance != null) GameManager.Instance.SetStatus("Патроны найдены: +" + ammoAmount);
            FinishPickup();
            return;
        }

        if (itemId == "Battery")
        {
            var flashlight = FindObjectOfType<FlashlightController>();
            if (flashlight != null) flashlight.battery = Mathf.Clamp(flashlight.battery + batteryAmount, 0f, 100f);
            if (GameManager.Instance != null) GameManager.Instance.SetStatus("Батарейка фонарика заряжена: +" + Mathf.RoundToInt(batteryAmount) + "%");
            FinishPickup();
            return;
        }

        if (itemId == "SecretFile")
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetStatus(pickupMessage);
                if (!string.IsNullOrEmpty(noteText)) GameManager.Instance.ShowNote(noteText);
            }
            if (InventorySystem.Instance != null && addToInventory) InventorySystem.Instance.AddItem(itemId);
            FinishPickup();
            return;
        }

        if (InventorySystem.Instance != null && addToInventory) InventorySystem.Instance.AddItem(itemId);
        if (GameManager.Instance != null)
        {
            if (itemId == "Keycard") GameManager.Instance.PickKey();
            else GameManager.Instance.SetStatus(pickupMessage);
        }
        FinishPickup();
    }

    void FinishPickup()
    {
        if (pickupSound != null) pickupSound.Play();
        Destroy(gameObject);
    }
}
