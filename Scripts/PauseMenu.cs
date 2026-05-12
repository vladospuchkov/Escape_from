using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }
    public GameObject pausePanel;

    Slider volumeSlider;
    Slider sensitivitySlider;
    Dropdown qualityDropdown;

    void Awake()
    {
        ForceResumeState();
    }

    void Start()
    {
        ForceResumeState();
        EnsureEventSystem();
        if (pausePanel == null) CreatePausePanel();
        EnsureSettingsControls();
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        // Кнопка Ё / ` на русской и английской раскладке
        if (Input.GetKeyDown(KeyCode.BackQuote)) TogglePause();
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
        if (pausePanel != null) pausePanel.SetActive(IsPaused);
        Cursor.lockState = IsPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsPaused;
    }

    public void Resume()
    {
        if (IsPaused) TogglePause();
    }

    public static void ForceResumeState()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    public void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
        var player = FindObjectOfType<SimplePlayerController>();
        if (player != null) player.mouseSensitivity = value;
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt("QualityLevel", index);
        PlayerPrefs.Save();
    }

    void CreatePausePanel()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var cObj = new GameObject("GameCanvas");
            canvas = cObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cObj.AddComponent<GraphicRaycaster>();
        }

        pausePanel = CreatePanel(canvas.transform, "PausePanel", new Color(0f, 0f, 0f, .88f));
    }

    void EnsureSettingsControls()
    {
        if (pausePanel == null) return;
        Transform p = pausePanel.transform;

        // Если старый генератор создал только пустую паузу, добавляем нормальные элементы поверх.
        if (p.Find("PauseMenuFullRoot") != null) return;

        var root = new GameObject("PauseMenuFullRoot");
        root.transform.SetParent(p, false);
        var rt = root.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        TextUI(root.transform, "Title", "ПАУЗА", 46, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, 205), new Vector2(700, 70));
        TextUI(root.transform, "Hint", "Ё / ` — закрыть меню", 20, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, 160), new Vector2(700, 40));

        volumeSlider = SliderUI(root.transform, "VolumeSlider", "Громкость", new Vector2(0, 85), 0f, 1f, PlayerPrefs.GetFloat("MasterVolume", AudioListener.volume));
        volumeSlider.onValueChanged.AddListener(SetVolume);
        SetVolume(volumeSlider.value);

        sensitivitySlider = SliderUI(root.transform, "SensitivitySlider", "Чувствительность мыши", new Vector2(0, 15), .5f, 6f, PlayerPrefs.GetFloat("MouseSensitivity", 2.1f));
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        SetSensitivity(sensitivitySlider.value);

        qualityDropdown = DropdownUI(root.transform, "QualityDropdown", "Качество графики", new Vector2(0, -65));
        qualityDropdown.onValueChanged.AddListener(SetQuality);
        SetQuality(qualityDropdown.value);

        ButtonUI(root.transform, "ResumeButton", "ПРОДОЛЖИТЬ", new Vector2(-150, -155)).onClick.AddListener(Resume);
        ButtonUI(root.transform, "RestartButton", "РЕСТАРТ", new Vector2(150, -155)).onClick.AddListener(RestartLevel);
        ButtonUI(root.transform, "MenuButton", "ЗАНОВО", new Vector2(0, -220)).onClick.AddListener(QuitToMenu);
    }

    void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    Text TextUI(Transform parent, string name, string text, int size, Vector2 amin, Vector2 amax, Vector2 pos, Vector2 sd)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = amin;
        rt.anchorMax = amax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = sd;
        return t;
    }

    Button ButtonUI(Transform parent, string name, string text, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(.25f, .07f, .04f, .95f);
        var b = go.AddComponent<Button>();
        b.targetGraphic = img;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(250, 48);
        TextUI(go.transform, "Text", text, 21, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return b;
    }

    Slider SliderUI(Transform parent, string name, string label, Vector2 pos, float min, float max, float value)
    {
        TextUI(parent, name + "Label", label + ": " + value.ToString("0.0"), 20, new Vector2(.5f,.5f), new Vector2(.5f,.5f), pos + new Vector2(-230, 0), new Vector2(260, 40));

        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f);
        rt.anchoredPosition = pos + new Vector2(115, 0);
        rt.sizeDelta = new Vector2(310, 28);

        var slider = go.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;

        var bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(.18f,.18f,.18f,1f);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, .25f);
        bgRt.anchorMax = new Vector2(1, .75f);
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var faRt = fillArea.AddComponent<RectTransform>();
        faRt.anchorMin = new Vector2(0, .25f);
        faRt.anchorMax = new Vector2(1, .75f);
        faRt.offsetMin = new Vector2(8, 0);
        faRt.offsetMax = new Vector2(-8, 0);

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(.85f,.2f,.08f,1f);
        var fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;

        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(go.transform, false);
        var haRt = handleArea.AddComponent<RectTransform>();
        haRt.anchorMin = Vector2.zero;
        haRt.anchorMax = Vector2.one;
        haRt.offsetMin = new Vector2(10, 0);
        haRt.offsetMax = new Vector2(-10, 0);

        var handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        var handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        var hRt = handle.GetComponent<RectTransform>();
        hRt.sizeDelta = new Vector2(24, 24);

        slider.fillRect = fillRt;
        slider.handleRect = hRt;
        slider.targetGraphic = handleImg;

        var labelText = GameObject.Find(name + "Label");
        slider.onValueChanged.AddListener(v => { if (labelText != null) labelText.GetComponent<Text>().text = label + ": " + v.ToString("0.0"); });
        return slider;
    }

    Dropdown DropdownUI(Transform parent, string name, string label, Vector2 pos)
    {
        TextUI(parent, name + "Label", label, 20, new Vector2(.5f,.5f), new Vector2(.5f,.5f), pos + new Vector2(-230, 0), new Vector2(260, 40));

        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(.16f,.16f,.16f,1f);
        var dd = go.AddComponent<Dropdown>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(.5f,.5f);
        rt.anchoredPosition = pos + new Vector2(115, 0);
        rt.sizeDelta = new Vector2(310, 38);

        var labelText = TextUI(go.transform, "Label", "", 18, Vector2.zero, Vector2.one, new Vector2(10,0), new Vector2(-20,0));
        labelText.alignment = TextAnchor.MiddleLeft;
        dd.captionText = labelText;
        dd.options = new List<Dropdown.OptionData>();
        string[] names = QualitySettings.names;
        if (names == null || names.Length == 0) names = new string[] { "Low", "Medium", "High" };
        foreach (string q in names) dd.options.Add(new Dropdown.OptionData(q));
        dd.value = Mathf.Clamp(PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel()), 0, dd.options.Count - 1);
        dd.RefreshShownValue();
        return dd;
    }
}
