using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Net.WebRequestMethods;

public class URL : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OpenURL(string url ="https://slink.ptit.edu.vn/sukien/public/qr/aDcFTzZSDoKL-zmYHhEUvKVlbujng_Uzl7jKVF_8iLWFje8v5dpoIxbsUlw0IabjElo41OILDz2vLYDjoF_NtPRigFmGEMZHU83luWn5Kr9D4GDiClJO_yuLf4a6otqs")
    {
        Application.OpenURL(url);
    }
}
