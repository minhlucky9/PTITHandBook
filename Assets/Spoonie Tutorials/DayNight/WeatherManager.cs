using System;
using UnityEngine;
using UnityEngine.VFX;

public class WeatherManager : MonoBehaviour
{
    /// <summary>
    /// Các trạng thái thời tiết cơ bản
    /// </summary>
    public enum WeatherType { Clear, Cloudy, Rain, Storm, Snow /* Có thể thêm Snow, Fog, v.v. */ }

    [Header("Thiết lập Thời tiết")]
    public WeatherType currentWeather = WeatherType.Clear; // Thời tiết hiện tại
    public WeatherType targetWeather = WeatherType.Clear;  // Thời tiết mục tiêu (khi chuyển)
    [Tooltip("Thời gian chuyển từ current -> target (giây).")]
    public float transitionDuration = 10f;                // Độ dài chuyển tiếp (giây)

    [Header("Tham chiếu")]
    public Light sunLight;               // Tham chiếu đèn Mặt Trời (từ DayNightCycle)
    public WindZone windZone;            // Tham chiếu WindZone trong scene (nếu không cần, bỏ đi)
    public VisualEffect rainEffect;      // VFX Graph cho mưa (phải có component VisualEffect)
    public VisualEffect snowEffect;      // VFX Graph cho tuyết (nếu dùng)

    [Header("Thông số Tổng quát")]
    [Tooltip("Tốc độ spawn hạt mưa lớn nhất (RainRate tối đa của VFX).")]
    public float maxRainRate = 2000f;
    [Tooltip("Nếu không có mưa, tắt hệ thống VFX mưa.")]
    public bool useRainEffect = true;

    [Tooltip("Tốc độ tăng độ ướt khi mưa (đơn vị wetness mỗi giây).")]
    public float wetnessIncreaseRate = 0.2f;
    [Tooltip("Tốc độ giảm độ ướt khi khô (đơn vị wetness mỗi giây).")]
    public float dryRate = 0.05f;

    [Tooltip("Danh sách Material cần cập nhật độ ướt (nếu shader hỗ trợ _Wetness).")]
    public Material[] wetMaterials;      // Các material có shader hỗ trợ thuộc tính "_Wetness"

    [Header("Cài đặt ban đầu cho từng trạng thái")]
    [Range(0f, 1f)] public float clearCloudCoverage = 0f;
    [Range(0f, 1f)] public float cloudyCloudCoverage = 0.5f;
    [Range(0f, 1f)] public float rainCloudCoverage = 0.8f;
    [Range(0f, 1f)] public float stormCloudCoverage = 1f;

    [Range(0f, 1f)] public float clearRainIntensity = 0f;
    [Range(0f, 1f)] public float cloudyRainIntensity = 0f;
    [Range(0f, 1f)] public float rainRainIntensity = 0.7f;
    [Range(0f, 1f)] public float stormRainIntensity = 1f;

    [Range(0f, 1f)] public float clearWindStrength = 0.1f;
    [Range(0f, 1f)] public float cloudyWindStrength = 0.2f;
    [Range(0f, 1f)] public float rainWindStrength = 0.4f;
    [Range(0f, 1f)] public float stormWindStrength = 1f;

    // Các tham số nội bộ (đang ghi đè)
    private float startCloudCoverage, targetCloudCoverage;
    private float startRainIntensity, targetRainIntensity;
    private float startWindStrength, targetWindStrength;

    private float currentCloudCoverage, currentRainIntensity, currentWindStrength;
    private float transitionTimer = 0f;

    // Độ ướt hiện tại (global wetness) từ 0->1
    private float currentWetness = 0f;

    private void Start()
    {
        // Khởi gán các giá trị ban đầu theo currentWeather
        ApplyWeatherSettingsInstant(currentWeather);
    }

