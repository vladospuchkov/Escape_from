using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    public Light target;
    public float minIntensity = 0.05f;
    public float maxIntensity = 1.2f;
    public float speed = 16f;
    float seed;

    void Start()
    {
        if (target == null) target = GetComponent<Light>();
        seed = Random.value * 100f;
    }

    void Update()
    {
        if (target == null) return;
        float n = Mathf.PerlinNoise(seed, Time.time * speed);
        target.intensity = Mathf.Lerp(minIntensity, maxIntensity, n);
    }
}
