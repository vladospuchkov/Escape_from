using UnityEngine;

public class KeyItem : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var item = GetComponent<InteractableItem>();
        if (item != null) item.Interact();
        else
        {
            if (GameManager.Instance != null) GameManager.Instance.PickKey();
            Destroy(gameObject);
        }
    }
}