    private void Update()
    {
        // Cho mục đích test, bạn có thể bấm phím 1->4 để chuyển thời tiết
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetWeather(WeatherType.Clear);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetWeather(WeatherType.Cloudy);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetWeather(WeatherType.Rain);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetWeather(WeatherType.Storm);

        // Nếu đang chuyển trạng thái thời tiết
        if (currentWeather != targetWeather)
        {
            transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTimer / transitionDuration);

            // Nội suy các tham số chính
            currentCloudCoverage = Mathf.Lerp(startCloudCoverage, targetCloudCoverage, t);
            currentRainIntensity = Mathf.Lerp(startRainIntensity, targetRainIntensity, t);
            currentWindStrength = Mathf.Lerp(startWindStrength, targetWindStrength, t);

            // Ngoài việc chuyển, làm tăng độ ướt nếu mưa, hoặc khô dần nếu không
            if (targetRainIntensity > 0f)
            {
                currentWetness = Mathf.Min(1f, currentWetness + wetnessIncreaseRate * Time.deltaTime);
            }
            else
            {
                currentWetness = Mathf.Max(0f, currentWetness - dryRate * Time.deltaTime);
            }

            // Cập nhật hiệu ứng dựa trên tham số hiện tại
            UpdateWeatherEffects();

            // Nếu quá trình chuyển đã xong, gán current = target
            if (t >= 1f)
            {
                currentWeather = targetWeather;
                transitionTimer = 0f;
            }
        }
        else
        {
            // Nếu không chuyển state, vẫn tiếp tục tăng/giảm wetness
            if (currentRainIntensity > 0f)
            {
                currentWetness = Mathf.Min(1f, currentWetness + wetnessIncreaseRate * Time.deltaTime);
            }
            else
            {
                currentWetness = Mathf.Max(0f, currentWetness - dryRate * Time.deltaTime);
            }

            UpdateWeatherEffects();
        }
    }

    /// <summary>
    /// Thiết lập ngay lập tức các tham số dựa theo một WeatherType (không nội suy)
    /// </summary>
    /// <param name="w">Loại thời tiết</param>
    void ApplyWeatherSettingsInstant(WeatherType w)
    {
        switch (w)
        {
            case WeatherType.Clear:
                currentCloudCoverage = targetCloudCoverage = clearCloudCoverage;
                currentRainIntensity = targetRainIntensity = clearRainIntensity;
                currentWindStrength = targetWindStrength = clearWindStrength;
                break;
            case WeatherType.Cloudy:
                currentCloudCoverage = targetCloudCoverage = cloudyCloudCoverage;
                currentRainIntensity = targetRainIntensity = cloudyRainIntensity;
                currentWindStrength = targetWindStrength = cloudyWindStrength;
                break;
            case WeatherType.Rain:
                currentCloudCoverage = targetCloudCoverage = rainCloudCoverage;
                currentRainIntensity = targetRainIntensity = rainRainIntensity;
                currentWindStrength = targetWindStrength = rainWindStrength;
                break;
            case WeatherType.Storm:
                currentCloudCoverage = targetCloudCoverage = stormCloudCoverage;
                currentRainIntensity = targetRainIntensity = stormRainIntensity;
                currentWindStrength = targetWindStrength = stormWindStrength;
                break;
                // Sau này có thể add thêm case cho Snow, Fog...
        }

        currentWeather = w;
        transitionTimer = 0f;
        UpdateWeatherEffects();
    }

    /// <summary>
    /// Bắt đầu chuyển từ currentWeather sang newWeather (có nội suy)
    /// </summary>
    /// <param name="newWeather">WeatherType mong muốn</param>
    public void SetWeather(WeatherType newWeather)
    {
        if (newWeather == targetWeather) return;

        // Ghi lại giá trị bắt đầu
        startCloudCoverage = currentCloudCoverage;
        startRainIntensity = currentRainIntensity;
        startWindStrength = currentWindStrength;

        // Gán giá trị đích dựa trên newWeather
        targetWeather = newWeather;
        switch (newWeather)
        {
            case WeatherType.Clear:
                targetCloudCoverage = clearCloudCoverage;
                targetRainIntensity = clearRainIntensity;
                targetWindStrength = clearWindStrength;
                break;
            case WeatherType.Cloudy:
                targetCloudCoverage = cloudyCloudCoverage;
                targetRainIntensity = cloudyRainIntensity;
                targetWindStrength = cloudyWindStrength;
                break;
            case WeatherType.Rain:
                targetCloudCoverage = rainCloudCoverage;
                targetRainIntensity = rainRainIntensity;
                targetWindStrength = rainWindStrength;
                break;
            case WeatherType.Storm:
                targetCloudCoverage = stormCloudCoverage;
                targetRainIntensity = stormRainIntensity;
                targetWindStrength = stormWindStrength;
                break;
        }

        transitionTimer = 0f;
    }

    /// <summary>
    /// Cập nhật hiệu ứng trong scene theo các tham số hiện tại:
    ///   - Điều chỉnh ánh sáng mặt trời (sunLight) theo độ nhiều mây
    ///   - Điều khiển VisualEffect mưa/snow
    ///   - Điều khiển WindZone
    ///   - Cập nhật wetness cho các material
    /// </summary>
    void UpdateWeatherEffects()
    {
        // 1. Điều chỉnh ánh sáng mặt trời dựa trên độ phủ mây (cloud coverage)
        if (sunLight != null)
        {
            // Ví dụ: mây 0% => ánh sáng full; mây 100% => giảm 70% ánh sáng
            float factor = 1f - currentCloudCoverage * 0.7f;
            sunLight.intensity = Mathf.Clamp01(sunLight.intensity * factor);
            // Hoặc nếu muốn ánh sáng trả về giá trị gốc: sunLight.intensity = maxSunIntensity * factor;
        }

        // 2. Rain VFX
        if (useRainEffect && rainEffect != null)
        {
            if (currentRainIntensity > 0f)
            {
                rainEffect.gameObject.SetActive(true);
                // Set spawn rate trong VFX Graph (giả sử parameter exposed là "RainRate")
                rainEffect.SetFloat("RainRate", currentRainIntensity * maxRainRate);
            }
            else
            {
                // Nếu không mưa, tắt VFX hoặc đặt rate = 0
                rainEffect.SetFloat("RainRate", 0f);
                // Có thể áp dụng: rainEffect.gameObject.SetActive(false);
            }
        }

        // 3. Snow VFX (nếu có)
        if (snowEffect != null)
        {
            // Ở ví dụ này, ta không khởi tạo state cho tuyết, nhưng bạn có thể copy tương tự rainEffect
            bool usingSnow = (targetWeather == WeatherType.Snow); // nếu thêm trạng thái Snow
            snowEffect.gameObject.SetActive(usingSnow);
            if (usingSnow)
            {
                // snowEffect.SetFloat("SnowRate", currentSnowIntensity * maxSnowRate);
            }
        }

        // 4. Gió: Set cho WindZone trong scene
        if (windZone != null)
        {
            windZone.windMain = currentWindStrength * windZone.windMain;
            // hoặc windZone.windMain = currentWindStrength * maxWindValue (nếu bạn định nghĩa maxWindValue)
        }

        // 5. Cập nhật độ ướt cho các material (nếu có)
        foreach (var mat in wetMaterials)
        {
            if (mat != null)
                mat.SetFloat("_Wetness", currentWetness);
        }

        // 6. (Tuỳ chọn) Cập nhật skybox procedural nếu muốn điều chỉnh đậm nhạt mây
        // Ví dụ: nếu skybox procedural có property "_CloudCoverage"
        // Shader.SetGlobalFloat("_CloudCoverage", currentCloudCoverage);
    }
}
