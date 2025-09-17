using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static GlobalResponseData_Login;
public class CharacterName_Login : MonoBehaviour
{
    public TMP_InputField Name;
    public TMP_InputField Id;

    // Update is called once per frame
    void Update()
    {
        Name.text = GlobalResponseData.fullname;
        Id.text = GlobalResponseData.student_id;
    }
}
