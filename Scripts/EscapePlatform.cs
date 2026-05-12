using UnityEngine;

public class EscapePlatform : MonoBehaviour
{
    public bool isOutsideLandingPlatform = false;
    public Transform outsideSpawnPoint;

    public void TryEscape(GameObject playerObject)
    {
        if (InventorySystem.Instance == null || !InventorySystem.Instance.HasItem("Jetpack"))
        {
            if (GameManager.Instance != null) GameManager.Instance.SetStatus("Нужен джетпак. Он лежит на третьем этаже.");
            return;
        }

        if (isOutsideLandingPlatform)
        {
            if (GameManager.Instance != null) GameManager.Instance.WinGame("ТЫ ВЫБРАЛСЯ\nПосле третьего этажа ты улетел из комплекса");
            return;
        }

        if (outsideSpawnPoint == null)
        {
            if (GameManager.Instance != null) GameManager.Instance.SetStatus("Выход наружу не настроен.");
            return;
        }

        var cc = playerObject.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        playerObject.transform.position = outsideSpawnPoint.position;
        playerObject.transform.rotation = outsideSpawnPoint.rotation;
        if (cc != null) cc.enabled = true;

        var jetpack = playerObject.GetComponent<JetpackFlightController>();
        if (jetpack != null) jetpack.ActivateJetpack();

        if (GameManager.Instance != null)
            GameManager.Instance.SetStatus("Ты выбрался наружу. Джетпак активирован: SPACE вверх, SHIFT ускорение. Лети на желтую платформу!");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) TryEscape(other.gameObject);
    }
}
