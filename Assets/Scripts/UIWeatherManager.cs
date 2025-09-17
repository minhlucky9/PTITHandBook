using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIWeatherManager : MonoBehaviour
{
    public static UIWeatherManager instance;

    public TMP_Text Text_Debug_daytime;
    public Slider Slider_daytime;

    public TMP_Text Text_Time;
    public TMP_Text Text_Date;

    public TMP_Text Text_Temperature;
    public TMP_Text Text_Weather;

    public Instance_Building currentChoosenBuilding;


    public Toggle tog_autoRotate;
    public Toggle tog_autoMode;

    public GameObject autoModeOverlay;

    public GameObject PanelOverview_Allcontent;

    public GameObject PanelLabelDT;

    public GameObject Panel_Info_Dashboard;
    public TMP_Text Text_NameBuilding;
    public GameObject Panel_Info_Brief;
    public Image Panel_Info_Brief_Image;
    public GameObject Panel_FloorList;
    public GameObject Panel_BackToPrevious;
    public GameObject Panel_Info_Room;
    public GameObject Panel_Comsumpt_Electric;
    public GameObject Panel_Comsumpt_Water;


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
        Slider_daytime.onValueChanged.AddListener(delegate
        {
            DebugSliderDaytime();
        });

        tog_autoRotate.onValueChanged.AddListener(delegate
        {
            SetAutoRotate(tog_autoRotate);
        });
        tog_autoRotate.isOn = true; SetAutoRotate(tog_autoRotate);

        tog_autoMode.onValueChanged.AddListener(delegate
        {
            SetAutoMode(tog_autoMode);
        });
        tog_autoMode.isOn = false; SetAutoMode(tog_autoMode);


        ChooseArea(currentChoosenBuilding);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DebugSliderDaytime()
    {
        Text_Debug_daytime.text = "Time: " + Slider_daytime.value.ToString("0");
        EnvironmentsVisual.instance.SetLighting(Slider_daytime.value, 0);
    }

    public void debugWeather(Toggle change)
    {
        EnvironmentsVisual.instance.SetWeather(change.transform.GetSiblingIndex());
        DebugSliderDaytime();
    }


    public void SetTextTime(string time, string date)
    {
        Text_Time.text = time;
        Text_Date.text = date;
    }

    public void SetTextWeather(string temperature, string weather)
    {
        Text_Temperature.text = temperature;
        Text_Weather.text = weather;
    }


    public void ChooseArea(Instance_Building building)
    {
        if (currentChoosenBuilding) currentChoosenBuilding.BeingUnChoosen();


        SetPanelInfoDashboard(false);
        SetPanelInfoBrief(false, null, null);
        SetPanelFloorList(false);
        SetPanelInfoRoom(false);
        SetPanelBackToPrevious(false, null);
        SetPanelComsumptElectric(false);
        SetPanelComsumptWater(false);

        currentChoosenBuilding = building;
        building.BeingChoosen();

        if(building.buildingname == "Hội trường A2")
        {
            CameraRotate.instance.SetCamAngle(new Vector3(15, 150, 0.0f));
        }

        StartCoroutine(delayUpdateLayout());
    }

    IEnumerator delayUpdateLayout()
    {
        yield return new WaitForSeconds(0.1f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(PanelOverview_Allcontent.GetComponent<RectTransform>());
    }

    float currentAutoRotateSpeed;
    public void SetAutoRotate(Toggle toggle)
    {
        if (toggle.isOn)
        {
            CameraRotate.instance.autoRotateSpeed = 0.01f;
            currentAutoRotateSpeed = 0.01f;
            toggle.GetComponentInChildren<Image>().color = new Color32(35, 195, 255, 255);
        }
        else
        {
            CameraRotate.instance.autoRotateSpeed = 0;
            currentAutoRotateSpeed = 0f;
            toggle.GetComponentInChildren<Image>().color = new Color32(52, 80, 206, 255);
        }
    }

    public Instance_Building[] ListAutoBuildings;
    public Animator UI_TextFlickering;
    public void SetAutoMode(Toggle toggle)
    {
        if (toggle.isOn)
        {
            autoModeOverlay.SetActive(true);
            CameraRotate.instance.autoRotateSpeed = 0.06f;
            CameraRotate.instance.SetCamAngle(new Vector3(30, 140, 0.0f));
            autoBuildingIndex = 0;
            StartCoroutine(BuildingPatrol());
            toggle.GetComponentInChildren<Image>().color = new Color32(35, 195, 255, 255);
            UI_TextFlickering.SetBool("isOn", true);
        }
        else
        {
            autoModeOverlay.SetActive(false);
            CameraRotate.instance.autoRotateSpeed = currentAutoRotateSpeed;
            StopAllCoroutines();
            toggle.GetComponentInChildren<Image>().color = new Color32(52, 80, 206, 255);
            UI_TextFlickering.SetBool("isOn", false);
        }
    }

    public void OffAutoMode(Toggle toggle)
    {
        toggle.isOn = false;
    }

    int autoBuildingIndex = 0;
    IEnumerator BuildingPatrol()
    {
        ChooseArea(ListAutoBuildings[autoBuildingIndex]);
        if (autoBuildingIndex >= ListAutoBuildings.Length - 1)
        {
            autoBuildingIndex = 0;
        }
        else
        {
            autoBuildingIndex++;
        }


        yield return new WaitForSeconds(5);
        StartCoroutine(BuildingPatrol());
    }

    public void SetPanelInfoDashboard(bool value)
    {
        Panel_Info_Dashboard.SetActive(value);
    }

    public void SetPanelInfoBrief(bool value, Sprite img, string textinfo)
    {
        Panel_Info_Brief.SetActive(value);

        Panel_Info_Brief_Image.sprite = img;
        Panel_Info_Brief.GetComponentInChildren<TMP_Text>().text = textinfo;
    }

    public void SetPanelFloorList(bool value)
    {
        if (!value)
        {
            foreach (Transform child in Panel_FloorList.transform)
            {
                Destroy(child.gameObject);
            }
        }

        Panel_FloorList.SetActive(value);
    }

    public void SetPanelBackToPrevious(bool value, Instance_Building buildingToBack)
    {
        Panel_BackToPrevious.SetActive(value);
        if (value)
        {
            Panel_BackToPrevious.GetComponentInChildren<Button>().onClick.AddListener(() => ChooseArea(buildingToBack));
        }
        
    }

    public void SetPanelInfoRoom(bool value)
    {
        if (!value)
        {
            foreach (Transform child in Panel_Info_Room.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in PanelLabelDT.transform)
            {
                Destroy(child.gameObject);
            }
        }

        Panel_Info_Room.SetActive(value);
    }

    public void SetTextNameBuilding(string name)
    {
        Text_NameBuilding.text = name;
    }

    public void SetPanelComsumptElectric(bool value)
    {
        Panel_Comsumpt_Electric.SetActive(value);
    }

    public void SetPanelComsumptWater(bool value)
    {
        Panel_Comsumpt_Water.SetActive(value);
    }

}
