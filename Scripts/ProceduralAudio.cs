using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProceduralAudio : MonoBehaviour
{
    AudioSource source;
    float phase;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = 0.18f;
        source.Play();
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        float sampleRate = AudioSettings.outputSampleRate;
        for (int i = 0; i < data.Length; i += channels)
        {
            phase += 1f / sampleRate;
            float drone = Mathf.Sin(phase * 2f * Mathf.PI * 47f) * 0.10f;
            float pulse = Mathf.Sin(phase * 2f * Mathf.PI * 1.2f) * 0.05f;
            float noise = (Random.value - 0.5f) * 0.015f;
            float sample = drone + pulse + noise;
            for (int c = 0; c < channels; c++) data[i + c] = sample;
        }
    }
}
