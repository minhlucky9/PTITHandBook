using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Info_BacktoPrevious : MonoBehaviour
{
    Instance_Building myBuildingInfo;

    public Instance_Building buildingToBack;

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
        UIWeatherManager.instance.SetPanelBackToPrevious(true, buildingToBack);
    }
}
