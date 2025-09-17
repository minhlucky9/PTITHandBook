using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(UIAnimationController))]
public class UIAnimationControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        UIAnimationController uiAnimation = (UIAnimationController)target;

        if(!Application.isPlaying)
        {
            uiAnimation.UpdateCanvasGroup();
        }
            
    }
}
#endif