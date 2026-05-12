using UnityEngine;

public class GuardHearing : MonoBehaviour
{
    public GuardAI guard;
    public Transform player;
    public float runningNoiseDistance = 7f;

    void Update()
    {
        if (guard == null || player == null) return;
        bool running = Input.GetKey(KeyCode.LeftShift) && (Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical")) > 0.2f);
        if (running && Vector3.Distance(transform.position, player.position) <= runningNoiseDistance)
            guard.Investigate(player.position);
    }
}
