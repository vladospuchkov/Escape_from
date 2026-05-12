using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public Light flashlight;
    public AudioSource clickSound;
    public KeyCode toggleKey = KeyCode.F;
    public float battery = 100f;
    public float drainPerSecond = 3.5f;
    public float rechargePerSecond = 1.2f;

    void Update()
    {
        if (PauseMenu.IsPaused) return;

        if (Input.GetKeyDown(toggleKey) && battery > 1f)
        {
            flashlight.enabled = !flashlight.enabled;
            if (clickSound != null) clickSound.Play();
        }

        if (flashlight.enabled)
        {
            battery -= drainPerSecond * Time.deltaTime;
            if (battery <= 0f)
            {
                battery = 0f;
                flashlight.enabled = false;
            }
        }
        else
        {
            battery = Mathf.Min(100f, battery + rechargePerSecond * Time.deltaTime);
        }
    }
}
