using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Info_FloorList : MonoBehaviour
{
    Instance_Building myBuildingInfo;

    public Instance_Building[] Floors;

    public GameObject UI_Floor_Item_Prefab;

    private void Awake()
    {
        myBuildingInfo = GetComponent<Instance_Building>();

        myBuildingInfo.beingChoosen += setUI;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void setUI()
    {
        UIWeatherManager.instance.SetPanelFloorList(true);

        for (int i = 0; i < Floors.Length; i++)
        {
            var f = Instantiate(UI_Floor_Item_Prefab, UIWeatherManager.instance.Panel_FloorList.transform, false);
            f.GetComponentInChildren<TMP_Text>().text = Floors[i].buildingname;

            

            if (!Floors[i].isAvailabled)
            {
                f.GetComponent<Button>().interactable = false;
            }
            else
            {
                int temp = i;
                f.GetComponent<Button>().onClick.AddListener(() => UIWeatherManager.instance.ChooseArea(Floors[temp]));

                if (Floors[i].GetComponent<DT_Aptomat>())
                {
                    Floors[i].GetComponent<DT_Aptomat>().setTurnAptomat(Floors[i].GetComponent<DT_Aptomat>().currentStateIsOn);
                }
            }
        }
    }

    
}
