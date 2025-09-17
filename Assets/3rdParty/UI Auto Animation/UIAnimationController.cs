using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(UIAutoAnimation), typeof(CanvasGroup))]
public class UIAnimationController : MonoBehaviour
{
    public bool isActive = false;
    CanvasGroup canvasGroup;
    UIAutoAnimation uIAutoAnimation;
    public UnityAction OnActivate;
    public UnityAction OnDeactivate;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        uIAutoAnimation = GetComponent<UIAutoAnimation>();
        UpdateCanvasGroup();
    }

    public void UpdateObjectChange()
    {
        uIAutoAnimation.SetAllEntranceState();
        uIAutoAnimation.GetComponentsList();
        if (!isActive)
        {
            uIAutoAnimation.SetAllExitState();
        } 
    }

    public void Activate()
    {
        if (!isActive)
        {
            CancelInvoke();
            
            isActive = true;
            UpdateCanvasGroup();

            //run animation
            uIAutoAnimation.EntranceAnimation();
            //
            OnActivate?.Invoke();
        }
    }

    public void Deactivate()
    {
        if (isActive)
        {
            CancelInvoke();
            isActive = false;

            //run animation
            uIAutoAnimation.ExitAnimation();
            
            Invoke("UpdateCanvasGroup", 0.5f);
            //
            OnDeactivate?.Invoke();
        }
    }

    public void OnValidate()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        uIAutoAnimation = GetComponent<UIAutoAnimation>();
    }

    public void UpdateCanvasGroup()
    {
        canvasGroup.alpha = isActive ? 1 : 0;
        canvasGroup.interactable = isActive;
        canvasGroup.blocksRaycasts = isActive;
    }
}
