using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorVisionManager : MonoBehaviour
{
    public Texture2D cursorNormal;
    public Texture2D cursorClick;
    float offset = 25f;
    void Awake()
    {
        //DontDestroyOnLoad(this);
        Cursor.SetCursor(cursorNormal, getOffset(), CursorMode.ForceSoftware);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Cursor.SetCursor(cursorClick, getOffset(), CursorMode.ForceSoftware);
        } else if(Input.GetMouseButtonUp(0))
        {
            Cursor.SetCursor(cursorNormal, getOffset(), CursorMode.ForceSoftware);
        }
    }

    public Vector3 getOffset()
    {
        return Vector3.one * offset;
    }
}
