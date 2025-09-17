using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static GlobalResponseData_Login;

public class DisplayResponseData : MonoBehaviour
{
    public TMP_Text responseText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        responseText.text = GlobalResponseData.fullname;
    }
}
