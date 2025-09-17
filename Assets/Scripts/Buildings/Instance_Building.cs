using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Instance_Building : MonoBehaviour
{
    public string buildingname;

    public GameObject[] Models;
    public GameObject label;
    public Transform LabelPos;

    public Transform AreaFocusPos;
    public Button ButtonArea_UI;

    public float minZoom;
    public float maxZoom;

    public enum showType
    {
        showAllOther,
        hideAllOther,
        floor,
        room,
        watermeter
    }

    public showType myShowType;

    public bool isAvailabled = true;

    // Start is called before the first frame update
    void Start()
    {
        if (label)
        {
            label.GetComponent<Label>().SetTarget(LabelPos);

            if (isAvailabled)
            {
                label.GetComponent<Button>().onClick.AddListener(delegate
                {
                    UIWeatherManager.instance.ChooseArea(this);
                });
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public event Action beingChoosen;

    public void BeingChoosen()
    {
        UIWeatherManager.instance.SetTextNameBuilding(buildingname);
        CameraRotate.instance.SetFocusPosition(AreaFocusPos);
        CameraRotate.instance.SetZoom(minZoom, maxZoom);

        if (ButtonArea_UI)
        {
            ButtonArea_UI.GetComponent<Image>().color = new Color32(52, 80, 206, 255);
            ButtonArea_UI.GetComponentInChildren<TMP_Text>().color = Color.white;
        }

        

        switch (myShowType)
        {
            case showType.showAllOther:
                foreach(Instance_Building b in FindObjectsOfType<Instance_Building>())
                {
                    if (b.myShowType == showType.floor)
                    {
                        b.SetVisible_Label(true);
                        b.SetVisible_Models(false);
                    }
                    else if (b.myShowType == showType.room)
                    {
                        b.SetVisible_Label(false);
                        b.SetVisible_Models(false);
                    }
                    else if (b.myShowType == showType.hideAllOther)
                    {
                        b.SetVisible_Label(true);
                        b.SetVisible_Models(false);
                    }
                    else 
                    {
                        b.SetVisible_Label(true);
                        b.SetVisible_Models(true);
                    }
                }
                Camera.main.clearFlags = CameraClearFlags.Skybox;
                break;

            case showType.hideAllOther:
                foreach (Instance_Building b in FindObjectsOfType<Instance_Building>())
                {
                    b.SetVisible_Label(false);
                    b.SetVisible_Models(false);
                }
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                SetVisible_Models(true);
                break;

            case showType.floor:
                foreach (Instance_Building b in FindObjectsOfType<Instance_Building>())
                {
                    b.SetVisible_Label(false);
                    b.SetVisible_Models(false);
                }

                foreach(Instance_Building r in GetComponent<Info_FloorList>().Floors)
                {
                    r.SetVisible_Label(true);
                    r.SetVisible_Models(true);
                }

                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                SetVisible_Models(true);
                break;

            case showType.room:
                foreach (Instance_Building b in FindObjectsOfType<Instance_Building>())
                {
                    if (b == GetComponent<Info_BacktoPrevious>().buildingToBack)
                    {
                        b.SetVisible_FirstModels();
                    }
                    else
                    {
                        b.SetVisible_Label(false);
                        b.SetVisible_Models(false);
                    }
                }
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                SetVisible_Models(true);
                break;
        }

        beingChoosen.Invoke();
    }

    public void BeingUnChoosen()
    {
        if (ButtonArea_UI)
        {
            ButtonArea_UI.GetComponent<Image>().color = Color.white;
            ButtonArea_UI.GetComponentInChildren<TMP_Text>().color = Color.black;
        }

    }


    void SetVisible_Label(bool value)
    {
        if (label)
        {
            label.SetActive(value);
        }
    }

    void SetVisible_Models(bool value)
    {
        if (Models.Length > 0)
        {
            for (int i = 0; i < Models.Length; i++)
            {
                Models[i].SetActive(value);
            }
        }
    }

    void SetVisible_FirstModels()
    {
        if (Models.Length > 0)
        {
            for (int i = 0; i < Models.Length; i++)
            {
                if (i == 0)
                {
                    Models[i].SetActive(true);
                }
                else
                {
                    Models[i].SetActive(false);
                }
                
            }
        }
    }
}
