using UnityEngine;

public class OpenableDoor : MonoBehaviour
{
    public float openAngle = 95f;
    public float openSpeed = 4f;
    public bool isOpen;
    public bool locked;
    public string requiredItemId = "";
    public string requiredItemDisplayName = "ключ-карта";
    public string lockedMessage = "Дверь заперта.";

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private AudioSource audioSource;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0f, openAngle, 0f));
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        Quaternion target = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * openSpeed);
    }

    public bool PlayerHasRequiredItem()
    {
        return string.IsNullOrEmpty(requiredItemId) || (InventorySystem.Instance != null && InventorySystem.Instance.HasItem(requiredItemId));
    }

    public void Interact()
    {
        if (locked)
        {
            if (!PlayerHasRequiredItem())
            {
                if (GameManager.Instance != null) GameManager.Instance.SetStatus(lockedMessage);
                return;
            }
            locked = false;
            if (GameManager.Instance != null) GameManager.Instance.SetStatus("Замок открыт. Дверь можно открыть.");
        }
        isOpen = !isOpen;
        if (audioSource != null) audioSource.Play();
        if (GameManager.Instance != null) GameManager.Instance.SetStatus(isOpen ? "Дверь открыта." : "Дверь закрыта.");
    }
}
