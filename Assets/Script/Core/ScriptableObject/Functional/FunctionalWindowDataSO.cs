using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FunctionType { Shop };

public class FunctionalWindowDataSO : ScriptableObject
{
    public string functionId;
    public FunctionType type;
    public string npcId;

    GameObject targetGameObject;

    public virtual void Init(GameObject target)
    {
        targetGameObject = target;
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        functionId = type.ToString() + "_" + npcId;
#endif
    }

}
