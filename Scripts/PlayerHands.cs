using UnityEngine;

public class PlayerHands : MonoBehaviour
{
    public Transform cameraRoot;
    public Vector3 idleOffset = new Vector3(0.28f, -0.35f, 0.55f);
    public float swayAmount = 0.035f;
    public float swaySmooth = 8f;
    public float jumpAnimationAmount = 0.18f;
    private Vector3 targetLocal;
    private float jumpTimer;

    void Start()
    {
        if (cameraRoot == null && Camera.main != null) cameraRoot = Camera.main.transform;
        targetLocal = idleOffset;
    }

    void Update()
    {
        if (cameraRoot == null) return;
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");
        float jumpBob = 0f;
        if (jumpTimer > 0f)
        {
            jumpTimer -= Time.deltaTime;
            jumpBob = Mathf.Sin((1f - jumpTimer / 0.45f) * Mathf.PI) * jumpAnimationAmount;
        }
        targetLocal = idleOffset + new Vector3(-mx, -my, 0f) * swayAmount + Vector3.down * jumpBob;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocal, Time.deltaTime * swaySmooth);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(my * 2f, -mx * 2f, 0), Time.deltaTime * swaySmooth);
    }

    public void JumpPulse()
    {
        jumpTimer = 0.45f;
    }
}
