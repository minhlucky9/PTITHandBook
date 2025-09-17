using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DT_Sensor4Type : MonoBehaviour
{
    Instance_Building myBuildingInfo;

    public string DTname;
    public Transform DTpos;
    public GameObject prefab_sensor4type;
    public GameObject prefab_LabelDT;

    // Start is called before the first frame update
    void Start()
    {
        myBuildingInfo = GetComponent<Instance_Building>();

        myBuildingInfo.beingChoosen += setUI;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void setUI()
    {
        var f = Instantiate(prefab_sensor4type, UIWeatherManager.instance.Panel_Info_Room.transform, false);
        f.GetComponentInChildren<TMP_Text>().text = DTname;

        var d = Instantiate(prefab_LabelDT, UIWeatherManager.instance.PanelLabelDT.transform, false);
        d.GetComponentInChildren<TMP_Text>().text = DTname;
        d.GetComponent<Label>().SetTarget(DTpos);
    }
}
