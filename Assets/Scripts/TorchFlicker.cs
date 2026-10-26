using UnityEngine;

public class TorchFlicker : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)]
    private float m_strength = 0.15f;

    [SerializeField, Min(0f)]
    private float m_speed = 3f;

    [SerializeField]
    private Light m_light;
    private float m_baseIntensity;
    private float m_offset;

    private void Awake()
    {
        m_baseIntensity = m_light.intensity;
        m_offset = Random.Range(0f, 1000f);
    }

    private void Update()
    {
        float noise = Mathf.Clamp01(
            Mathf.PerlinNoise(m_offset, Time.time * m_speed));

        float variation = (noise * 2f - 1f) * m_strength;
        m_light.intensity = m_baseIntensity * (1f + variation);
    }

    private void OnDisable()
    {
        if (m_light != null)
            m_light.intensity = m_baseIntensity;
    }
}
