using UnityEngine;

public class HorrorTrigger : MonoBehaviour
{
    public GameObject objectToEnable;
    public AudioSource sound;
    public string message = "Что-то рядом...";
    bool used;

    void OnTriggerEnter(Collider other)
    {
        if (used || !other.CompareTag("Player")) return;
        used = true;
        if (objectToEnable != null) objectToEnable.SetActive(true);
        if (sound != null) sound.Play();
        if (GameManager.Instance != null) GameManager.Instance.SetStatus(message);
    }
}
