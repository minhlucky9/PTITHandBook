using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DragAndDropUIManager : MonoBehaviour
{
    public static DragAndDropUIManager instance;

    public UIAnimationController Container;

    [Header("DropBackGround")]

    //public UIAnimationController DropContainer;

    public List<Image> DropImageSlot;

    public List<Image> DropImageItems;

    [Header("DragBackGround")]

    //public UIAnimationController DragContainer;

    public List<Image> DragImageSlot;

    public List<Image> DragImageItems;

    [Header("Slider")]

    public Slider timerSlider;  

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public IEnumerator ActivateMiniGameUI()
    {
     //   LayoutRebuilder.ForceRebuildLayoutImmediate(DropContainer.GetComponent<RectTransform>());
     //   DropContainer.UpdateObjectChange();
     //   DropContainer.Activate();
        yield return new WaitForSeconds(0.3f);
      //  LayoutRebuilder.ForceRebuildLayoutImmediate(DragContainer.GetComponent<RectTransform>());
      //  DragContainer.UpdateObjectChange();
      //  DragContainer.Activate();

        Container.Activate();

    }

    public IEnumerator DeActivateMiniGameUI()
    {
       
        yield return new WaitForSeconds(0.3f);
        
        Container.Deactivate();

    }
}
