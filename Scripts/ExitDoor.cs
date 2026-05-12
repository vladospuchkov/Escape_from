using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public void TryOpen()
    {
        if (GameManager.Instance != null) GameManager.Instance.TryExit();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) TryOpen();
    }
}
