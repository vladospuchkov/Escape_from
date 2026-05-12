using UnityEngine;

public class FloorTeleporter : MonoBehaviour
{
    public Transform targetPoint;
    public string message = "Переход на другой этаж";
    private float cooldown;

    void Update() { if (cooldown > 0f) cooldown -= Time.deltaTime; }

    void OnTriggerEnter(Collider other)
    {
        if (cooldown > 0f || targetPoint == null || !other.CompareTag("Player")) return;
        var cc = other.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        other.transform.position = targetPoint.position;
        if (cc != null) cc.enabled = true;
        cooldown = 1f;
        if (GameManager.Instance != null) GameManager.Instance.SetStatus(message);
    }
}
