using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    public Transform cameraRoot;
    public float interactDistance = 2.6f;
    public Text promptText;

    void Start()
    {
        if (cameraRoot == null && Camera.main != null) cameraRoot = Camera.main.transform;
    }

    void Update()
    {
        if (PauseMenu.IsPaused) return;
        UpdatePrompt();
        if (Input.GetKeyDown(KeyCode.E)) TryInteract();
    }

    void UpdatePrompt()
    {
        if (promptText == null || cameraRoot == null) return;
        promptText.text = "";
        if (!Physics.Raycast(cameraRoot.position, cameraRoot.forward, out RaycastHit hit, interactDistance, ~0, QueryTriggerInteraction.Collide)) return;

        var item = hit.collider.GetComponentInParent<InteractableItem>();
        if (item != null) { promptText.text = "E — взять: " + item.displayName; return; }

        var note = hit.collider.GetComponentInParent<CollectibleNote>();
        if (note != null) { promptText.text = "E — прочитать записку"; return; }

        var door = hit.collider.GetComponentInParent<OpenableDoor>();
        if (door != null)
        {
            if (door.locked && !door.PlayerHasRequiredItem())
                promptText.text = "ЗАКРЫТО — нужна " + door.requiredItemDisplayName;
            else
                promptText.text = door.isOpen ? "E — закрыть дверь" : "E — открыть дверь";
            return;
        }

        var exit = hit.collider.GetComponentInParent<EscapePlatform>();
        if (exit != null) { promptText.text = "E — улететь на джетпаке"; return; }
    }

    void TryInteract()
    {
        if (cameraRoot == null) return;
        if (Physics.Raycast(cameraRoot.position, cameraRoot.forward, out RaycastHit hit, interactDistance, ~0, QueryTriggerInteraction.Collide))
        {
            var item = hit.collider.GetComponentInParent<InteractableItem>();
            if (item != null) { item.Interact(); return; }

            var note = hit.collider.GetComponentInParent<CollectibleNote>();
            if (note != null) { note.Read(); return; }

            var openableDoor = hit.collider.GetComponentInParent<OpenableDoor>();
            if (openableDoor != null) { openableDoor.Interact(); return; }

            var platform = hit.collider.GetComponentInParent<EscapePlatform>();
            if (platform != null) { platform.TryEscape(gameObject); return; }
        }
        if (GameManager.Instance != null) GameManager.Instance.SetStatus("Здесь нечего использовать.");
    }
}
