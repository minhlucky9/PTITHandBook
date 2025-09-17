using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class DayNightCycle : MonoBehaviour
{
    [Header("Tham chiếu Đèn")]
    public Light sunLight;
    public Light moonLight;

    [Header("Cài đặt Thời gian")]
    public float dayLength = 120f;
    [Range(0f, 24f)]
    public float TimeOfDay = 12f;

    [Header("Skybox / Ambient")]
    public Material daySkybox;
    public Material nightSkybox;
    public Color dayAmbientLight = Color.white * 0.6f;
    public Color nightAmbientLight = Color.black * 0.2f;

    [Header("Cường độ Ánh sáng")]
    public float sunMaxIntensity = 1f;
    public float moonMaxIntensity = 0.3f;

    [Header("UI Đồng hồ (tuỳ chọn)")]
    public Text clockText;

    void Update()
    {
        // Tự động tăng TimeOfDay khi Play
        if (Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime * (24f / dayLength);
            TimeOfDay %= 24f;
        }

        float timePercent = TimeOfDay / 24f;
        UpdateSunAndMoon(timePercent);
        UpdateSkyboxAndAmbient(timePercent);
        UpdateClockText();
    }

    void UpdateSunAndMoon(float timePercent)
    {
        // Góc xRotation cho Sun
        float xRotation = timePercent * 360f - 90f;

        // --- Sun Light ---
        if (sunLight != null)
        {
            sunLight.transform.localRotation = Quaternion.Euler(xRotation, 170f, 0f);
            float sunFullAngle = 160f;
            float sunZeroAngle = 200f;

            if (xRotation <= sunFullAngle)
            {
                sunLight.intensity = sunMaxIntensity;
            }
            else if (xRotation > sunFullAngle && xRotation < sunZeroAngle)
            {
                float t = Mathf.InverseLerp(sunFullAngle, sunZeroAngle, xRotation);
                sunLight.intensity = Mathf.Lerp(sunMaxIntensity, 0f, t);
            }
            else
            {
                sunLight.intensity = 0f;
            }

            sunLight.enabled = (sunLight.intensity > 0f);
        }

        // --- Moon Light ---
        if (moonLight != null)
        {
            // Xoay Moon đối diện Sun
            float moonAngle = xRotation + 180f;
            moonLight.transform.localRotation = Quaternion.Euler(moonAngle, 170f, 0f);

            float sunFullAngle2 = 160f;
            float sunZeroAngle2 = 200f;

            if (xRotation <= sunFullAngle2)
            {
                moonLight.intensity = 0f;
            }
            else if (xRotation > sunFullAngle2 && xRotation < sunZeroAngle2)
            {
                float t = Mathf.InverseLerp(sunFullAngle2, sunZeroAngle2, xRotation);
                moonLight.intensity = Mathf.Lerp(0f, moonMaxIntensity, t);
            }
            else
            {
                moonLight.intensity = moonMaxIntensity;
            }

            moonLight.enabled = (moonLight.intensity > 0f);
        }
    }

    void UpdateSkyboxAndAmbient(float timePercent)
    {
        bool isDay = (timePercent >= 0.25f && timePercent < 0.75f);
        if (isDay)
        {
            if (RenderSettings.skybox != daySkybox)
            {
                RenderSettings.skybox = daySkybox;
                DynamicGI.UpdateEnvironment();
            }
            RenderSettings.ambientLight = dayAmbientLight;
        }
        else
        {
            if (RenderSettings.skybox != nightSkybox)
            {
                RenderSettings.skybox = nightSkybox;
                DynamicGI.UpdateEnvironment();
            }
            RenderSettings.ambientLight = nightAmbientLight;
        }
    }

    void UpdateClockText()
    {
        if (clockText == null) return;
        int hours = Mathf.FloorToInt(TimeOfDay);
        int minutes = Mathf.FloorToInt((TimeOfDay - hours) * 60f);
        clockText.text = string.Format("{0:00}:{1:00}", hours, minutes);
    }
}
