using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EnvironmentsVisual : MonoBehaviour
{
    public static EnvironmentsVisual instance;

    bool isOnDebug = false;

    public GameObject Skydome;

    public ParticleSystem vfx_rain;

    public GameObject sunLightParent;
    Light sunLight;

    public GameObject NightLights;

    public GameObject icon_Day;
    public GameObject icon_Night;

    public GameObject icon_Sunny;
    public GameObject icon_Cloud;
    public GameObject icon_Rain;

    public string apiKey = "f126e8e7994a4850b580591c999a712d"; // Your OpenWeatherMap API key
    public string city = "Hanoi"; // City for which you want to fetch weather data
    private const string apiUrl = "http://api.openweathermap.org/data/2.5/weather?q={0}&units=metric&appid={1}";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        sunLight = sunLightParent.GetComponentInChildren<Light>();

        StartCoroutine(updateTime());
        StartCoroutine(GetWeatherData());

    }

    // Update is called once per frame
    void Update()
    {
        Skydome.transform.Rotate(0, 1.5f * Time.deltaTime, 0);
    }


    public void SetDebug(bool value)
    {
        isOnDebug = value;

        if (!value)
        {
            StartCoroutine(updateTime());
            StartCoroutine(GetWeatherData());
        }
        else
        {
            UIWeatherManager.instance.Slider_daytime.value = DateTime.Now.Hour;
        }
    }



    IEnumerator updateTime()
    {
        if (!isOnDebug)
        {
            DateTime currentTime = DateTime.Now;
            UIWeatherManager.instance.SetTextTime(currentTime.ToString("HH:mm"), currentTime.ToString("dd-MM-yyyy"));

            SetLighting(currentTime.Hour,currentTime.Minute);

            yield return new WaitForSeconds(3);
            StartCoroutine(updateTime());
        }
    }

    float[] intensityValues = new float[24] {
        // Intensity values for 0 to 23 hours
        0.1f, 0.1f, 0.2f, 0.3f, 0.4f,  // 0 to 4 hours
        0.5f, 0.6f, 1.0f, 1.2f, 1.5f,  // 5 to 9 hours
        1.5f, 1.5f, 1.5f, 1.5f, 1.5f,  // 10 to 14 hours
        1.5f, 1.5f, 1.2f, 1.0f, 0.7f,  // 15 to 19 hours
        0.5f, 0.4f, 0.2f, 0.1f         // 20 to 23 hours
    };

    public Color[] fogColor;

    public void SetLighting(float hour, float minute)
    {
        float angle = -hour * 15f - (minute / 60f) * 15f;
        // Explanation:
        // - Each hour corresponds to 15 degrees of rotation (360 degrees / 24 hours)
        // - Add fraction of hour based on minutes (each minute is 1/60 of an hour)

        // Apply rotation to the sun
        sunLightParent.transform.localRotation = Quaternion.Euler(angle, 0, 0);

        if (intensityValues[Mathf.RoundToInt(hour)] > maxSunIntensity)
        {
            sunLight.intensity = maxSunIntensity;
        }
        else
        {
            sunLight.intensity = intensityValues[Mathf.RoundToInt(hour)];
        }

        if (sunLight.intensity > maxSunShadow)
        {
            sunLight.shadowStrength = maxSunShadow;
        }
        else
        {
            sunLight.shadowStrength = sunLight.intensity;
        }

        

        Color skycolor = Skydome.GetComponent<MeshRenderer>().material.color;

        switch (intensityValues[Mathf.RoundToInt(hour)])
        {
            case 0.1f:
                skycolor.a = 0;
                RenderSettings.fogColor = fogColor[0];
                break;

            case 0.2f:
                skycolor.a = 0.1f;
                RenderSettings.fogColor = fogColor[1];
                break;

            case 0.3f:
                skycolor.a = 0.2f;
                RenderSettings.fogColor = fogColor[2];
                break;

            case 0.4f:
                skycolor.a = 0.2f;
                RenderSettings.fogColor = fogColor[3];
                break;

            case 0.5f:
                skycolor.a = 0.3f;
                RenderSettings.fogColor = fogColor[4];
                break;

            case 0.6f:
                skycolor.a = sunLight.intensity;
                RenderSettings.fogColor = fogColor[5];
                break;

            case 0.7f:
                skycolor.a = sunLight.intensity;
                RenderSettings.fogColor = fogColor[6];
                break;

            case 0.8f:
                skycolor.a = sunLight.intensity;
                RenderSettings.fogColor = fogColor[7];
                break;

            case 1.0f:
                skycolor.a = 1;
                RenderSettings.fogColor = fogColor[8];
                break;

            case 1.2f:
                skycolor.a = 1;
                RenderSettings.fogColor = fogColor[9];
                break;

            case 1.5f:
                skycolor.a = 1;
                RenderSettings.fogColor = fogColor[9];
                break;
        }

        if(hour >= 6 && hour <= 18)
        {
            icon_Day.SetActive(true);
            icon_Night.SetActive(false);
            foreach (Transform l in NightLights.transform)
            {
                l.gameObject.SetActive(false);
            }
        }
        else
        {
            icon_Day.SetActive(false);
            icon_Night.SetActive(true);
            foreach (Transform l in NightLights.transform)
            {
                l.gameObject.SetActive(true);
            }
        }

        if (skycolor.a > maxSkyAlpha)
        {
            skycolor.a = maxSkyAlpha;
        }
        
        Skydome.GetComponent<MeshRenderer>().material.color = skycolor;
    }

    float maxSunIntensity = 1.5f;
    float maxSunShadow = 1;
    float maxSkyAlpha = 1;

    

    public void SetWeather(int index)
    {

        switch (index)
        {
            case 0:
                //normal
                maxSunIntensity = 1.5f;
                maxSunShadow = 1;
                maxSkyAlpha = 1;
                vfx_rain.Stop();
                icon_Sunny.SetActive(true);
                icon_Cloud.SetActive(false);
                icon_Rain.SetActive(false);
                break;

            case 1:
                //overcast
                maxSunIntensity = 1.3f;
                maxSunShadow = 1.0f;
                maxSkyAlpha = 0.6f;
                vfx_rain.Stop();
                icon_Sunny.SetActive(false);
                icon_Cloud.SetActive(true);
                icon_Rain.SetActive(false);
                break;

            case 2:
                //rain
                maxSunIntensity = 1.0f;
                maxSunShadow = 0.5f;
                maxSkyAlpha = 0;
                vfx_rain.Play();
                icon_Sunny.SetActive(false);
                icon_Cloud.SetActive(false);
                icon_Rain.SetActive(true);
                break;
        }
       
    }

    IEnumerator GetWeatherData()
    {
        string url = string.Format(apiUrl, city, apiKey);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Failed to fetch weather data: " + request.error);
            }
            else
            {
                // Parse JSON response
                string jsonResponse = request.downloadHandler.text;
                WeatherData weatherData = JsonUtility.FromJson<WeatherData>(jsonResponse);

                // Use weather data
                if (weatherData != null)
                {
                    string description = TranslateWeatherDescription(weatherData.weather[0].description);

                    UIWeatherManager.instance.SetTextWeather(weatherData.main.temp + "<sup>o</sup>C", description);
                }
                else
                {
                    Debug.LogError("Failed to parse weather data.");
                }
            }
        }
    }

    string TranslateWeatherDescription(string englishDescription)
    {
        // Add your translation logic here, mapping English descriptions to Vietnamese equivalents
        switch (englishDescription.ToLower())
        {
            case "clear sky":
                SetWeather(0);
                return "Trời quang";

            case "few clouds":
                SetWeather(0);
                return "Ít mây";

            case "scattered clouds":
                SetWeather(0);
                return "Mây rải rác";

            case "broken clouds":
                SetWeather(0);
                return "Có mây";

            case "overcast clouds":
                SetWeather(1);
                return "Trời âm u";

            case "shower rain":
                SetWeather(2);
                return "Mưa rào";

            case "rain":
                SetWeather(2);
                return "Mưa";

            case "light rain":
                SetWeather(2);
                return "Mưa nhẹ";

            case "moderate rain":
                SetWeather(2);
                return "Mưa vừa";

            case "thunderstorm":
                SetWeather(2);
                return "Dông";

            case "snow":
                SetWeather(1);
                return "Tuyết";

            case "mist":
                SetWeather(1);
                return "Sương mù";

            default:
                return englishDescription; // Return the original English description if no translation is available
        }
    }

    [Serializable]
    public class WeatherData
    {
        public MainData main;
        public Weather[] weather;
    }


    [Serializable]
    public class MainData
    {
        public float temp;
        // You can add other weather parameters like humidity, pressure, etc.
    }

    [Serializable]
    public class Weather
    {
        public string description;
        // You can add other weather parameters like icon, etc.
    }

}
