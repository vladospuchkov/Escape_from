using UnityEngine;

// Enables full outside jetpack flight after the player reaches the outside finale.
[RequireComponent(typeof(CharacterController))]
public class JetpackFlightController : MonoBehaviour
{
    public bool jetpackEnabled = false;
    public Transform cameraRoot;
    public float normalFlightSpeed = 7.0f;
    public float boostFlightSpeed = 13.0f;
    public float verticalSpeed = 8.5f;
    public AudioSource jetpackLoopSound;

    CharacterController controller;
    bool messageShown;

    public bool IsFlyingActive => jetpackEnabled;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraRoot == null && Camera.main != null) cameraRoot = Camera.main.transform;
    }

    void Update()
    {
        if (!jetpackEnabled || PauseMenu.IsPaused || Time.timeScale == 0f) return;

        if (!messageShown)
        {
            messageShown = true;
            if (GameManager.Instance != null)
                GameManager.Instance.SetStatus("Джетпак активен: WASD — лететь, SPACE — вверх, CTRL — вниз, SHIFT — ускорение. Лети на жёлтую платформу.");
        }

        Transform dirRoot = cameraRoot != null ? cameraRoot : transform;
        Vector3 forward = dirRoot.forward;
        Vector3 right = dirRoot.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float speed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? boostFlightSpeed : normalFlightSpeed;

        Vector3 move = (forward * z + right * x) * speed;
        bool usingJetpack = move.sqrMagnitude > 0.01f;

        if (Input.GetKey(KeyCode.Space))
        {
            move += Vector3.up * verticalSpeed;
            usingJetpack = true;
        }
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
        {
            move += Vector3.down * verticalSpeed;
            usingJetpack = true;
        }

        // Small hover force so the player does not fall while the jetpack is active.
        if (!usingJetpack) move += Vector3.up * 0.15f;

        if (controller != null) controller.Move(move * Time.deltaTime);

        if (jetpackLoopSound != null)
        {
            if (usingJetpack && !jetpackLoopSound.isPlaying) jetpackLoopSound.Play();
            if (!usingJetpack && jetpackLoopSound.isPlaying) jetpackLoopSound.Stop();
        }
    }

    public void ActivateJetpack()
    {
        jetpackEnabled = true;
        messageShown = false;
    }
}
