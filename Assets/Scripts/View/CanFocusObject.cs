using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CanFocusObject : MonoBehaviour
{
    public int AreaIndex;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool readyForFocus = false;

    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            readyForFocus = true;
        }
            
    }

    private void OnMouseExit()
    {
        readyForFocus = false;
    }

    private void OnMouseUp()
    {
        if (readyForFocus)
        {
            //UIManager.instance.ChooseArea(AreaIndex);
            
        }
    }
}
