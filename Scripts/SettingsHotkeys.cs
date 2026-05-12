using UnityEngine;

public class SettingsHotkeys : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftBracket)) AudioListener.volume = Mathf.Clamp01(AudioListener.volume - 0.1f);
        if (Input.GetKeyDown(KeyCode.RightBracket)) AudioListener.volume = Mathf.Clamp01(AudioListener.volume + 0.1f);
    }
}
