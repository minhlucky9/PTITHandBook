using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PanelCollapse : MonoBehaviour
{
    Button btn;
    Transform triangle;
    RectTransform rect;

    public GameObject Panel;
    public float pos_On;
    public float pos_Off;

    bool isOn = true;


    // Start is called before the first frame update
    void Start()
    {
        btn = GetComponent<Button>();
        triangle = transform.GetChild(0);
        rect = Panel.GetComponent<RectTransform>();


        btn.onClick.AddListener(delegate
        {
            Collapse();
        });

        if (isOn)
        {
            On();
        }
        else
        {
            Off();
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    void Collapse()
    {
        if (isOn)
        {
            Off();
            isOn = false;
        }
        else
        {
            On();
            isOn = true;
        }
    }


    void On()
    {
        triangle.localEulerAngles = new Vector3(0, 0, 180);
        btn.GetComponent<Image>().color = new Color32(255, 255, 255, 90);


        //Panel.GetComponent<RectTransform>().DOLocalMoveX(pos_On, 0.3f);
        rect.anchoredPosition = new Vector3(pos_On, rect.anchoredPosition.y);

    }

    void Off()
    {
        triangle.localEulerAngles = new Vector3(0, 0, 0);
        btn.GetComponent<Image>().color = new Color32(53, 79, 206, 255);


        //Panel.GetComponent<RectTransform>().transform.DOLocalMoveX(pos_Off, 0.3f);
        rect.anchoredPosition = new Vector3(pos_Off, rect.anchoredPosition.y);
    }
}
