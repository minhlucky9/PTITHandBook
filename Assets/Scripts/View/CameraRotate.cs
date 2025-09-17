using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CameraRotate : MonoBehaviour
{

    public static CameraRotate instance;

    public float speedH = 4.0f;
    public float speedV = 4.0f;
    public float speedZoom = 4.0f;

    public float minZoom;
    public float maxZoom;

    public float yaw = 0.0f;
    public float pitch = 0.0f;

    public float autoRotateSpeed;

    bool isPress = false;

    public GameObject cam;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    // Use this for initialization
    void Start()
    {
        SetCamAngle(new Vector3(30, 140, 0.0f));
    }

    public void SetCamAngle(Vector3 angle)
    {
        transform.eulerAngles = angle;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)&&(!EventSystem.current.IsPointerOverGameObject()))
        {
            isPress = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isPress = false;
        }

        if (isPress)
        {
            yaw = transform.eulerAngles.y;
            yaw += speedH * Input.GetAxis("Mouse X");
            pitch -= speedV * Input.GetAxis("Mouse Y");

            //yaw = Mathf.Clamp(yaw, 110f, 260f);
            pitch = Mathf.Clamp(pitch, 0f, 55f);

            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }
        else
        {
            transform.eulerAngles += new Vector3(0.0f, autoRotateSpeed, 0.0f);
        }

        if((cam.transform.localPosition.z< minZoom) &&(Input.GetAxis("Mouse ScrollWheel") >0) && (!EventSystem.current.IsPointerOverGameObject()))
        {
            cam.transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * speedZoom);
        }
        if ((cam.transform.localPosition.z > maxZoom) && (Input.GetAxis("Mouse ScrollWheel") < 0) && (!EventSystem.current.IsPointerOverGameObject()))
        {
            cam.transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * speedZoom);
        }
    }

    public void mouseDown()
    {
        isPress = true;
    }

    public void MouseUp()
    {
        isPress = false;
    }

    public void SetFocusPosition(Transform pos)
    {
        transform.DOMove(pos.position, 0.7f);
    }

    public void SetZoom(float minzoom, float maxzoom)
    {
        minZoom = minzoom;
        maxZoom = maxzoom;
        cam.transform.DOLocalMove(new Vector3(0, 0, maxzoom), 1.2f);
    }
}