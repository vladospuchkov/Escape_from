using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public GameObject settingsPanel;
    public Slider volumeSlider;

    void Start()
    {
        Time.timeScale = 1f;
        ShowMenuCursor();
        ForceFullscreen();
        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        PlayGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            PlayGame();

        if (Input.GetKeyDown(KeyCode.Escape))
            QuitGame();
    }

    void WireButton(string objectName, UnityEngine.Events.UnityAction action)
    {
        var obj = GameObject.Find(objectName);
        if (obj == null) return;
        var button = obj.GetComponent<Button>();
        if (button == null) return;
        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;
        ForceFullscreen();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene("EscapeFromGuard_Horror");
    }

    void ForceFullscreen()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
        Screen.fullScreen = true;
    }

    public void ToggleSettings()
    {
        ShowMenuCursor();
        if (settingsPanel != null) settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void ShowMenuCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
