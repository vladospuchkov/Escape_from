using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    public float walkSpeed = 3.2f;
    public float runSpeed = 5.4f;
    public float crouchSpeed = 1.65f;
    public float mouseSensitivity = 2.1f;
    public float gravity = -22f;
    public float jumpHeight = 1.15f;
    public Transform cameraRoot;
    public AudioSource stepSound;
    public PlayerHands hands;

    CharacterController controller;
    JetpackFlightController jetpack;
    float verticalVelocity;
    float cameraPitch;
    float stepTimer;
    float standHeight = 1.8f;
    float crouchHeight = 1.15f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        jetpack = GetComponent<JetpackFlightController>();
        standHeight = controller.height;
        if (cameraRoot == null && Camera.main != null) cameraRoot = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (PauseMenu.IsPaused || Time.timeScale == 0f) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
        cameraPitch = Mathf.Clamp(cameraPitch - mouseY, -80f, 80f);
        if (cameraRoot != null) cameraRoot.localEulerAngles = new Vector3(cameraPitch, 0, 0);

        if (jetpack != null && jetpack.IsFlyingActive)
        {
            verticalVelocity = 0f;
            return;
        }

        bool crouching = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
        controller.height = Mathf.Lerp(controller.height, crouching ? crouchHeight : standHeight, Time.deltaTime * 10f);
        controller.center = new Vector3(0, controller.height * 0.5f, 0);
        if (cameraRoot != null)
        {
            Vector3 camPos = cameraRoot.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, crouching ? 0.25f : 0.65f, Time.deltaTime * 10f);
            cameraRoot.localPosition = camPos;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        bool running = Input.GetKey(KeyCode.LeftShift) && !crouching;
        float speed = crouching ? crouchSpeed : (running ? runSpeed : walkSpeed);
        Vector3 move = transform.right * x + transform.forward * z;
        if (move.magnitude > 1f) move.Normalize();

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = -2f;
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (hands != null) hands.JumpPulse();
            }
        }

        verticalVelocity += gravity * Time.deltaTime;
        Vector3 velocity = move * speed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        if (move.magnitude > 0.15f && controller.isGrounded)
        {
            stepTimer -= Time.deltaTime * (running ? 1.8f : (crouching ? 0.55f : 1f));
            if (stepTimer <= 0f)
            {
                stepTimer = 0.55f;
                if (stepSound != null && !crouching) stepSound.Play();
            }
        }
    }
}
