using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DT_Aptomat : MonoBehaviour
{
    Instance_Building myBuildingInfo;

    public string DTname;
    public Transform DTpos;
    public GameObject prefab_aptomat;
    public GameObject prefab_LabelDT;

    public bool currentStateIsOn;

    public Animator[] ceilingFanAnims;
    public Light[] ceilingLights;

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
        var f = Instantiate(prefab_aptomat, UIWeatherManager.instance.Panel_Info_Room.transform, false);
        f.GetComponentInChildren<TMP_Text>().text = DTname;

        f.GetComponent<Controller_Aptomat>().setDTUIComponent(this);

        var d = Instantiate(prefab_LabelDT, UIWeatherManager.instance.PanelLabelDT.transform, false);
        d.GetComponentInChildren<TMP_Text>().text = DTname;
        d.GetComponent<Label>().SetTarget(DTpos);
    }

    public void setTurnAptomat(bool value)
    {
        currentStateIsOn = value;

        foreach (Animator a in ceilingFanAnims)
        {
            a.SetBool("isOn", value);
        }

        foreach(Light l in ceilingLights)
        {
            l.gameObject.SetActive(value);
        }
    }
}
