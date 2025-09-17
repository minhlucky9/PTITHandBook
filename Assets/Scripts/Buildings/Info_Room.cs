using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Info_Room : MonoBehaviour
{

    Instance_Building myBuildingInfo;

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
        UIWeatherManager.instance.SetPanelInfoRoom(true);
    }
}
