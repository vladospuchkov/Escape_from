using UnityEngine;
using UnityEngine.UI;

public class GunController : MonoBehaviour
{
    public Transform cameraRoot;
    public Text ammoText;
    public int maxAmmo = 12;
    public int ammo = 12;
    public float shootDistance = 45f;
    public float fireCooldown = 0.28f;
    public float robotDisableSeconds = 60f;
    public AudioSource shootSound;
    public AudioSource emptySound;
    public Light muzzleFlash;
    public GameObject muzzleFlashObject;

    private float nextShotTime;
    private float flashTimer;
    private Vector3 gunRootStart;
    public Transform gunRoot;

    void Start()
    {
        if (cameraRoot == null && Camera.main != null) cameraRoot = Camera.main.transform;
        if (gunRoot != null) gunRootStart = gunRoot.localPosition;
        UpdateAmmoUI();
        HideFlash();
    }

    void Update()
    {
        if (PauseMenu.IsPaused || Time.timeScale == 0f) return;
        if (Input.GetMouseButtonDown(0)) Shoot();
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (gunRoot != null) gunRoot.localPosition = gunRootStart + new Vector3(0, 0, -Mathf.Sin(flashTimer * 80f) * 0.035f);
            if (flashTimer <= 0f)
            {
                HideFlash();
                if (gunRoot != null) gunRoot.localPosition = gunRootStart;
            }
        }
    }

    void Shoot()
    {
        if (Time.time < nextShotTime) return;
        nextShotTime = Time.time + fireCooldown;
        if (ammo <= 0)
        {
            if (emptySound != null) emptySound.Play();
            if (GameManager.Instance != null) GameManager.Instance.SetStatus("Патронов нет");
            return;
        }
        ammo--;
        UpdateAmmoUI();
        if (shootSound != null) shootSound.Play();
        ShowFlash();
        if (cameraRoot == null) return;
        if (Physics.Raycast(cameraRoot.position, cameraRoot.forward, out RaycastHit hit, shootDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            GuardAI guard = hit.collider.GetComponentInParent<GuardAI>();
            if (guard != null)
            {
                guard.DisableForSeconds(robotDisableSeconds);
                return;
            }
        }
        if (GameManager.Instance != null) GameManager.Instance.SetStatus("Выстрел. Патроны: " + ammo + "/" + maxAmmo);
    }

    void ShowFlash()
    {
        if (muzzleFlash != null) { muzzleFlash.enabled = true; muzzleFlash.intensity = 8f; }
        if (muzzleFlashObject != null) muzzleFlashObject.SetActive(true);
        flashTimer = 0.09f;
    }

    void HideFlash()
    {
        if (muzzleFlash != null) muzzleFlash.enabled = false;
        if (muzzleFlashObject != null) muzzleFlashObject.SetActive(false);
    }

    public void AddAmmo(int amount)
    {
        ammo = Mathf.Clamp(ammo + amount, 0, maxAmmo);
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null) ammoText.text = "ПАТРОНЫ: " + ammo + " / " + maxAmmo;
    }
}
