using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookAtCam : MonoBehaviour
{
    public Transform target;
    public Vector3 offsetLookAt;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        transform.LookAt(target);
        //offset
        transform.eulerAngles += offsetLookAt;
    }
}
