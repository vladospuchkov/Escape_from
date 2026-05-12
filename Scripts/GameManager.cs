using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Text statusText;
    public Text batteryText;
    public GameObject winPanel;
    public GameObject caughtPanel;
    public GameObject notePanel;
    public Text noteText;
    public FlashlightController flashlight;
    public AudioSource caughtSound;
    public AudioSource keySound;

    public bool HasKey { get; private set; }
    bool ended;
    float noteTimer;
    string endMessage;
    Color endOverlayColor;

    void Awake()
    {
        Instance = this;
        PauseMenu.ForceResumeState();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (caughtPanel != null) caughtPanel.SetActive(false);
        if (notePanel != null) notePanel.SetActive(false);
        HideStartupOverlays();
        NormalizeHud();
        SetStatus("Цель: поднимись на 3 этаж, возьми джетпак, выйди наружу и долети до финальной платформы. Ё / ` — пауза, R — рестарт.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) RestartLevel();
        if (flashlight != null && batteryText != null)
            batteryText.text = "Фонарик: " + Mathf.RoundToInt(flashlight.battery) + "%";
        if (noteTimer > 0f)
        {
            noteTimer -= Time.deltaTime;
            if (noteTimer <= 0f && notePanel != null) notePanel.SetActive(false);
        }
    }

    public void SetStatus(string text)
    {
        if (statusText != null) statusText.text = text;
    }

    public void PickKey()
    {
        HasKey = true;
        if (keySound != null) keySound.Play();
        SetStatus("Ключ-карта найдена. Беги к выходу.");
    }

    public void TryExit()
    {
        WinGame("ВЫХОД НАЙДЕН\nТы сбежал из комплекса");
    }

    public void WinGame(string message)
    {
        if (ended) return;
        ended = true;
        endMessage = message;
        endOverlayColor = new Color(0f, .10f, .04f, .92f);
        if (winPanel == null) winPanel = CreateEndPanel("WinPanelRuntime", message, new Color(0f, .10f, .04f, .92f));
        if (winPanel != null) winPanel.SetActive(true);
        if (winPanel != null)
        {
            var texts = winPanel.GetComponentsInChildren<Text>(true);
            foreach (var t in texts) if (t.name == "WinText" || t.name == "Win") t.text = message;
        }
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PlayerCaught()
    {
        if (ended) return;
        ended = true;
        endMessage = "ТЫ ПРОИГРАЛ\nТебя поймал наблюдатель";
        endOverlayColor = new Color(.28f, 0f, 0f, .94f);
        if (caughtSound != null) caughtSound.Play();
        caughtPanel = CreateEndPanel("CaughtPanelRuntime", "ТЫ ПРОИГРАЛ\nТебя поймал наблюдатель", new Color(.28f, 0f, 0f, .94f));
        if (caughtPanel != null) caughtPanel.SetActive(true);
        if (caughtPanel != null)
        {
            BringPanelToFront(caughtPanel);
            var texts = caughtPanel.GetComponentsInChildren<Text>(true);
            foreach (var t in texts)
                if (t.name == "CaughtText" || t.name == "Caught")
                    t.text = "ТЫ ПРОИГРАЛ\nТебя поймал наблюдатель";
        }
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnGUI()
    {
        if (!ended || string.IsNullOrEmpty(endMessage)) return;

        Color oldColor = GUI.color;
        GUI.color = endOverlayColor;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle title = new GUIStyle(GUI.skin.label);
        title.fontSize = Mathf.Clamp(Screen.height / 14, 34, 72);
        title.fontStyle = FontStyle.Bold;
        title.alignment = TextAnchor.MiddleCenter;
        title.normal.textColor = Color.white;
        GUI.Label(new Rect(40, Screen.height * .28f, Screen.width - 80, Screen.height * .28f), endMessage, title);

        GUIStyle hint = new GUIStyle(GUI.skin.label);
        hint.fontSize = Mathf.Clamp(Screen.height / 34, 18, 32);
        hint.alignment = TextAnchor.MiddleCenter;
        hint.normal.textColor = new Color(1f, 1f, 1f, .85f);
        GUI.Label(new Rect(40, Screen.height * .62f, Screen.width - 80, 60), "Нажми R, чтобы начать заново", hint);
        GUI.color = oldColor;
    }

    GameObject CreateEndPanel(string panelName, string message, Color background)
    {
        var canvasObj = new GameObject(panelName + "Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        var panel = new GameObject(panelName);
        panel.transform.SetParent(canvasObj.transform, false);
        var image = panel.AddComponent<Image>();
        image.color = background;
        var panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        var textObj = new GameObject(panelName.Contains("Caught") ? "CaughtText" : "WinText");
        textObj.transform.SetParent(panel.transform, false);
        var text = textObj.AddComponent<Text>();
        text.text = message;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 46;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        var textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.anchoredPosition = new Vector2(0f, 60f);
        textRt.sizeDelta = new Vector2(-80f, -160f);

        var hintObj = new GameObject("RestartHint");
        hintObj.transform.SetParent(panel.transform, false);
        var hint = hintObj.AddComponent<Text>();
        hint.text = "Нажми R, чтобы начать заново";
        hint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hint.fontSize = 24;
        hint.color = new Color(1f, 1f, 1f, .82f);
        hint.alignment = TextAnchor.MiddleCenter;
        var hintRt = hintObj.GetComponent<RectTransform>();
        hintRt.anchorMin = new Vector2(.5f, .5f);
        hintRt.anchorMax = new Vector2(.5f, .5f);
        hintRt.anchoredPosition = new Vector2(0f, -115f);
        hintRt.sizeDelta = new Vector2(620f, 50f);
        return panel;
    }

    void BringPanelToFront(GameObject panel)
    {
        panel.transform.SetAsLastSibling();
        var canvas = panel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = 5000;
        }
    }

    void HideStartupOverlays()
    {
        Image[] images = FindObjectsOfType<Image>(true);
        foreach (var image in images)
        {
            if (image == null || image.gameObject == null) continue;
            RectTransform rect = image.GetComponent<RectTransform>();
            if (rect == null) continue;
            bool fullscreen = rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one;
            if (!fullscreen || image.color.a < .45f) continue;
            if (image.transform.childCount == 0) continue;
            image.gameObject.SetActive(false);
        }
    }

    void NormalizeHud()
    {
        Text[] texts = FindObjectsOfType<Text>(true);
        foreach (var text in texts)
        {
            if (text == null) continue;

            if (text.name == "Controls")
            {
                text.gameObject.SetActive(false);
                continue;
            }

            if (text.name == "AMMO CLEAR")
            {
                text.fontSize = 22;
                SetTopLeftHudRect(text.rectTransform, new Vector2(18, -18), new Vector2(300, 34));
            }
            else if (text.name == "Inventory")
            {
                text.fontSize = 18;
                SetTopLeftHudRect(text.rectTransform, new Vector2(18, -58), new Vector2(320, 54));
            }
            else if (text.name == "Battery")
            {
                text.fontSize = 18;
                SetTopLeftHudRect(text.rectTransform, new Vector2(18, -112), new Vector2(280, 30));
            }
            else if (text.name == "Status")
            {
                text.fontSize = 18;
                SetTopCenterHudRect(text.rectTransform, new Vector2(170, -28), new Vector2(620, 34));
            }
        }
    }

    void SetTopLeftHudRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        if (rect == null) return;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    void SetTopCenterHudRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        if (rect == null) return;
        rect.anchorMin = new Vector2(.5f, 1f);
        rect.anchorMax = new Vector2(.5f, 1f);
        rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    public void ShowNote(string text)
    {
        if (notePanel == null || noteText == null) return;
        noteText.text = text;
        notePanel.SetActive(true);
        noteTimer = 5f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
