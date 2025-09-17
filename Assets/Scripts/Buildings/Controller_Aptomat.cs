using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Controller_Aptomat : MonoBehaviour
{
    Toggle m_toggle;

    DT_Aptomat DTuiComponent;

    // Start is called before the first frame update
    void Start()
    {
        m_toggle = GetComponentInChildren<Toggle>();

        m_toggle.onValueChanged.AddListener(delegate
        {
            TurnAptomat(m_toggle);
        });

        if(m_toggle.isOn== DTuiComponent.currentStateIsOn)
        {
            DTuiComponent.setTurnAptomat(DTuiComponent.currentStateIsOn);
        }
        else
        {
            m_toggle.isOn = DTuiComponent.currentStateIsOn;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setDTUIComponent(DT_Aptomat com)
    {
        DTuiComponent = com;
    }

    void TurnAptomat(Toggle change)
    {
        DTuiComponent.setTurnAptomat(change.isOn);
    }
}
