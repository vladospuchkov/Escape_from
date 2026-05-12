using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Slider volumeSlider;
    public Slider sensitivitySlider;
    public Dropdown qualityDropdown;

    void Start()
    {
        if (volumeSlider) volumeSlider.value = PlayerPrefs.GetFloat("Volume", 0.8f);
        if (sensitivitySlider) sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        if (qualityDropdown) qualityDropdown.value = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
        ApplyAll();
    }

    public void ApplyAll()
    {
        SetVolume(volumeSlider ? volumeSlider.value : PlayerPrefs.GetFloat("Volume", 0.8f));
        SetSensitivity(sensitivitySlider ? sensitivitySlider.value : PlayerPrefs.GetFloat("MouseSensitivity", 2f));
        SetQuality(qualityDropdown ? qualityDropdown.value : PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel()));
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("Volume", value);
    }

    public void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        var p = FindObjectOfType<SimplePlayerController>();
        if (p) p.mouseSensitivity = value;
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("Quality", index);
    }
}
