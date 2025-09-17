using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameDataSO : ScriptableObject
{
    public string minigameId;
    public string questId;
    public string stepId;
    public GameObject targetNPC;

    public virtual void Init(GameObject targetGameObject)
    {
        targetNPC = targetGameObject;
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        minigameId = questId + "_" + stepId;
#endif
    }
}
